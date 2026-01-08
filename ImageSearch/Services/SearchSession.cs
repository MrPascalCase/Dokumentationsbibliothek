using System.Collections;
using System.Diagnostics;

namespace ImageSearch.Services;

public class SearchSession : IReadOnlyList<string>
{
    public int SearchDelayMs { get; init; } = 1000;
    public int ApiPaging { get; init; } = 25;
    public ImageQuery? CurrentQuery { get; private set; }
    public int? TotalImageCount { get; private set; }
    public TimeSpan? TimeToSearchCount { get; private set; }
    public int Count => _images.Count;
    public string this[int index] => _images[index];
    public event Action<SearchResultsAddedArgs>? OnResultsAdded;
    public event Action? OnQueryChanged;

    private readonly List<string> _images = new();
    private readonly SearchService _service;
    private readonly ILogger<SearchSession>? _logger;
    private CancellationTokenSource? _debounceCts;
    private int _fetchGeneration;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public SearchSession(SearchService service, ILogger<SearchSession>? logger = null)
    {
        if (service == null) throw new ArgumentNullException(nameof(service));

        _service = service;
        _logger = logger;

        logger?.LogTrace($"{nameof(SearchSession)} created.");
    }

    public async Task SetQuery(ImageQuery? query, bool debounce = true)
    {
        if (CurrentQuery == query) return;

        CurrentQuery = null;
        TotalImageCount = null;
        TimeToSearchCount = null;
        _fetchGeneration = 0;
        _images.Clear();

        if (query == null)
        {
            if (OnQueryChanged?.GetInvocationList().Any() ?? false) OnQueryChanged.Invoke();
            return;
        }


        if (debounce)
        {
            bool searchRequired = await Debounce();
            if (!searchRequired) return;
        }

        CurrentQuery = query;
        if (OnQueryChanged?.GetInvocationList().Any() ?? false) OnQueryChanged.Invoke();

        Stopwatch sw = Stopwatch.StartNew();
        TotalImageCount = await _service.Count(query);
        sw.Stop();
        TimeToSearchCount = sw.Elapsed;

        if (OnQueryChanged?.GetInvocationList().Any() ?? false) OnQueryChanged.Invoke();

        await FetchData(1);
    }

    /// <param name="fetchGeneration">
    ///     Indicates how many times FetchData was already called. This way we can ensure that
    ///     overlapping async calls load exactly the amount of data required.
    /// </param>
    public async Task FetchData(int? fetchGeneration)
    {
        if (CurrentQuery == null) return;
        if (_images.Count == TotalImageCount) return;

        if (_images.Count > TotalImageCount)
        {
            _logger?.LogWarning($"Count of the result-list ({_images.Count}) is larger than the total count provided by the API ({TotalImageCount}).");
        }

        bool didAcquireLock = await _semaphore.WaitAsync(500);
        if (!didAcquireLock)
        {
            _logger?.LogWarning("Failed to acquire lock during the allocated time.");
            return;
        }

        try
        {
            if (fetchGeneration != null && fetchGeneration <= _fetchGeneration)
            {
                _logger?.LogTrace(
                    $"Requesting to load results of generation {fetchGeneration}. The request is stale. We are at generation {_fetchGeneration}.");
                return;
            }

            _logger?.LogTrace($"Requesting results {Count} to {Count + ApiPaging} for '{CurrentQuery}'.");
            ImageIdCollection ids = await _service.LoadIds(CurrentQuery, Count, ApiPaging);
            _images.AddRange(ids);
            _fetchGeneration++;
        }
        finally
        {
            _semaphore.Release();
        }

        if (OnResultsAdded?.GetInvocationList().Any() ?? false)
        {
            OnResultsAdded?.Invoke(new SearchResultsAddedArgs { LatestFetchGeneration = _fetchGeneration, });
        }
    }

    public IEnumerator<string> GetEnumerator()
    {
        return _images.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private async Task<bool> Debounce()
    {
        if (_debounceCts != null)
        {
            await _debounceCts.CancelAsync();
            _debounceCts.Dispose();
            _debounceCts = null;
        }

        _debounceCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(SearchDelayMs, _debounceCts.Token);
            if (_debounceCts.IsCancellationRequested)
            {
                _logger?.LogInformation("Search is cancelled.");
                return false;
            }
        }
        catch (TaskCanceledException)
        {
            _logger?.LogInformation("Search is cancelled.");
            return false;
        }

        _logger?.LogInformation("Search must be done.");
        return true;
    }
}