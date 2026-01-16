using ImageSearch.Services.Dto;

namespace ImageSearch.Services.Interfaces;

internal static class SearchServiceExtensions
{
    public static async Task<Image[]> LoadImages(this ISearchService service, IEnumerable<string> imageIds)
    {
        if (imageIds == null) throw new ArgumentNullException(nameof(imageIds));

        Task<Image?>[] tasks = imageIds.Select(service.LoadImage).ToArray();
        Image?[] images = await Task.WhenAll(tasks);
        return images.Where(img => img != null).Select(img => img!).ToArray();
    }
}