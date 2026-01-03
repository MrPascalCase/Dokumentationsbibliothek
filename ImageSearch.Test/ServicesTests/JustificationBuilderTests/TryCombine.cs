using ImageSearch.Services.Justifications;

namespace ImageSearch.Test.ServicesTests.JustificationBuilderTests;

[TestClass]
public class TryCombine
{
    [TestMethod]
    public void TestTryCombine_context_does_not_overlap()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 2, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };
        TextRange r1 = new(1, 1);
        Match match1 = new(new[] { r1, }, r1, Elipses.Both);

        TextRange r2 = new(3, 1);
        Match match2 = new(new[] { r2, }, r2, Elipses.Both);

        // Act
        bool didCombine = builder.TryCombine(match1, match2, out Match? result);

        // Assert
        Assert.IsFalse(didCombine);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestTryCombine_context_does_overlap_matches_do_not()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 2, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };
        TextRange r1 = new(1, 1);
        TextRange c1 = new(0, 3);
        Match match1 = new(new[] { r1, }, c1, Elipses.Both);

        TextRange r2 = new(3, 1);
        TextRange c2 = new(2, 3);
        Match match2 = new(new[] { r2, }, c2, Elipses.None);

        // Act
        bool didCombine = builder.TryCombine(match1, match2, out Match? result);

        // Assert
        Assert.IsTrue(didCombine);
        Assert.IsNotNull(result);

        Assert.AreEqual(new TextRange(0, 5), result.Value.ContextRange);
        Assert.AreEqual(2, result.Value.MatchRanges.Count);
        Assert.AreEqual(new TextRange(1, 1), result.Value.MatchRanges[0]);
        Assert.AreEqual(new TextRange(3, 1), result.Value.MatchRanges[1]);
        Assert.AreEqual(Elipses.Start, result.Value.Elipses);
    }

    [TestMethod]
    public void TestTryCombine_context_does_overlap_both_matches_have_submatches()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 2, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };
        TextRange r1 = new(0, 1);
        TextRange r2 = new(2, 2);
        TextRange r3 = new(7, 2);
        TextRange c1 = new(0, 10);
        Match match1 = new(new[] { r1, r2, r3, }, c1, Elipses.Both);

        TextRange r4 = new(8, 2);
        TextRange r5 = new(12, 4);
        TextRange c2 = new(6, 10);
        Match match2 = new(new[] { r4, r5, }, c2, Elipses.None);

        // Act
        bool didCombine = builder.TryCombine(match1, match2, out Match? result);

        // Assert
        Assert.IsTrue(didCombine);
        Assert.IsNotNull(result);

        Assert.AreEqual(new TextRange(0, 16), result.Value.ContextRange);
        Assert.AreEqual(Elipses.Start, result.Value.Elipses);
        Assert.AreEqual(4, result.Value.MatchRanges.Count);
        Assert.AreEqual(new TextRange(0, 1), result.Value.MatchRanges[0]);
        Assert.AreEqual(new TextRange(2, 2), result.Value.MatchRanges[1]);
        Assert.AreEqual(new TextRange(7, 3), result.Value.MatchRanges[2]);
        Assert.AreEqual(new TextRange(12, 4), result.Value.MatchRanges[3]);
    }

    [TestMethod]
    public void TestTryCombine_submatches_touch()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 2, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };
        TextRange r1 = new(2, 1);
        TextRange c1 = new(0, 3);
        Match match1 = new(new[] { r1, }, c1, Elipses.None);

        TextRange r2 = new(3, 1);
        TextRange c2 = new(2, 3);
        Match match2 = new(new[] { r2, }, c2, Elipses.Both);

        // Act
        bool didCombine = builder.TryCombine(match1, match2, out Match? result);

        // Assert
        Assert.IsTrue(didCombine);
        Assert.IsNotNull(result);
        Console.WriteLine(result);

        Assert.AreEqual(new TextRange(0, 5), result.Value.ContextRange);
        Assert.AreEqual(Elipses.End, result.Value.Elipses);
        Assert.AreEqual(1, result.Value.MatchRanges.Count);
        Assert.AreEqual(new TextRange(2, 2), result.Value.MatchRanges[0]);
    }
}