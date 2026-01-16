using ImageSearch.Services;
using ImageSearch.Services.Dto;
using ImageSearch.Services.Interfaces;

namespace ImageSearch.Test.Services;

[TestClass]
public class SearchServiceTest : TestBase
{
    #region Tests for the method 'LoadIds'

    [TestMethod]
    public async Task TestLoadIds_by_imageNr()
    {
        // Arrange
        ISearchService service = new SearchService(new HttpClient(), ArrangeConsoleLogger<SearchService>());

        // Act
        ImageIdCollection ids = await service.LoadIds(new Query { ImageNr = 14826, }, 0, 25);
        Image? image = await service.LoadImage(ids.Single());

        // Assert
        Console.WriteLine(image);
    }

    [TestMethod]
    public async Task TestLoadIds_fetch_ids_form_2_pages()
    {
        // Arrange
        ISearchService service = new SearchService(new HttpClient(), ArrangeConsoleLogger<SearchService>());

        // Act
        ImageIdCollection ids = await service.LoadIds(Query.FromText("Postauto"), 0, 40);

        // Assert
        Assert.AreEqual(40, ids.Count);
        Assert.AreEqual(2, ids.PagesQueried);
    }

    [TestMethod]
    public async Task TestLoadIds_load_2_ids_from_2_pages()
    {
        // Arrange
        ISearchService searchService = new SearchService(new HttpClient(), ArrangeConsoleLogger<SearchService>());
        ImageIdCollection ids = await searchService.LoadIds(Query.FromText("Postauto"), 0, 50);

        // Act
        ImageIdCollection ids25And26 = await searchService.LoadIds(Query.FromText("Postauto"), 24, 2);

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
        ISearchService searchService = new SearchService(new HttpClient(), ArrangeConsoleLogger<SearchService>());
        ImageIdCollection ids0To100 = await searchService.LoadIds(Query.FromText("Schnee"), 0, 100);

        // Act
        ImageIdCollection ids33To77 = await searchService.LoadIds(Query.FromText("Schnee"), 33, 44);

        // Assert
        Assert.AreEqual(100, ids0To100.Count);
        Assert.AreEqual(4, ids0To100.PagesQueried);

        Assert.AreEqual(44, ids33To77.Count);
        Assert.AreEqual(3, ids33To77.PagesQueried);

        for (int i = 0; i < 44; i++)
        {
            Assert.AreEqual(ids0To100[i + 33], ids33To77[i], $"Assert for i={i}");
        }
    }

    [TestMethod]
    public async Task TestLoadIds_5_images_for_postauto()
    {
        // Arrange
        ISearchService search = new SearchService(new HttpClient(), ArrangeConsoleLogger<SearchService>());

        // Act
        ImageIdCollection ids = await search.LoadIds(Query.FromText("Postauto"), 0, 5);
        Image[] images = await search.LoadImages(ids);

        // Assert
        foreach (Image image in images)
        {
            Console.WriteLine(image);
            Assert.IsTrue(image.Description.ToLowerInvariant().Contains("postauto"));
        }
    }

    [TestMethod]
    public async Task TestLoadIds_5_images_for_schnee()
    {
        // Arrange
        ISearchService search = new SearchService(new HttpClient(), ArrangeConsoleLogger<SearchService>());

        // Act
        ImageIdCollection ids = await search.LoadIds(Query.FromText("schnee"), 0, 5);

        // Assert
        Image[] images = await search.LoadImages(ids);
        foreach (Image image in images)
        {
            Console.WriteLine(image);
            Assert.IsTrue(image.Description.ToLowerInvariant().Contains("schnee"));
        }
    }

    [TestMethod]
    public async Task TestLoadIds_search_for_autor_steiner_albert()
    {
        // Arrange
        ISearchService search = new SearchService(new HttpClient(), ArrangeConsoleLogger<SearchService>());
        Query query = new() { Author = "Steiner, Albert", };

        // Act
        ImageIdCollection ids = await search.LoadIds(query, 0, 5);

        // Assert
        Assert.IsNotNull(query.CachedAuthors);
        Assert.AreEqual(1, query.CachedAuthors.Count);

        Person person = query.CachedAuthors.First();
        Assert.AreEqual("Albert", person.FirstName);
        Assert.AreEqual("Steiner", person.LastName);
        Assert.AreEqual("St. Moritz", person.City);

        Image[] images = await search.LoadImages(ids);
        foreach (Image image in images)
        {
            Assert.AreEqual("Steiner, Albert", image.Urheber);
        }
    }

    [TestMethod]
    [Ignore] // TODO this test fails as there are multiple steiner, hanses and no images that are from BOTH of them.
    public async Task TestLoadIds_search_for_autor_steiner_hans()
    {
        // Arrange
        ISearchService search = new SearchService(new HttpClient(), ArrangeConsoleLogger<SearchService>());
        Query query = Query.ParseSearchText("autor:\"Steiner, Hans\"")!;

        // Act
        ImageIdCollection ids = await search.LoadIds(query, 0, 5);

        // Assert
        Assert.IsTrue(ids.Any());

        // Assert.IsNotNull(query.CachedAuthors);
        // Assert.AreEqual(1, query.CachedAuthors.Count);
    }

    #endregion

    #region Test for the method 'LoadImage(s)'

    [TestMethod]
    public async Task TestLoadImages_5_images_for_Morteratsch()
    {
        // Arrange
        ISearchService search = new SearchService(new HttpClient(), ArrangeConsoleLogger<SearchService>());
        ImageIdCollection ids = await search.LoadIds(Query.FromText("Morteratsch"), 0, 5);

        // Act
        Image[] images = await search.LoadImages(ids);

        // Assert
        foreach (Image image in images)
        {
            Console.WriteLine(image);
            Assert.IsTrue(image.Description.ToLowerInvariant().Contains("morteratsch"));
        }
    }

    #endregion

    #region Test for the method 'ResolveNodeLabel'

    [TestMethod]
    public async Task ResolveNodeLabel()
    {
        // Arrange
        SearchService service = new(new HttpClient(), ArrangeConsoleLogger<SearchService>());

        // Act
        string result = await service.ResolveNodeLabel("http://rdfh.ch/lists/0804/h3umZrvkSD2AI1JV_G2NnQ");

        // Assert
        Console.WriteLine(result);
        Assert.AreEqual("Winter", result);
    }

    #endregion

    #region Tests for the method 'SearchPeople'

    [TestMethod]
    public async Task TestSearchPeople_find_all_the_alberts()
    {
        // Arrange
        SearchService service = new(new HttpClient(), ArrangeConsoleLogger<SearchService>());

        // Act
        Person[] results = await service.SearchPeople("albert");

        // Assert
        Assert.IsTrue(results.Length >= 3);

        Person steiner = results.Single(p => p.LastName == "Steiner");
        Assert.AreEqual("Albert", steiner.FirstName);
        Assert.AreEqual("St. Moritz", steiner.City);
        Assert.AreEqual("http://rdfh.ch/0804/yy-J4xvSQeeiMvEATP_wpA", steiner.Id);

        Person maennchen = results.Single(p => p.LastName == "Maennchen");
        Assert.AreEqual("Albert", maennchen.FirstName);
        Assert.AreEqual("Berlin", maennchen.City);
        Assert.AreEqual("http://rdfh.ch/0804/629EFJ4oTDK9daLn3UV8NQ", maennchen.Id);

        Person scheuing = results.Single(p => p.LastName == "Scheuing");
        Assert.AreEqual("Albert", scheuing.FirstName);
        Assert.AreEqual("http://rdfh.ch/0804/kjnHjyRORnGJiPS1Frl99Q", scheuing.Id);
    }

    [TestMethod]
    public async Task TestSearchPeople_find_the_albert()
    {
        // Arrange
        SearchService service = new(new HttpClient(), ArrangeConsoleLogger<SearchService>());

        // Act
        Person[] results = await service.SearchPeople("maennchen, albert");

        // Assert
        Assert.AreEqual(1, results.Length);
        Person maennchen = results.Single();

        Assert.AreEqual("Albert", maennchen.FirstName);
        Assert.AreEqual("Maennchen", maennchen.LastName);
        Assert.AreEqual("Berlin", maennchen.City);
        Assert.AreEqual("http://rdfh.ch/0804/629EFJ4oTDK9daLn3UV8NQ", maennchen.Id);
    }

    #endregion

    [TestMethod]
    public async Task TestSearch_check_that_both_endpoints_return_consistent_counts()
    {
        // Arrange
        ISearchService service = new SearchService(new HttpClient(), ArrangeConsoleLogger<SearchService>());
        Query query = Query.ParseSearchText("dec:1930 postauto")!;

        // Act
        ImageIdCollection ids = await service.LoadIds(query, 0, 100);
        int? count = await service.Count(query);

        // Assert
        Console.WriteLine("Count (Count API): " + count);
        Console.WriteLine("Id count (Search API): " + ids.Count);

        Assert.AreEqual(count, ids.Count);
    }
}