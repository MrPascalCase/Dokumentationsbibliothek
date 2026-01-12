using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ImageSearch.Services.Subjects;

public class SubjectList : IReadOnlyList<SubjectNode>
{
    private readonly HttpClient _httpClient;
    private SubjectNode[] _nodes = Array.Empty<SubjectNode>();

    public SubjectList(HttpClient httpClient)
    {
        _httpClient = httpClient;
        Initialize().GetAwaiter().GetResult();
    }

    public async Task Initialize()
    {
        // TODO: we should probably get this id from the ontology?
        string url = "https://api.dasch.swiss/v2/lists/http%3A%2F%2Frdfh.ch%2Flists%2F0804%2Fw5dBw8W1TymqNHZQ3dq3wQ";
        using HttpRequestMessage request = new(HttpMethod.Get, url);

        HttpResponseMessage response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        List<SubjectNode> nodes = ParseListTree(content);
        _nodes = nodes.ToArray();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (SubjectNode node in this)
        {
            sb.AppendLine(node.ToString());
        }

        return sb.ToString();
    }

    private static List<SubjectNode> ParseListTree(string content)
    {
        JObject root = JsonConvert.DeserializeObject<JObject>(content) ?? throw new NullReferenceException(nameof(root));
        JToken? subNodesToken = root["knora-api:hasSubListNode"];
        if (subNodesToken == null)
        {
            return new List<SubjectNode>();
        }

        return ParseSubListNodes(subNodesToken);
    }

    private static List<SubjectNode> ParseSubListNodes(JToken token)
    {
        List<SubjectNode> result = new();

        // knora-api:hasSubListNode can be an array OR a single object
        if (token.Type == JTokenType.Array)
        {
            foreach (JToken child in token)
            {
                SubjectNode node = ParseSingleNode((JObject)child);
                result.Add(node);
            }
        }
        else if (token.Type == JTokenType.Object)
        {
            SubjectNode node = ParseSingleNode((JObject)token);
            result.Add(node);
        }

        return result;
    }

    private static SubjectNode ParseSingleNode(JObject nodeObj)
    {
        SubjectNode node = new()
        {
            Id = nodeObj["@id"]?.Value<string>(),
            Label = nodeObj["rdfs:label"]?.Value<string>(),
            Position = nodeObj["knora-api:listNodePosition"]?.Value<int?>(),
        };

        JToken? childrenToken = nodeObj["knora-api:hasSubListNode"];
        if (childrenToken != null)
        {
            List<SubjectNode> children = ParseSubListNodes(childrenToken);
            node.Children.AddRange(children);
        }

        return node;
    }

    public IEnumerator<SubjectNode> GetEnumerator()
    {
        return ((IEnumerable<SubjectNode>)_nodes).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _nodes.Length;

    public SubjectNode this[int index] => _nodes[index];
}