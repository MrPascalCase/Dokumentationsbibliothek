using System.Text.Json;
using ImageSearch.Services.Dto;

namespace ImageSearch.Test.Services;

[TestClass]
public class QueryTest
{
    #region Test to ensure consistent serialization and deserialization

    [TestMethod]
    public void TestSerialize_and_deserialize()
    {
        // Arrange
        Query query = new()
        {
            Decade = 1950,
            Terms = new[] { "test1", "test2", },
            Subject = new[] { "sub1", "sub1.1", },
            Author = "Albert",
        };

        // Act
        string serialized = JsonSerializer.Serialize(query);
        Query? deserialized = JsonSerializer.Deserialize<Query>(serialized);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(1950, deserialized.Decade);
        Assert.IsNull(deserialized.ImageNr);
        Assert.AreEqual("Albert", deserialized.Author);
        CollectionAssert.AreEquivalent(new[] { "test1", "test2", }, deserialized.Terms.ToArray());
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
        Query query = Query.ParseSearchText(input)!;

        // Assert
        Assert.IsNotNull(query);
        Assert.AreEqual(1950, query.Decade);
        Assert.AreEqual(null, query.ImageNr);
        CollectionAssert.AreEquivalent(query.Terms.ToArray(), new[] { "postauto", "winter", });
    }


    [TestMethod]
    public void TestParseSearchText_with_subject()
    {
        // Arrange
        string input = "dec:1950 postauto thema:Wirtschaft>Verkehr";

        // Act
        Query query = Query.ParseSearchText(input)!;

        // Assert
        Console.WriteLine(query);
        Assert.IsNotNull(query);
        Assert.AreEqual(1950, query.Decade);
        Assert.AreEqual(null, query.ImageNr);
        CollectionAssert.AreEquivalent(query.Terms.ToArray(), new[] { "postauto", });
        CollectionAssert.AreEqual(query.Subject.ToArray(), new[] { "Wirtschaft", "Verkehr", });
    }

    [TestMethod]
    public void TestParseSearchText_incomplete_author()
    {
        // Arrange
        string input = "autor:\"";

        // Act
        Query query = Query.ParseSearchText(input)!;

        // Assert
        Assert.IsNull(query.Author);
        string representation = query.ToString();
        Assert.AreEqual(string.Empty, representation);
    }
    
    [TestMethod]
    [Ignore] // TODO this is broken, probably should be fixed when we support 
    public void TestParseSearchText_incomplete_author_2()
    {
        // Arrange
        string input = "autor:\" \"";

        // Act
        Query query = Query.ParseSearchText(input)!;

        // Assert
        Assert.IsNull(query.Author);
        string representation = query.ToString();
        Assert.AreEqual(string.Empty, representation);
    }

    #endregion

    #region Tests for the static method 'ParseUrl'

    [TestMethod]
    public void TestParseUrl_parse_decade_with_2_search_terms()
    {
        // Arrange
        string input = "?query=postauto,winter&dec=1950";

        // Act
        Query query = Query.ParseUrl(input)!;

        // Assert
        Assert.IsNotNull(query);
        Assert.AreEqual(1950, query.Decade);
        Assert.AreEqual(null, query.ImageNr);
        CollectionAssert.AreEquivalent(query.Terms.ToArray(), new[] { "postauto", "winter", });
    }

    [TestMethod]
    public void TestParseUrl_parse_with_subject()
    {
        // Arrange
        string input = "?query=postauto,winter&dec=1950&subject=Wirtschaft,Verkehr";

        // Act
        Query query = Query.ParseUrl(input)!;

        // Assert
        Assert.IsNotNull(query);
        Assert.AreEqual(1950, query.Decade);
        Assert.AreEqual(null, query.ImageNr);
        CollectionAssert.AreEquivalent(query.Terms.ToArray(), new[] { "postauto", "winter", });
        CollectionAssert.AreEqual(query.Subject.ToArray(), new[] { "Wirtschaft", "Verkehr", });
    }

    #endregion

    #region Tests for the method 'ToUrl'

    [TestMethod]
    public void TestToUrl()
    {
        // Arrange
        Query query = new() { Decade = 1950, Terms = new[] { "postauto", "winter", }, };

        // Act
        string url = query.ToUrl();

        // Assert
        Assert.AreEqual("?query=postauto,winter&dekade=1950", url);
    }

    [TestMethod]
    public void TestToUrl_ParseUrlQuery_url_is_properly_encoded()
    {
        // Arrange
        Query query = new() { Terms = new[] { "postauto/winter", }, };

        // Act
        string url = query.ToUrl();
        Query parsed = Query.ParseUrl(url)!;

        // Assert
        Assert.AreEqual(1, parsed.Terms.Count);
        Assert.AreEqual("postauto/winter", parsed.Terms[0]);
        Assert.IsFalse(url.Contains("/"));
    }

    #endregion

    #region Round-trips: ToUrl and ParseUrl / ToCanonicalSearchText and ParseSearchText

    [TestMethod]
    public void TestToUrl_and_ParseUrl_are_round_tripable()
    {
        // Arrange
        Query query = new()
        {
            Author = "Albert", Decade = null, ImageNr = 123, Terms = new[] { "postauto", "winter", }, Subject = new[] { "Wirtschaft", },
        };

        // Act
        Query? result = Query.ParseUrl(query.ToUrl());

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(query, result);
    }

    [TestMethod]
    public void TestToCanonicalSearchText_and_ParseSearchText_are_round_tripable()
    {
        // Arrange
        Query query = new()
        {
            Author = "Albert", Decade = null, ImageNr = 123, Terms = new[] { "postauto", "winter", }, Subject = new[] { "Wirtschaft", },
        };

        // Act
        Query? result = Query.ParseSearchText(query.ToCanonicalSearchText());

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(query, result);
    }

    #endregion

    #region Tests for equality and hashcodes

    [TestMethod]
    public void TestEquals_is_equal()
    {
        // Arrange
        Query q1 = new() { Decade = 1950, Terms = new[] { "test1", "test2", }, };
        Query q2 = new() { Decade = 1950, Terms = new[] { "test1", "test2", }, };

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
        Query q1 = new() { Decade = 1950, Terms = new[] { "test1", "test2", "test3", }, };
        Query q2 = new() { Decade = 1950, Terms = new[] { "test1", "test2", }, };

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
        Query q1 = new() { Decade = 1950, Terms = new[] { "test1", "test2", }, Subject = new[] { "sub1", "sub1.1", }, };
        Query q2 = new() { Decade = 1950, Terms = new[] { "test1", "test2", }, Subject = new[] { "sub1", "sub1.1", }, };

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
        Query q1 = new() { Decade = 1950, Terms = new[] { "test1", "test2", }, Subject = new[] { "sub1", "sub1.1", }, };
        Query q2 = new() { Decade = 1950, Terms = new[] { "test1", "test2", }, Subject = new[] { "sub1", "sub1.1", "sub1.1.2", }, };

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