namespace ImageSearch.Services;

public interface IImageDetailResolver
{
    Task<string> ResolveSeasonNodeLabel(string nodeId);
}