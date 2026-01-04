using System.Text.Json;
using ImageSearch.Services;

namespace ImageSearch.Test.ServicesTests;

[TestClass]
public class ImageQueryTest
{
    [TestMethod]
    public void TestSerialize_and_deserialize()
    {
        // Arrange
        ImageQuery query = new() { Decade = 1950, Description = new[] { "test1", "test2", }, };

        // Act
        string serialized = JsonSerializer.Serialize(query);
        ImageQuery? deserialized = JsonSerializer.Deserialize<ImageQuery>(serialized);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(1950, deserialized.Decade);
        Assert.AreEqual(null, deserialized.ImageNr);
        CollectionAssert.AreEquivalent(deserialized.Description.ToArray(), new[] { "test1", "test2", });
    }

    [TestMethod]
    public void TestParseSearchText_parse_decade_with_2_search_terms()
    {
        // Arrange
        string input = "dec:1950 postauto winter";

        // Act
        ImageQuery query = ImageQuery.ParseSearchText(input)!;

        // Assert
        Assert.IsNotNull(query);
        Assert.AreEqual(1950, query.Decade);
        Assert.AreEqual(null, query.ImageNr);
        CollectionAssert.AreEquivalent(query.Description.ToArray(), new[] { "postauto", "winter", });
    }

    [TestMethod]
    public void TestParseUrl_parse_decade_with_2_search_terms()
    {
        // Arrange
        string input = "?query=postauto,winter&dec=1950";

        // Act
        ImageQuery query = ImageQuery.ParseUrlQuery(input)!;

        // Assert
        Assert.IsNotNull(query);
        Assert.AreEqual(1950, query.Decade);
        Assert.AreEqual(null, query.ImageNr);
        CollectionAssert.AreEquivalent(query.Description.ToArray(), new[] { "postauto", "winter", });
    }

    [TestMethod]
    public void TestToUrl()
    {
        // Arrange
        ImageQuery query = new() { Decade = 1950, Description = new[] { "postauto", "winter", }, };

        // Act
        string url = query.ToUrl();

        // Assert
        Assert.AreEqual("?query=postauto,winter&dec=1950", url);
    }

    [TestMethod]
    public void TestToUrl_ParseUrlQuery_url_is_properly_encoded()
    {
        // Arrange
        ImageQuery query = new() { Description = new[] { "postauto/winter", }, };

        // Act
        string url = query.ToUrl();
        ImageQuery parsed = ImageQuery.ParseUrlQuery(url)!;

        // Assert
        Assert.AreEqual(1, parsed.Description.Count);
        Assert.AreEqual("postauto/winter", parsed.Description[0]);
        Assert.IsFalse(url.Contains("/"));
    }
}