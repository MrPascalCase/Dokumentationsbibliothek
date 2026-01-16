namespace ImageSearch.Services.Interfaces;

public interface IImageDetailResolver
{
    Task<string> ResolveSeasonNodeLabel(string nodeId);
}