using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ImageSearch.Components;

public partial class Overlay : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<Overlay>? _objectRef;

    [Parameter]
    public required RenderFragment ChildContent { get; set; }

    [Parameter]
    public required EventCallback OnOverlayClosed { get; set; }

    [Inject]
    public required IJSRuntime JsRuntime { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _objectRef = DotNetObjectReference.Create(this);
            _module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Overlay.razor.js");
            await _module.InvokeVoidAsync("init", _objectRef, nameof(CloseOverlay));
        }
    }

    [JSInvokable]
    public async Task CloseOverlay()
    {
        await JsRuntime.InvokeVoidAsync("document.body.classList.remove", "overlay-open");
        await OnOverlayClosed.InvokeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            await _module.InvokeVoidAsync("dispose");
            await _module.DisposeAsync();
        }

        if (_objectRef != null)
        {
            _objectRef.Dispose();
            _objectRef = null;
        }
    }
}