using ImageSearch.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ImageSearch.Test.Services;

[TestClass]
public class SearchSessionTests
{
    #region Tests for the method 'SetQuery'

    [TestMethod]
    public async Task TestSetQuery()
    {
        // Arrange
        ImageQuery query = ImageQuery.ParseSearchText("cats")!;

        string[] ids = { "1", "2", "3", "4", "5", };
        ImageIdCollection idCollection = new(ids, 1);

        ISearchService mock = Substitute.For<ISearchService>();
        mock.LoadIds(query, Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(idCollection));
        mock.Count(Arg.Any<ImageQuery>()).Returns(Task.FromResult((int?)5));

        SearchSession session = new(mock, ArrangeConsoleLogger());

        // Act
        await session.SetQuery(query, false);

        // Assert
        Assert.AreEqual(5, session.TotalImageCount);
        Assert.AreEqual(ImageQuery.ParseSearchText("cats"), session.CurrentQuery);

        #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        mock.Received().LoadIds(query, Arg.Any<int>(), Arg.Any<int>());
        mock.Received().Count(query);

        #pragma warning restore CS4014
    }

    #endregion

    #region Tests for the method 'FetchData'

    [TestMethod]
    public async Task TestFetchData()
    {
        // Arrange
        ImageQuery query = ImageQuery.ParseSearchText("cats")!;

        string[] ids = { "1", "2", "3", "4", "5", };
        ImageIdCollection idCollection = new(ids, 1);

        ISearchService mock = Substitute.For<ISearchService>();
        mock.LoadIds(query, Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(idCollection));
        mock.Count(Arg.Any<ImageQuery>()).Returns(Task.FromResult((int?)50));

        SearchSession session = new(mock, ArrangeConsoleLogger());
        await session.SetQuery(query, false);
        int resultsAddedCalled = 0;
        session.OnResultsAdded += _ => { resultsAddedCalled++; };

        // Act
        await session.FetchData(2);

        // Assert
        Assert.AreEqual(1, resultsAddedCalled);
    }
    
    [TestMethod]
    public async Task TestFetchData_queue_fetch_data_of_the_same_generation_in_parallel()
    {
        // Arrange
        ImageQuery query = ImageQuery.ParseSearchText("cats")!;

        string[] ids = { "1", "2", "3", "4", "5", };
        ImageIdCollection idCollection = new(ids, 1);

        ISearchService mock = Substitute.For<ISearchService>();
        mock.LoadIds(query, Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(idCollection));
        mock.Count(Arg.Any<ImageQuery>()).Returns(Task.FromResult((int?)50));

        SearchSession session = new(mock, ArrangeConsoleLogger());
        await session.SetQuery(query, false);
        int resultsAddedCalled = 0;
        session.OnResultsAdded += _ => { resultsAddedCalled++; };

        // Act
        Task[] tasks = Enumerable
            .Range(0, 5)
            .Select(async _ =>
            {
                await session.FetchData(2);
            })
            .ToArray();
        
        await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(1, resultsAddedCalled);
    }

    #endregion

    private ILogger<SearchSession> ArrangeConsoleLogger()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(Configure);
        ILogger<SearchSession> logger = loggerFactory.CreateLogger<SearchSession>();
        return logger;

        void Configure(ILoggingBuilder builder) =>
            builder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole();
    }
}