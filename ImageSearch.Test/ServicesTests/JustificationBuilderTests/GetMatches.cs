using ImageSearch.Services.Justifications;

namespace ImageSearch.Test.ServicesTests.JustificationBuilderTests;

[TestClass]
public class GetMatches
{
    [TestMethod]
    public void TestGetMatches_one_match()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 1, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };

        string input = "bbbabbb";
        //              0123456

        // Act
        TextRange[] result = builder.GetMatches(new[] { "a", }, input);

        // Assert
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(3, result[0].Start);
        Assert.AreEqual(1, result[0].Length);
    }

    [TestMethod]
    public void TestGetMatches_two_matches()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 1, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };

        string input = "babbbab";
        //              0123456

        // Act
        TextRange[] result = builder.GetMatches(new[] { "a", }, input);

        // Assert
        Assert.AreEqual(2, result.Length);

        Assert.AreEqual(1, result[0].Start);
        Assert.AreEqual(1, result[0].Length);
        Assert.AreEqual(5, result[1].Start);
        Assert.AreEqual(1, result[1].Length);
    }

    [TestMethod]
    public void TestGetMatches_one_match_is_a_substring_of_the_other()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 1, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };

        string input = "Vorpflug Postauto bei der Schneeräumung";
        //              0123456789   
        //                        0123456789

        // Act
        TextRange[] result = builder.GetMatches(new[] { "post", "postauto", }, input);

        // Assert
        foreach (TextRange range in result) Console.WriteLine(range);

        Assert.AreEqual(2, result.Length);
        Assert.AreEqual(9, result[0].Start);
        Assert.AreEqual(4, result[0].Length);
        Assert.AreEqual(9, result[1].Start);
        Assert.AreEqual(8, result[1].Length);
    }

    [TestMethod]
    public void TestGetMatches_a_mess_of_substrings()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 1, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };

        string input = "banana";

        // Act
        TextRange[] result = builder.GetMatches(new[] { "nan", "na", }, input);

        // Assert
        foreach (TextRange range in result) Console.WriteLine(range);

        Assert.AreEqual(3, result.Length);
        Assert.AreEqual(new TextRange(2, 3), result[0]);
        Assert.AreEqual(new TextRange(2, 2), result[1]);
        Assert.AreEqual(new TextRange(4, 2), result[2]);
    }
}