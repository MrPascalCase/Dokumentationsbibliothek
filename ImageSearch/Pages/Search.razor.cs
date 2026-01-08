using ImageSearch.Services;
using Microsoft.AspNetCore.Components;

namespace ImageSearch.Pages;

// ReSharper disable once ClassNeverInstantiated.Global
public partial class Search : ComponentBase
{
    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required SearchSession SearchSession { get; set; }

    private string _inputText = string.Empty;
    private string? _overlayImageId;

    protected override async Task OnParametersSetAsync()
    {
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
        await SearchSession.SetQuery(query, forced);
    }

    private async Task OnImageSelected(string imageId)
    {
        _overlayImageId = imageId;
        await InvokeAsync(StateHasChanged);
    }
}