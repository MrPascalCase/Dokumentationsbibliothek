using ImageSearch.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ImageSearch.Components;

public partial class ImagePreview : ComponentBase
{
    public const int Height = 250;

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required SearchService SearchService { get; set; }

    [Parameter]
    public required string ImageId { get; set; }

    [Parameter]
    public ImageQuery? Query { get; set; }

    private string Title => string.IsNullOrWhiteSpace(_result?.Caption) ? "<Titel nicht definiert>" : _result.Caption;
    private string Jahr => string.IsNullOrWhiteSpace(_result?.Year) ? "<Jahr nicht definiert>" : _result.Year;
    private string Size => $"width: {_width}px; height: {Height}px";
    private string DetailUrl => NavigationManager.BaseUri + $"image?id={ImageId}";

    private int _width = Height;
    private string _imageSrc = "./placeholder";
    private string _imageAlt = "Bild wird geladen...";
    private Image? _result;

    protected override async Task OnInitializedAsync()
    {
        _result = await SearchService.LoadImage(ImageId);
        if (_result == null) return;

        double aspectRatio = (double)_result.Width / _result.Height;
        _width = (int)Math.Round(aspectRatio * Height);
        _imageSrc = _result.GetThumbnailUrl(_width, Height);

        StateHasChanged();
    }

    private void OnClick(MouseEventArgs obj)
    {
        NavigationManager.NavigateTo(DetailUrl);
    }
}