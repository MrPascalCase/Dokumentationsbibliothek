using ImageSearch.Services;
using Microsoft.AspNetCore.Components;

namespace ImageSearch.Components;

public partial class ImageDetail : ComponentBase
{
    [Parameter]
    public required string ImageId { get; set; } = string.Empty;

    [Inject]
    public required SearchService SearchService { get; set; }

    private string AspectRatioStyle => _result != null ? $"aspect-ratio: {_result.Width} / {_result.Height};" : "aspect-ratio: 1 / 1;";
    private string GetImageSize => _result == null ? "noch nicht bekannt" : $"{_result.Width}x{_result.Height}";

    private string _imageSrc = "./placeholder";
    private readonly string _imageAlt = "Bild wird geladen...";
    private Image? _result;

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrEmpty(ImageId)) throw new NullReferenceException(nameof(ImageId));

        _result = await SearchService.LoadImage(ImageId);
        if (_result != null)
        {
            _imageSrc = _result.ImageUrl;
            StateHasChanged();
        }
    }
}