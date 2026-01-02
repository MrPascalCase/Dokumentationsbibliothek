using ImageSearch.Components;
using ImageSearch.Services;
using Microsoft.AspNetCore.Components;

namespace ImageSearch.Pages;

public partial class Search : ComponentBase
{
    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    private Results? _results;
    private SearchInitializedArgument? _search;
    private ImageQuery? _query;
    private string _inputText = string.Empty;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        Uri uri = new(NavigationManager.Uri);
        ImageQuery? query = ImageQuery.ParseUrlQuery(uri.Query);

        _inputText = query?.ToCanonicalSearchText() ?? string.Empty;
        _query = query;
    }

    private async Task OnInput()
    {
        string baseUri = NavigationManager.BaseUri;
        _query = ImageQuery.ParseSearchText(_inputText);

        if (_query == null)
        {
            NavigationManager.NavigateTo(baseUri + "search");
        }
        else
        {
            NavigationManager.NavigateTo(baseUri + "search" + _query.ToUrl());
        }

        if (_results != null) await _results.UpdateSearchEventually(_query);
    }

    private void SearchInitialized(SearchInitializedArgument argument)
    {
        _search = argument;
    }
}