using Microsoft.AspNetCore.Components;

namespace ImageSearch.Pages;

public partial class Redirect : ComponentBase
{
    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    protected override void OnInitialized()
    {
        string baseUri = NavigationManager.BaseUri;
        NavigationManager.NavigateTo(baseUri + "search");
        base.OnInitialized();
    }
}