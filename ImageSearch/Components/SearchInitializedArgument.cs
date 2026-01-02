using ImageSearch.Services;

namespace ImageSearch.Components;

public class SearchInitializedArgument
{
    public ImageQuery? Query { get; set; }
    public int? ResultCount { get; set; }
}