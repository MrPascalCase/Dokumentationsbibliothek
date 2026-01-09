using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using ImageSearch.Services.QueryProcessing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ImageSearch.Services;

public class SearchService : ISearchService, IImageDetailResolver
{
    // The API uses pages of size 25
    private const int ApiPageSize = 25;

    private const string SearchEndpoint = "https://api.dasch.swiss/v2/searchextended";
    private const string CountEndpoint = "https://api.dasch.swiss/v2/searchextended/count";
    private const string ResourcesEndpoint = "https://api.dasch.swiss/v2/resources";
    private const string NodeEndpoint = "https://api.dasch.swiss/v2/node";

    private readonly ILogger<SearchService>? _logger;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, string> _nodeLabelCache;
    private readonly ImageBuilder _imageBuilder;

    public SearchService(HttpClient httpClient, ILogger<SearchService>? logger)
    {
        _logger = logger;
        _httpClient = httpClient;
        _nodeLabelCache = new ConcurrentDictionary<string, string>();
        _imageBuilder = new ImageBuilder(this);
    }

    public async Task<int?> Count(ImageQuery query)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));

        QueryProcessor processor = new();
        _logger?.LogTrace($"Loading Count for query='{query}'...");

        string sparqlQuery = processor.BuildQuery(query);
        string content = await RunQuery(sparqlQuery, CountEndpoint);
        int? count = ExtractCount(content);
        if (count == null)
        {
            _logger?.LogWarning($"Failed to get the count for query='{query}'.");
        }
        else
        {
            _logger?.LogTrace($"Received the count {count} for query='{query}'.");
        }

        return count;
    }

    public async Task<ImageIdCollection> LoadIds(string query, int start, int count)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
        if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));

        _logger?.LogTrace($"Loading Ids {start} to {start + count} for query='{query}'");

        int startPage = start / ApiPageSize; // Start page number of the API
        int additionalElementsStart = start - ApiPageSize * startPage; // number elements that are loaded additionally at the "front" due to paging
        int pageCount = (int)Math.Ceiling((count + additionalElementsStart) / (double)ApiPageSize);

        // Create tasks for each page
        Task<string[]>[] tasks = Enumerable
            .Range(startPage, pageCount)
            .Select(async page =>
            {
                string queryWithOffset = query + $"OFFSET {page}";
                string content = await RunQuery(queryWithOffset, SearchEndpoint);
                string[] ids = ExtractIds(content);
                foreach (string id in ids)
                {
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        _logger?.LogWarning($"Query returned Image Ids which are null or whitespace (id='{id}', query='{queryWithOffset}').");
                    }
                }

                return ids.Where(id => !string.IsNullOrWhiteSpace(id)).ToArray();
            })
            .ToArray();

        // Await all queries in parallel
        string[][] results = await Task.WhenAll(tasks);

        // Flatten results in page order
        string[] ids = results.SelectMany(r => r).ToArray();

        string[] relevantIds = ids
            .Skip(additionalElementsStart)
            .Take(count)
            .ToArray();

        _logger?.LogTrace($"Using {relevantIds.Length} Ids of {ids.Length} Ids received from {tasks.Length} page(s).");
        return new ImageIdCollection(relevantIds, tasks.Length);
    }

    public async Task<Image?> LoadImage(string imageId)
    {
        if (string.IsNullOrWhiteSpace(imageId)) throw new ArgumentNullException(nameof(imageId));

        string details = await QueryImageDetails(imageId);
        if (_imageBuilder.IsValidImage(details, out string? error))
        {
            Image img = await _imageBuilder.BuildImage(details);
            return img;
        }

        _logger?.LogWarning($"Image with Id='{imageId}' cannot be loaded: {error}{Environment.NewLine} content:{details}");
        return null;
    }

    public async Task<string> ResolveNodeLabel(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId)) throw new ArgumentNullException(nameof(nodeId));

        if (_nodeLabelCache.TryGetValue(nodeId, out string? cached)) return cached;

        string encoded = UrlEncoder.Default.Encode(nodeId);
        using HttpRequestMessage request = new(HttpMethod.Get, $"{NodeEndpoint}/{encoded}");
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        JObject node = JsonConvert.DeserializeObject<JObject>(json)!;
        string label = node["rdfs:label"]?.Value<string>() ?? "<nicht definiert>";
        _nodeLabelCache[nodeId] = label;

        return label;
    }

    private async Task<string> RunQuery(string query, string endpoint)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));

        using HttpRequestMessage request = new(HttpMethod.Post, endpoint);

        request.Content = new StringContent(query, Encoding.UTF8, "application/sparql-query");
        request.Headers.Accept.ParseAdd("application/ld+json");

        Stopwatch sw = Stopwatch.StartNew();
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        _logger?.LogInformation($"Response form {endpoint} arrived in {sw.ElapsedMilliseconds} ms.");

        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();

        return content;
    }

    private string[] ExtractIds(string content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        JObject root = JsonConvert.DeserializeObject<JObject>(content) ?? throw new NullReferenceException(nameof(root));
        JToken? graph = root["@graph"];
        if (graph == null)
        {
            // @graph might be null, if we find a single image match.
            JToken? id = root["@id"];
            if (id != null)
            {
                return new[] { id.ToString(), };
            }
        }

        if (graph == null)
        {
            _logger?.LogWarning("Unable to extract Ids from content. content='{content}'", content);
            return Array.Empty<string>();
        }

        List<string> ids = new();
        foreach (JObject elem in graph.OfType<JObject>().ToArray())
        {
            JToken id = elem["@id"] ?? throw new NullReferenceException(nameof(id));
            ids.Add(id.ToString());
        }

        return ids.ToArray();
    }

    private int? ExtractCount(string content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        //  content should look something like this:
        //
        // {
        //     "schema:numberOfItems": 76,
        //     "@context": {
        //         "schema": "http://schema.org/"
        //     }
        // }

        JObject root = JsonConvert.DeserializeObject<JObject>(content) ?? throw new NullReferenceException(nameof(root));
        return root["schema:numberOfItems"]?.Value<int?>();
    }

    private async Task<string> QueryImageDetails(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

        string encoded = UrlEncoder.Default.Encode(id);
        using HttpRequestMessage request = new(HttpMethod.Get, $"{ResourcesEndpoint}/{encoded}");

        HttpResponseMessage response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to load the image with id='{id}'. StatusCode: {response.StatusCode}. Reason: {response.ReasonPhrase}.");
        }

        return await response.Content.ReadAsStringAsync();
    }


    async Task<string> IImageDetailResolver.ResolveSeasonNodeLabel(string nodeId)
    {
        return await ResolveNodeLabel(nodeId);
    }
}