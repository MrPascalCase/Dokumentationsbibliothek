using ImageSearch.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ImageSearch.Components;

public partial class Results : ComponentBase
{
    [Inject]
    public required SearchService SearchService { get; set; }

    [Inject]
    public required IJSRuntime JsRuntime { get; set; }

    [Inject]
    public required ILogger<Results> Logger { get; set; }

    [Parameter]
    public string? InitialSearchText { get; set; }

    [Parameter]
    public EventCallback<SearchInitializedArgument> OnSearchInitialized { get; set; }

    private const int ApiPaging = 25;
    private const int SearchDelayMs = 300;

    private int _imagesPerRow = 4;
    private string _searchText = string.Empty;
    private int _indexNextElement = 0;
    private List<string> _results = new();
    private int? _resultCount = null;
    private int? _width = null;
    private IJSObjectReference? _module;
    private ElementReference? _endMarker;
    private bool _didRegisterObserver = false;
    private bool _isFetching = false;
    private CancellationTokenSource? _debounceCts;

    public async Task UpdateSearchEventually(string text)
    {
        // Cancel the previous pending action
        if (_debounceCts != null)
        {
            await _debounceCts.CancelAsync();
            _debounceCts.Dispose();
            _debounceCts = null;
        }

        _debounceCts = new CancellationTokenSource();

        try
        {
            // Wait for stability
            await Task.Delay(SearchDelayMs, _debounceCts.Token);

            // If we got here, no new calls occurred
            await UpdateSearch(text);
        }
        catch (TaskCanceledException)
        {
            // Expected when new input arrives
        }
    }

    public async Task FetchMore()
    {
        if (_isFetching) return;

        _isFetching = true;

        ImageQuery query = BuildQuery();
        Logger.LogInformation($"Requesting results {_indexNextElement} to {_indexNextElement + ApiPaging} for '{_searchText}'.");
        ImageIdCollection ids = await SearchService.LoadIds(query, _indexNextElement, ApiPaging);

        _indexNextElement += ApiPaging;
        _results.AddRange(ids);

        _isFetching = false;
        StateHasChanged();
    }

    public async Task UpdateSearch(string text)
    {
        Logger.LogInformation($"Setting '{nameof(_searchText)}' from '{_searchText}' to '{text}'.");
        _searchText = text;
        _results = new List<string>();
        _indexNextElement = 0;

        ImageQuery query = BuildQuery();
        int? count = await SearchService.Count(query);
        _resultCount = count;

        await OnSearchInitialized.InvokeAsync(new SearchInitializedArgument { Query = query, ResultCount = count, });
        if (!string.IsNullOrWhiteSpace(text))
        {
            await FetchMore();
        }
        else
        {
            // StateHasChanges already called in FetchMore (but also needed when we change the search to String.Empty)
            StateHasChanged();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _searchText = InitialSearchText ?? string.Empty;
        await FetchMore();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        DotNetObjectReference<Results> objectRef = DotNetObjectReference.Create(this);

        if (_endMarker != null && !_didRegisterObserver)
        {
            _module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./observer.js");
            await _module.InvokeVoidAsync("registerObserver", objectRef, _endMarker);
            _didRegisterObserver = true;
        }

        if (firstRender)
        {
            // initial value
            _width = await JsRuntime.InvokeAsync<int>("windowSize.getWidth");

            // subscribe to resize events
            await JsRuntime.InvokeVoidAsync("windowSize.registerResizeCallback", objectRef);
        }
    }

    private ImageQuery BuildQuery()
    {
        return ImageQuery.FromText(_searchText);
    }

    [JSInvokable]
    public async Task OnIntersection()
    {
        if (!_isFetching) await FetchMore();
    }

    [JSInvokable]
    public void OnBrowserResize(int width)
    {
        if (width != _width)
        {
            Logger.LogInformation($"Width changed to form {_width} to {width}.");
            _width = width;

            int imageMargin = 5; // see ImagePreview.razor.css
            int imagesPerRow = ImageAspectRatio.SuggestNumberOfImages(ImagePreview.Height, 2 * imageMargin, width);
            if (imagesPerRow != _imagesPerRow)
            {
                Logger.LogInformation($"Number of images per row changes from {_imagesPerRow} to {imagesPerRow}.");
                _imagesPerRow = imagesPerRow;

                InvokeAsync(StateHasChanged);
            }
            else
            {
                Logger.LogInformation($"Number of images per row ({_imagesPerRow}) does not change.");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_module != null) await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Blazor Server: ignore if circuit already gone
        }
    }
}