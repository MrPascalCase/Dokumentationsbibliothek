using ImageSearch.Services;
using ImageSearch.Services.Dto;
using ImageSearch.Services.Justifications;
using Microsoft.AspNetCore.Components;

namespace ImageSearch.Test.Services.Justifications;

[TestClass]
public class JustificationBuilderTest
{
    #region Tests for the method 'AddContext'

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

    #endregion

    #region Tests for the method 'GetIndexesOf'

    [TestMethod]
    public void TestGetIndexesOf()
    {
        // Arrange
        string input = "abcbabcab";
        //              012345678
        //              '   '  '

        // Act
        int[] result = JustificationBuilder.GetIndexesOf("ab", input);

        // Assert
        CollectionAssert.AreEqual(new[] { 0, 4, 7, }, result);
    }

    #endregion

    #region Tests for the method 'GetMatches'

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

    #endregion

    #region Tests for the method 'Justify'

    [TestMethod]
    public void TestJustify_example_with_one_term()
    {
        // Arrange
        Query query = Query.FromText("postauto");
        Image result = new() { Description = "Vorpflug Postauto bei der Schneeräumung", };
        JustificationBuilder builder = new() { ContextLength = 40, MatchStyle = "match", };

        // Act
        MarkupString markup = builder.Justify(query, result);

        // Assert
        Console.WriteLine(markup);
        Assert.AreEqual("<div>Vorpflug <span style=\"match\">Postauto</span> bei der Schneeräumung</div>", markup.ToString());
    }

    [TestMethod]
    public void TestJustify_simple_example_searching_for_2_terms()
    {
        // Arrange
        string text =
            "Neues Post-Hotel, Schweizerische Kreditanstalt (SKA), heute Boutiquen, Via Serlas, Flaggen (Fahnen) verschiedener Nationen,Parkierte Autos";

        Query query = new() { Terms = new[] { "post", "hotel", }, };
        Image result = new() { Description = text, };
        JustificationBuilder builder = new() { ContextLength = 40, MatchStyle = "match", };

        // Act
        MarkupString markup = builder.Justify(query, result);

        // Assert
        Console.WriteLine(markup.ToString());
    }

    [TestMethod]
    public void TestJustify_one_match_with_context()
    {
        // Arrange
        JustificationBuilder builder = new() { ContextLength = 2, MatchStyle = "match", RequireWhitespace = false, RespectHardBreak = false, };
        string input = "bbbabbb";

        // Act
        MarkupString result = builder.Justify(new[] { "a", }, input);

        // Assert
        Console.WriteLine(result);
        Assert.AreEqual("<div>...bb<span style=\"match\">a</span>bb...</div>", result.ToString());
    }

    [TestMethod]
    public void TestJustify_require_whitespace_ellipsis_at_start_hard_break_at_then_end()
    {
        // Arrange
        JustificationBuilder builder = new()
        {
            ContextLength = 30,
            MatchStyle = "match",
            RequireWhitespace = true,
            RespectHardBreak = true,
        };

        string input =
            """
            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut 
            enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor 
            in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, 
            sunt in culpa qui officia deserunt mollit anim id est laborum.
            """;

        // Act
        MarkupString result = builder.Justify(new[] { "commodo", }, input);

        // Assert
        Console.WriteLine(result);
        Assert.AreEqual("<div>...laboris nisi ut aliquip ex ea <span style=\"match\">commodo</span> consequat.</div>", result.ToString());
    }

    [TestMethod]
    public void TestJustify_require_whitespace_ellipsis_at_start_and_end()
    {
        // Arrange
        JustificationBuilder builder = new()
        {
            ContextLength = 10,
            MatchStyle = "match",
            RequireWhitespace = true,
            RespectHardBreak = true,
        };

        string input =
            """
            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut 
            enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor 
            in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, 
            sunt in culpa qui officia deserunt mollit anim id est laborum.
            """;

        // Act
        MarkupString result = builder.Justify(new[] { "consectetur", }, input);

        // Assert
        Console.WriteLine(result);
        Assert.AreEqual("<div>...sit amet, <span style=\"match\">consectetur</span> adipiscing...</div>", result.ToString());
    }

    [TestMethod]
    public void TestJustify_require_whitespace_ellipsis_at_the_end_textstart_at_the_start()
    {
        // Arrange
        JustificationBuilder builder = new()
        {
            ContextLength = 40,
            MatchStyle = "match",
            RequireWhitespace = true,
            RespectHardBreak = true,
        };

        string input =
            """
            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut 
            enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor 
            in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, 
            sunt in culpa qui officia deserunt mollit anim id est laborum.
            """;

        // Act
        MarkupString result = builder.Justify(new[] { "consectetur", }, input);

        // Assert
        Console.WriteLine(result);
        Assert.AreEqual("<div>Lorem ipsum dolor sit amet, <span style=\"match\">consectetur</span> adipiscing elit, sed do eiusmod tempor incididunt...</div>",
            result.ToString());
    }

    [TestMethod]
    public void TestJustify_require_whitespace_ellipsis_at_the_start_reaching_text_end()
    {
        // Arrange
        JustificationBuilder builder = new()
        {
            ContextLength = 15,
            MatchStyle = "match",
            RequireWhitespace = true,
            RespectHardBreak = true,
        };

        string input =
            """
            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut 
            enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor 
            in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, 
            sunt in culpa qui officia deserunt mollit anim id est laborum.
            """;

        // Act
        MarkupString result = builder.Justify(new[] { "mollit", }, input);

        // Assert
        Console.WriteLine(result);
        Assert.AreEqual("<div>...officia deserunt <span style=\"match\">mollit</span> anim id est laborum.</div>", result.ToString());
    }

    [TestMethod]
    public void TestJustify_preceding_hard_break()
    {
        // Arrange
        JustificationBuilder builder = new()
        {
            ContextLength = 20,
            MatchStyle = "match",
            RequireWhitespace = true,
            RespectHardBreak = true,
        };

        string input =
            """
            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut 
            enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor 
            in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, 
            sunt in culpa qui officia deserunt mollit anim id est laborum.
            """;

        // Act
        MarkupString result = builder.Justify(new[] { "occaecat", }, input);

        // Assert
        Console.WriteLine(result);
        Assert.AreEqual("<div>Excepteur sint <span style=\"match\">occaecat</span> cupidatat non proident,...</div>", result.ToString());
    }

    [TestMethod]
    public void TestJustify_preceding_hard_break_which_is_an_exception()
    {
        // Arrange
        JustificationBuilder builder = new()
        {
            ContextLength = 20,
            MatchStyle = "match",
            RequireWhitespace = true,
            RespectHardBreak = true,
            HardBreakExceptions = new[] { "tur. Exc", },
        };

        string input =
            """
            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut 
            enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor 
            in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, 
            sunt in culpa qui officia deserunt mollit anim id est laborum.
            """;

        // Act
        MarkupString result = builder.Justify(new[] { "occaecat", }, input);

        // Assert
        Console.WriteLine(result);
        Assert.AreEqual("<div>...pariatur. Excepteur sint <span style=\"match\">occaecat</span> cupidatat non proident,...</div>", result.ToString());
    }
    
    [TestMethod]
    public void TestJustify_hard_break_afterwards()
    {
        // Arrange
        JustificationBuilder builder = new()
        {
            ContextLength = 20,
            MatchStyle = "match",
            RequireWhitespace = true,
            RespectHardBreak = true,
        };

        string input =
            """
            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut 
            enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor 
            in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, 
            sunt in culpa qui officia deserunt mollit anim id est laborum.
            """;

        // Act
        MarkupString result = builder.Justify(new[] { "fugiat", }, input);

        // Assert
        Console.WriteLine(result);
        Assert.AreEqual("<div>...esse cillum dolore eu <span style=\"match\">fugiat</span> nulla pariatur.</div>", result.ToString());
    }

    
    [TestMethod]
    public void TestJustify_hard_break_afterwards_is_an_exception()
    {
        // Arrange
        JustificationBuilder builder = new()
        {
            ContextLength = 20,
            MatchStyle = "match",
            RequireWhitespace = true,
            RespectHardBreak = true,
            HardBreakExceptions = new[] { "tur. Exc", },
        };

        string input =
            """
            Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut 
            enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor 
            in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, 
            sunt in culpa qui officia deserunt mollit anim id est laborum.
            """;

        // Act
        MarkupString result = builder.Justify(new[] { "fugiat", }, input);

        // Assert
        Console.WriteLine(result);
        Assert.AreEqual("<div>...esse cillum dolore eu <span style=\"match\">fugiat</span> nulla pariatur. Excepteur...</div>", result.ToString());
    }

    #endregion

    #region Tests for the method 'TryCombine'

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

    #endregion
}