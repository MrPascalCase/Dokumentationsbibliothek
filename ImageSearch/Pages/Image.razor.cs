using Microsoft.AspNetCore.Components;

namespace ImageSearch.Pages;

public partial class Image : ComponentBase
{
    [SupplyParameterFromQuery(Name = "id")]
    public required string ImageId { get; set; } = string.Empty;
}