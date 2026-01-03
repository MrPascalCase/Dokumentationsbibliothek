using ImageSearch.Services.Justifications;

namespace ImageSearch.Test.ServicesTests.JustificationBuilderTests;

[TestClass]
public class AddContext
{
    [TestMethod]
    public void TestAddContext_context_lenght_1()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 1, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };
        string input = "bbbabbb";

        TextRange range = builder.GetMatches(new[] { "a", }, input).Single();

        // Act
        Match match = builder.AddContext(range, input);

        // Assert
        Assert.AreEqual(new TextRange(3, 1), match.MatchRanges.Single());
        Assert.AreEqual(new TextRange(2, 3), match.ContextRange);
        Assert.AreEqual(Elipses.Both, match.Elipses);
    }

    [TestMethod]
    public void TestAddContext_lenght_2()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 2, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };
        string input = "bbbabbb";
        TextRange range = builder.GetMatches(new[] { "a", }, input).Single();

        // Act
        Match match = builder.AddContext(range, input);

        // Assert
        Assert.AreEqual("..{1}[3, 4]{6}..", match.ToString());
    }

    [TestMethod]
    public void TestAddContext_lenght_3()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 3, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };
        string input = "bbbabbb";
        TextRange range = builder.GetMatches(new[] { "a", }, input).Single();

        // Act
        Match match = builder.AddContext(range, input);

        // Assert
        Assert.AreEqual("{0}[3, 4]{7}", match.ToString());
    }

    [TestMethod]
    public void TestAddContext_context_lenght_0()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 0, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };
        string input = "bbbabbb";
        //              0123
        TextRange range = builder.GetMatches(new[] { "a", }, input).Single();

        // Act
        Match match = builder.AddContext(range, input);

        // Assert
        Assert.AreEqual(new TextRange(3, 1), match.MatchRanges.Single());
        Assert.AreEqual(new TextRange(3, 1), match.ContextRange);
        Assert.AreEqual(Elipses.Both, match.Elipses);
    }

    [TestMethod]
    public void TestAddContext_context_lenght_longer_than_text()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 10, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };
        string input = "bbbabbb";
        //              0123
        TextRange range = builder.GetMatches(new[] { "a", }, input).Single();

        // Act
        Match match = builder.AddContext(range, input);

        // Assert
        Assert.AreEqual(new TextRange(3, 1), match.MatchRanges.Single());
        Assert.AreEqual(new TextRange(0, 7), match.ContextRange);
        Assert.AreEqual(Elipses.None, match.Elipses);
    }
}