using ImageSearch.Services.QueryProcessing;

namespace ImageSearch.Services;

public interface ISearchService
{
    public Task<int?> Count(ImageQuery query);

    public Task<ImageIdCollection> LoadIds(string query, int start, int count);

    public async Task<ImageIdCollection> LoadIds(ImageQuery query, int start, int count)
    {
        QueryProcessor processor = new();
        string sparqlQuery = processor.BuildQuery(query);
        return await LoadIds(sparqlQuery, start, count);
    }

    public Task<Image?> LoadImage(string imageId);

    public async Task<Image[]> LoadImages(IEnumerable<string> imageIds)
    {
        if (imageIds == null) throw new ArgumentNullException(nameof(imageIds));

        Task<Image?>[] tasks = imageIds.Select(LoadImage).ToArray();
        Image?[] images = await Task.WhenAll(tasks);
        return images.Where(img => img != null).Select(img => img!).ToArray();
    }
}