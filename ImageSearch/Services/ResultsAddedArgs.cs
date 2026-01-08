namespace ImageSearch.Services;

public class SearchResultsAddedArgs
{
    /// <summary>
    ///     Indicates that now FetchData was this many times called. The next call should be made with
    ///     LatestFetchGeneration + 1.
    /// </summary>
    public int LatestFetchGeneration { get; init; }
}