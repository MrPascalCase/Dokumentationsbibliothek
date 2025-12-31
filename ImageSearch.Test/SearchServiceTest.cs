using ImageSearch.Services;

namespace ImageSearch.Test;

[TestClass]
public class SearchServiceTest
{
    [TestMethod]
    public async Task TestLoadIds_by_imageNr()
    {
        // Arrange
        SearchService searchService = new( new HttpClient() , logger: null);

        // Act
        SearchService.Ids ids = await searchService.LoadIds(new Query { ImageNr = 14826, }, 0, 25);
        Image image = await searchService.LoadImage(ids.Single());

        // Assert
        Console.WriteLine(image);
    }

    [TestMethod]
    public async Task TestLoadIds_fetch_ids_form_2_pages()
    {
        // Arrange
        SearchService searchService = new(new HttpClient() , logger: null);

        // Act
        SearchService.Ids ids = await searchService.LoadIds(new Query { Text = "Postauto", }, 0, 40);

        // Assert
        Assert.AreEqual(40, ids.Count);
        Assert.AreEqual(2, ids.PagesQueried);
    }

    [TestMethod]
    public async Task TestLoadIds_load_2_ids_from_2_pages()
    {
        // Arrange
        SearchService searchService = new(new HttpClient() , logger: null);
        SearchService.Ids ids = await searchService.LoadIds(new Query { Text = "Postauto", }, 0, 50);

        // Act
        SearchService.Ids ids25And26 = await searchService.LoadIds(new Query { Text = "Postauto", }, 24, 2);

        // Assert
        Assert.AreEqual(50, ids.Count);
        Assert.AreEqual(2, ids.PagesQueried);

        Assert.AreEqual(2, ids25And26.Count);
        Assert.AreEqual(2, ids25And26.PagesQueried);

        Assert.AreEqual(ids[24], ids25And26[0]);
        Assert.AreEqual(ids[25], ids25And26[1]);
    }

    [TestMethod]
    public async Task TestLoadIds_load_ids_from_3_pages_that_would_fit_on_2_pages()
    {
        // Arrange
        SearchService searchService = new(new HttpClient() , logger: null);
        SearchService.Ids ids0To100 = await searchService.LoadIds(new Query { Text = "Schnee", }, 0, 100);

        // Act
        SearchService.Ids ids33To77 = await searchService.LoadIds(new Query { Text = "Schnee", }, 33, 44);

        // Assert
        Assert.AreEqual(100, ids0To100.Count);
        Assert.AreEqual(4, ids0To100.PagesQueried);

        Assert.AreEqual(44, ids33To77.Count);
        Assert.AreEqual(3, ids33To77.PagesQueried);

        for (int i = 0; i < 44; i++)
        {
            Assert.AreEqual(ids0To100[i + 33], ids33To77[i]);
        }
    }
}