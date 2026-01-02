using ImageSearch.Services;
using Microsoft.AspNetCore.Components;

namespace ImageSearch.Pages;

public partial class Image : ComponentBase
{
    [SupplyParameterFromQuery(Name = "id")]
    public string ImageId { get; set; } = string.Empty;

    [Inject]
    public required SearchService SearchService { get; set; }

    private string _imageSrc = "./placeholder";
    private readonly string _imageAlt = "Bild wird geladen...";
    private Services.Image? _result;

    protected override async Task OnInitializedAsync()
    {
        _result = await SearchService.LoadImage(ImageId);
        _imageSrc = _result.ImageUrl;
        StateHasChanged();
    }
}