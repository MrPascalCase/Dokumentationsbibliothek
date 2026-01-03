using ImageSearch.Services;
using ImageSearch.Services.QueryProcessing;

namespace ImageSearch.Test.ServicesTests;

[TestClass]
public class QueryProcessorTest
{
    [TestMethod]
    public async Task TestBuildQuery_search_for_schee_and_post_in_the_1950s()
    {
        // Arrange
        SearchService service = new(new HttpClient(), null);
        QueryProcessor processor = new();
        ImageQuery query = new() {Decade = 1950, Description = new[] { "schnee", "post", }};

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