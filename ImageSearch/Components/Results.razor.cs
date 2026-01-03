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
    public ImageQuery? InitialQuery { get; set; }

    [Parameter]
    public EventCallback<SearchInitializedArgument> OnSearchInitialized { get; set; }

    [Parameter]
    public EventCallback<string> OnImageSelected { get; set; }

    private const int ApiPaging = 25;
    private const int SearchDelayMs = 300;

    private int _imagesPerRow = 4;
    private ImageQuery? _query;
    private int _indexNextElement = 0;
    private List<string> _results = new();
    private int? _resultCount = null;
    private int? _width = null;
    private IJSObjectReference? _module;
    private ElementReference? _endMarker;
    private bool _didInitializeJs = false;
    private bool _isFetching = false;
    private CancellationTokenSource? _debounceCts;
    private DotNetObjectReference<Results>? _objectRef;

    public async Task UpdateSearchEventually(ImageQuery? query)
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
            await UpdateSearch(query);
        }
        catch (TaskCanceledException)
        {
            // Expected when new input arrives
        }
    }

    public async Task FetchMore()
    {
        if (_isFetching) return;
        if (_query == null) return;

        _isFetching = true;

        Logger.LogInformation($"Requesting results {_indexNextElement} to {_indexNextElement + ApiPaging} for '{_query}'.");
        ImageIdCollection ids = await SearchService.LoadIds(_query, _indexNextElement, ApiPaging);

        _indexNextElement += ApiPaging;

        if (_results.Union(ids).Any())
        {
            foreach (string duplicate in _results.Union(ids))
            {
                Logger.LogWarning($"Image with Id='{duplicate}' is already in the results list.");
            }
        }

        _results.AddRange(ids.Except(_results));

        _isFetching = false;
        StateHasChanged();
    }

    public async Task UpdateSearch(ImageQuery? query)
    {
        Logger.LogInformation($"Setting '{nameof(_query)}' from '{_query}' to '{query}'.");
        _query = query;
        _results = new List<string>();
        _indexNextElement = 0;

        int? count = query == null ? 0 : await SearchService.Count(query);
        _resultCount = count;

        if (query != null)
        {
            await OnSearchInitialized.InvokeAsync(new SearchInitializedArgument { Query = query, ResultCount = count, });
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
        _query = InitialQuery;
        await FetchMore();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_endMarker != null && !_didInitializeJs)
        {
            _module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Results.razor.js");
            await Task.WhenAll(
                HandleInitialResizeAsync(_module),
                InitializeJsAsync(_module));
        }

        async Task HandleInitialResizeAsync(IJSObjectReference module)
        {
            int width = await module.InvokeAsync<int>("getWidth");
            await OnBrowserResize(width);
        }

        async Task InitializeJsAsync(IJSObjectReference module)
        {
            _objectRef = DotNetObjectReference.Create(this);
            await module.InvokeVoidAsync("init", _objectRef, _endMarker);
            _didInitializeJs = true;
        }
    }

    [JSInvokable]
    public async Task OnIntersection()
    {
        if (!_isFetching) await FetchMore();
    }

    [JSInvokable]
    public async Task OnBrowserResize(int width)
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

                await InvokeAsync(StateHasChanged);
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
            if (_module != null)
            {
                await _module.InvokeVoidAsync("dispose");
                await _module.DisposeAsync();
                _module = null;
            }
        }
        catch (JSDisconnectedException)
        {
            // Blazor Server: ignore if circuit already gone
        }
        
        if (_objectRef != null)
        {
            _objectRef.Dispose();
            _objectRef = null;
        }
    }
}