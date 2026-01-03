using ImageSearch.Services;
using Microsoft.AspNetCore.Components;

namespace ImageSearch.Components;

public partial class ImageDetail : ComponentBase
{
    [Parameter]
    public required string ImageId { get; set; } = string.Empty;
    
    [Inject]
    public required SearchService SearchService { get; set; }

    private string Size => _result == null ? string.Empty : $"width: {_result.Width}px; height: {_result.Height}px";
    private string _imageSrc = "./placeholder";
    private readonly string _imageAlt = "Bild wird geladen...";
    private Image? _result;
    
    string AspectRatioStyle =>
        (_result != null)
            ? $"aspect-ratio: {_result.Width} / {_result.Height};"
            : "aspect-ratio: 1 / 1;"; // default square placeholder

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
    
    private string GetImageSize()
    {
        return _result == null ? string.Empty : $"({_result.Width}x{_result.Height}) ";
    }
}