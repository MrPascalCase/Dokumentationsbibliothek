using ImageSearch.Services;
using ImageSearch.Services.Dto;
using Microsoft.AspNetCore.Components;

namespace ImageSearch.Pages;

// ReSharper disable once ClassNeverInstantiated.Global
public partial class Search : ComponentBase, IDisposable
{
    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required SearchSession SearchSession { get; set; }
    
    [Inject]
    public required ILogger<Search> Logger { get; set; }

    private string _inputText = string.Empty;
    private string? _overlayImageId;

    protected override async Task OnParametersSetAsync()
    {
        SearchSession.OnQueryChanged += OnQueryChanged;
        Uri uri = new(NavigationManager.Uri);
        ImageQuery? query = ImageQuery.ParseUrlQuery(uri.Query);
        _inputText = query?.ToCanonicalSearchText() ?? string.Empty;
        await SearchSession.SetQuery(query);
    }

    private async Task QueueSearch(bool forced)
    {
        string baseUri = NavigationManager.BaseUri;
        ImageQuery? query = ImageQuery.ParseSearchText(_inputText);
        NavigationManager.NavigateTo(baseUri + "search" + (query?.ToUrl() ?? string.Empty));
        await SearchSession.SetQuery(query, !forced);
    }

    private async Task OnImageSelected(string imageId)
    {
        _overlayImageId = imageId;
        await InvokeAsync(StateHasChanged);
    }

    private void OnQueryChanged()
    {
        ImageQuery? inputAsQuery = ImageQuery.ParseSearchText(_inputText);
        if (inputAsQuery != SearchSession.CurrentQuery)
        {
            Logger.LogInformation($"External change of the the query form '{inputAsQuery}' to '{SearchSession.CurrentQuery}'.");
            _inputText = SearchSession.CurrentQuery?.ToCanonicalSearchText() ?? string.Empty;
            InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        SearchSession.OnQueryChanged -= OnQueryChanged;
    }
}