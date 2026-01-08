using ImageSearch.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ImageSearch.Components;

public partial class Results : ComponentBase
{
    [Inject]
    public required SearchSession SearchSession { get; set; }

    [Inject]
    public required IJSRuntime JsRuntime { get; set; }

    [Inject]
    public required ILogger<Results> Logger { get; set; }

    [Parameter]
    public EventCallback<string> OnImageSelected { get; set; }

    private int _imagesPerRow = 4;
    private int? _width = null;
    private IJSObjectReference? _module;
    private ElementReference? _endMarker;
    private bool _didInitializeJs = false;
    private DotNetObjectReference<Results>? _objectRef;
    private int _nextFetchgeneration = 0;

    protected override Task OnParametersSetAsync()
    {
        SearchSession.OnResultsAdded += OnResultsAdded;
        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_didInitializeJs)
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
            await module.InvokeVoidAsync("init", _objectRef);
            _didInitializeJs = true;
        }

        if (_module != null && _didInitializeJs && _endMarker != null)
        {
            await _module.InvokeVoidAsync("observe", _endMarker, _nextFetchgeneration);
        }
    }

    public async Task FetchMore()
    {
        await SearchSession.FetchData(null);
    }

    [JSInvokable]
    public async Task OnIntersection(int currentFetchGeneration)
    {
        await SearchSession.FetchData(currentFetchGeneration);
    }

    [JSInvokable]
    public async Task OnBrowserResize(int width)
    {
        if (width != _width)
        {
            Logger.LogTrace($"Width changed to form {_width} to {width}.");
            _width = width;

            int imageMargin = 5; // see ImagePreview.razor.css
            int imagesPerRow = ImageAspectRatio.SuggestNumberOfImages(ImagePreview.Height, 2 * imageMargin, width);
            if (imagesPerRow != _imagesPerRow)
            {
                Logger.LogTrace($"Number of images per row changes from {_imagesPerRow} to {imagesPerRow}.");
                _imagesPerRow = imagesPerRow;
                await InvokeAsync(StateHasChanged);
            }

            Logger.LogTrace($"Number of images per row ({_imagesPerRow}) does not change.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        SearchSession.OnResultsAdded -= OnResultsAdded;
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

    private void OnResultsAdded(SearchResultsAddedArgs args)
    {
        if (args == null) throw new ArgumentNullException(nameof(args));
        _nextFetchgeneration = args.LatestFetchGeneration + 1;
        InvokeAsync(StateHasChanged);
    }
}