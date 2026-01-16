using ImageSearch.Services;
using ImageSearch.Services.Interfaces;
using ImageSearch.Services.Justifications;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ImageSearch.Components;

public partial class ImagePreview : ComponentBase
{
    public const int Height = 250;

    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    [Inject]
    public required SearchSession SearchSession { get; set; }
    
    [Inject]
    public required ISearchService SearchService { get; set; }
    
    [Parameter]
    public required string ImageId { get; set; }

    [Parameter]
    public EventCallback<string> OnImageSelected { get; set; }

    private string Title => string.IsNullOrWhiteSpace(_result?.Caption) ? "<Titel nicht definiert>" : _result.Caption;
    private string Jahr => string.IsNullOrWhiteSpace(_result?.Year) ? "<Jahr nicht definiert>" : _result.Year;
    private string Size => $"width: {_width}px; height: {Height}px";
    private string DetailUrl => NavigationManager.BaseUri + $"image?id={ImageId}";

    private int _width = Height;
    private string _imageSrc = "./placeholder";
    private string _imageAlt = "Bild wird geladen...";
    private Image? _result;
    private JustificationBuilder _builder = new();

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrWhiteSpace(ImageId)) throw new NullReferenceException(nameof(ImageId));

        _result = await SearchService.LoadImage(ImageId);
        if (_result == null) return;

        double aspectRatio = (double)_result.Width / _result.Height;
        _width = (int)Math.Round(aspectRatio * Height);
        _imageSrc = _result.GetThumbnailUrl(_width, Height);

        StateHasChanged();
    }

    private async Task OnClick(MouseEventArgs obj)
    {
        if (OnImageSelected.HasDelegate)
        {
            await OnImageSelected.InvokeAsync(ImageId);
        }
        else
        {
            NavigationManager.NavigateTo(DetailUrl);
        }
    }
}