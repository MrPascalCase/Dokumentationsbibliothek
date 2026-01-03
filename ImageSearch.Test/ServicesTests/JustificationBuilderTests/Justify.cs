using ImageSearch.Services;
using ImageSearch.Services.Justifications;
using Microsoft.AspNetCore.Components;

namespace ImageSearch.Test.ServicesTests.JustificationBuilderTests;

[TestClass]
public class Justify
{
    [TestMethod]
    public void TestJustify_example_with_one_term()
    {
        // Arrange
        ImageQuery query = ImageQuery.FromText("postauto");
        Image result = new() { Description = "Vorpflug Postauto bei der Schneeräumung", };
        JustificationBuilder builder = new() { ContextLength = 40, MatchStyle = "match", };

        // Act
        MarkupString markup = builder.Justify(query, result);

        // Assert
        Console.WriteLine(markup);
        Assert.AreEqual("<div>Vorpflug <span style=\"match\">Postauto</span> bei der Schneeräumung</div>", markup.ToString());
    }

    [TestMethod]
    public void TestJustify_simple_example_seaching_for_2_terms()
    {
        // Arrange
        string text =
            "Neues Post-Hotel, Schweizerische Kreditanstalt (SKA), heute Boutiquen, Via Serlas, Flaggen (Fahnen) verschiedener Nationen,Parkierte Autos";

        ImageQuery query = new() { Description = new[] { "post", "hotel", }, };
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
}