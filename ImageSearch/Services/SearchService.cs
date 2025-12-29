using System.Collections;
using System.Text;
using System.Text.Encodings.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ImageSearch.Services;

public class SearchService
{
    // The API uses pages of size 25
    private const int ApiPageSize = 25;

    private static readonly HttpClient HttpClient = new();

    public async Task<Ids> LoadIds(Query query, int start, int count)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
        if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));

        QueryBuilder builder = new();

        int startPage = start / ApiPageSize; // Start page number of the API
        int additionalElementsStart = start - ApiPageSize * startPage; // number elements that are loaded additionally at the "front" due to paging
        int pageCount = (int)Math.Ceiling((count + additionalElementsStart) / (double)ApiPageSize);

        // Create tasks for each page
        Task<string[]>[] tasks = Enumerable
            .Range(startPage, pageCount)
            .Select(async page =>
            {
                string sparqlQuery = builder.BuildQuery(query, page);
                string content = await RunQuery(sparqlQuery);
                return ExtractIds(content);
            })
            .ToArray();

        // Await all queries in parallel
        string[][] results = await Task.WhenAll(tasks);

        // Flatten results in page order
        IEnumerable<string> ids = results.SelectMany(r => r);

        string[] relevantIds = ids
            .Skip(additionalElementsStart)
            .Take(count)
            .ToArray();

        return new Ids(relevantIds, tasks.Length);
    }

    public async Task<Image> LoadImage(string imageId)
    {
        string details = await QueryImageDetails(imageId);
        return new Image(details);
    }

    private async Task<string> RunQuery(string query)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));

        using HttpRequestMessage request = new(HttpMethod.Post, "https://api.dasch.swiss/v2/searchextended");

        request.Content = new StringContent(query, Encoding.UTF8, "application/sparql-query");
        request.Headers.Accept.ParseAdd("application/ld+json");

        HttpResponseMessage response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();

        return content;
    }

    private string[] ExtractIds(string content)
    {
        List<string> ids = new();

        if (content == null) throw new ArgumentNullException(nameof(content));

        JObject root =
            JsonConvert.DeserializeObject<JObject>(content)
            ?? throw new NullReferenceException(nameof(root));

        JToken? graph = root["@graph"];

        // @graph might be null, if we find a single image match.
        if (root["dokubib:hasBildnummer"] != null)
        {
            JToken id = root["@id"] ?? throw new NullReferenceException(nameof(id));
            return new[] { id.ToString(), };
        }

        if (graph == null)
        {
            throw new NullReferenceException(nameof(graph));
        }

        foreach (JObject elem in graph.OfType<JObject>().ToArray())
        {
            JToken id =
                elem["@id"]
                ?? throw new NullReferenceException(nameof(id));

            ids.Add(id.ToString());
        }

        return ids.ToArray();
    }

    private async Task<string> QueryImageDetails(string id)
    {
        HttpClient client = new();
        string encode = UrlEncoder.Default.Encode(id);
        using HttpRequestMessage request = new(HttpMethod.Get, $"https://api.dasch.swiss/v2/resources/{encode}");

        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
    
    public readonly struct Ids : IReadOnlyList<string>
    {
        public IReadOnlyList<string> List { get; }
        public int PagesQueried { get; }

        public Ids(string[] list, int pagesQueried)
        {
            List = list;
            PagesQueried = pagesQueried;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => List.Count;

        public string this[int index] => List[index];
    }
}