using ImageSearch.Services.Dto;

namespace ImageSearch.Services.Interfaces;

public interface ISearchService
{
    public Task<int?> Count(Query query);

    public Task<ImageIdCollection> LoadIds(Query query, int start, int count);

    public Task<Image?> LoadImage(string imageId);
}