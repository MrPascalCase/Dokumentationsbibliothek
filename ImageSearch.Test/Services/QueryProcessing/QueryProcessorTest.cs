using ImageSearch.Services;
using ImageSearch.Services.Dto;
using ImageSearch.Services.QueryProcessing;

namespace ImageSearch.Test.Services.QueryProcessing;

[TestClass]
public class QueryProcessorTest
{
    [TestMethod]
    public async Task TestBuildQuery_search_for_schnee_and_post_in_the_1950s()
    {
        // Arrange
        ISearchService service = new SearchService(new HttpClient(), null);
        QueryProcessor processor = new();
        ImageQuery query = new() { Decade = 1950, Description = new[] { "schnee", "post", }, };

        // Act
        string sparqlQuery = processor.BuildQuery(query);
        Console.WriteLine(sparqlQuery);

        // Assert
        ImageIdCollection ids = await service.LoadIds(sparqlQuery, 0, 10);
        Image[] images = await service.LoadImages(ids);
        foreach (Image image in images)
        {
            Assert.AreEqual(1950, image.Decade);
            Assert.IsTrue(image.Description.ToLowerInvariant().Contains("schnee"));
            Assert.IsTrue(image.Description.ToLowerInvariant().Contains("post"));
        }
    }
}