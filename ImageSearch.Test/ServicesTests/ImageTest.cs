using ImageSearch.Services;

namespace ImageSearch.Test.ServicesTests;

[TestClass]
public class ImageTest
{
    [TestMethod]
    public void TestConstructor()
    {
        // Arrange
        HttpClient client = new();
        string url = "https://api.dasch.swiss/v2/resources/http%3A%2F%2Frdfh.ch%2F0804%2F0hJhqVA_SeGE0SDYtntHDQ";
        using HttpRequestMessage request = new(HttpMethod.Get, url);

        HttpResponseMessage response = client.SendAsync(request).Result;
        response.EnsureSuccessStatusCode();
        string content = response.Content.ReadAsStringAsync().Result;

        // Act
        Image result = new(content);

        // Assert
        Console.WriteLine(result);

        Assert.AreEqual(13661, result.ImageNr);
        Assert.AreEqual("1940", result.Year);
        Assert.AreEqual("Vorpflug Postauto bei der Schneeräumung", result.Description);
        Assert.AreEqual(new DateTime(2014, 06, 19, 10, 08, 19), result.CreationDate);
        Assert.AreEqual("WIRTSCHAFT-VERKEHR-Schneeräumungs- und Pistenfahrzeuge (013661)", result.Title);
    }

    [TestMethod]
    public void TestProcessTitle()
    {
        // Arrange
        string input = "SOMETHING-SOME OTHER THING-Actual Title (000012)";

        // Act
        bool result = Image.ProcessTitle(input, 12, out string[] subject, out string caption);

        // Assert
        Assert.IsTrue(result);
        CollectionAssert.AreEquivalent(new[] { "Something", "Some Other Thing", }, subject);
        Assert.AreEqual("Actual Title", caption);
    }

    [TestMethod]
    public void TestProcessTitle_more_subjects_some_trimming_necessary()
    {
        // Arrange
        string input = " SOMETHING -SOME OTHER THING-AND ANOTHER THING - Actual Title (000012)  ";

        // Act
        bool result = Image.ProcessTitle(input, 12, out string[] subject, out string caption);

        // Assert
        Assert.IsTrue(result);
        CollectionAssert.AreEquivalent(new[] { "Something", "Some Other Thing", "And Another Thing", }, subject);
        Assert.AreEqual("Actual Title", caption);
    }

    [TestMethod]
    public void TestProcessTitle_no_subjects_but_minues_in_the_title()
    {
        // Arrange
        string input = " Just a Title that--contains a minus (000012)  ";

        // Act
        bool result = Image.ProcessTitle(input, 12, out string[] subject, out string caption);

        // Assert
        Assert.IsTrue(result);
        CollectionAssert.AreEquivalent(Array.Empty<string>(), subject);
        Assert.AreEqual("Just a Title that--contains a minus", caption);
    }

    [TestMethod]
    public void TestProcessTitle_number_does_not_match()
    {
        // Arrange
        string input = " Just a Title (012)   ";

        // Act
        bool result = Image.ProcessTitle(input, 13, out string[] subject, out string caption);

        // Assert
        Assert.IsTrue(result);
        CollectionAssert.AreEquivalent(Array.Empty<string>(), subject);
        Assert.AreEqual("Just a Title (012)", caption);
    }

    [TestMethod]
    public void TestProcessTitle_longer_padding()
    {
        // Arrange
        string input = "SOMETHING-SOME OTHER THING-Actual Title (00000000012)";

        // Act
        bool result = Image.ProcessTitle(input, 12, out string[] subject, out string caption);

        // Assert
        Assert.IsTrue(result);
        CollectionAssert.AreEquivalent(new[] { "Something", "Some Other Thing", }, subject);
        Assert.AreEqual("Actual Title", caption);
    }

    [TestMethod]
    public void TestProcessTitle_no_padding()
    {
        // Arrange
        string input = "SOMETHING-SOME OTHER THING-Actual Title (12)";

        // Act
        bool result = Image.ProcessTitle(input, 12, out string[] subject, out string caption);

        // Assert
        Assert.IsTrue(result);
        CollectionAssert.AreEquivalent(new[] { "Something", "Some Other Thing", }, subject);
        Assert.AreEqual("Actual Title", caption);
    }
}