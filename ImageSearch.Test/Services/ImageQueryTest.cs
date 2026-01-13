using System.Text.Json;
using ImageSearch.Services.Dto;

namespace ImageSearch.Test.Services;

[TestClass]
public class ImageQueryTest
{
    #region Test to ensure consistent serialization and deserialization

    [TestMethod]
    public void TestSerialize_and_deserialize()
    {
        // Arrange
        ImageQuery query = new()
        {
            Decade = 1950,
            Description = new[] { "test1", "test2", },
            Subject = new[] { "sub1", "sub1.1", },
        };

        // Act
        string serialized = JsonSerializer.Serialize(query);
        ImageQuery? deserialized = JsonSerializer.Deserialize<ImageQuery>(serialized);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(1950, deserialized.Decade);
        Assert.IsNull(deserialized.ImageNr);
        CollectionAssert.AreEquivalent(new[] { "test1", "test2", }, deserialized.Description.ToArray());
        CollectionAssert.AreEqual(new[] { "sub1", "sub1.1", }, deserialized.Subject.ToArray());
    }

    #endregion

    #region Tests for the static method 'ParseSearchText'

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
    public void TestParseSearchText_with_subject()
    {
        // Arrange
        string input = "dec:1950 postauto thema:Wirtschaft>Verkehr";

        // Act
        ImageQuery query = ImageQuery.ParseSearchText(input)!;

        // Assert
        Console.WriteLine(query);
        Assert.IsNotNull(query);
        Assert.AreEqual(1950, query.Decade);
        Assert.AreEqual(null, query.ImageNr);
        CollectionAssert.AreEquivalent(query.Description.ToArray(), new[] { "postauto", });
        CollectionAssert.AreEqual(query.Subject.ToArray(), new[] { "Wirtschaft", "Verkehr", });
    }

    #endregion

    #region Tests for the static method 'ParseUrlQuery'

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
    public void TestParseUrl_parse_with_subject()
    {
        // Arrange
        string input = "?query=postauto,winter&dec=1950&subject=Wirtschaft,Verkehr";

        // Act
        ImageQuery query = ImageQuery.ParseUrlQuery(input)!;

        // Assert
        Assert.IsNotNull(query);
        Assert.AreEqual(1950, query.Decade);
        Assert.AreEqual(null, query.ImageNr);
        CollectionAssert.AreEquivalent(query.Description.ToArray(), new[] { "postauto", "winter", });
        CollectionAssert.AreEqual(query.Subject.ToArray(), new[] { "Wirtschaft", "Verkehr", });
    }

    #endregion

    #region Tests for the method 'ToUrl'

    [TestMethod]
    public void TestToUrl()
    {
        // Arrange
        ImageQuery query = new() { Decade = 1950, Description = new[] { "postauto", "winter", }, };

        // Act
        string url = query.ToUrl();

        // Assert
        Assert.AreEqual("?query=postauto,winter&dekade=1950", url);
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

    #endregion

    #region Tests for equality and hashcodes

    [TestMethod]
    public void TestEquals_is_equal()
    {
        // Arrange
        ImageQuery q1 = new() { Decade = 1950, Description = new[] { "test1", "test2", }, };
        ImageQuery q2 = new() { Decade = 1950, Description = new[] { "test1", "test2", }, };

        // Act
        bool result = q1.Equals(q2);
        int hash1 = q1.GetHashCode();
        int hash2 = q2.GetHashCode();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void TestEquals_is_not_equal()
    {
        // Arrange
        ImageQuery q1 = new() { Decade = 1950, Description = new[] { "test1", "test2", "test3", }, };
        ImageQuery q2 = new() { Decade = 1950, Description = new[] { "test1", "test2", }, };

        // Act
        bool result = q1.Equals(q2);
        int hash1 = q1.GetHashCode();
        int hash2 = q2.GetHashCode();

        // Assert
        Assert.IsFalse(result);
        Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void TestEquals_is_equal_with_a_subject_defined()
    {
        // Arrange
        ImageQuery q1 = new() { Decade = 1950, Description = new[] { "test1", "test2", }, Subject = new[] { "sub1", "sub1.1", }, };
        ImageQuery q2 = new() { Decade = 1950, Description = new[] { "test1", "test2", }, Subject = new[] { "sub1", "sub1.1", }, };

        // Act
        bool result = q1.Equals(q2);
        int hash1 = q1.GetHashCode();
        int hash2 = q2.GetHashCode();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void TestEquals_is_not_equal_because_of_difference_in_the_subject()
    {
        // Arrange
        ImageQuery q1 = new() { Decade = 1950, Description = new[] { "test1", "test2", }, Subject = new[] { "sub1", "sub1.1", }, };
        ImageQuery q2 = new() { Decade = 1950, Description = new[] { "test1", "test2", }, Subject = new[] { "sub1", "sub1.1", "sub1.1.2", }, };

        // Act
        bool result = q1.Equals(q2);
        int hash1 = q1.GetHashCode();
        int hash2 = q2.GetHashCode();

        // Assert
        Assert.IsFalse(result);
        Assert.AreNotEqual(hash1, hash2);
    }

    #endregion
}