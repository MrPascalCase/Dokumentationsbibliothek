using ImageSearch.Services.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ImageSearch.Services;

public class PersonBuilder
{
    public Person[] BuildPeople(string content)
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
                return new[] { BuildPerson(content), };
            }
        }

        // if (graph == null)
        // {
        //     _logger?.LogWarning("Unable to extract Ids from content. content='{content}'", content);
        //     return Array.Empty<string>();
        // }

        List<Person> people = new();
        foreach (JObject elem in graph.OfType<JObject>().ToArray())
        {
            Person person = BuildPerson(elem.ToString());
            people.Add(person);
        }

        return people.ToArray();
    }

    private Person BuildPerson(string content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        JObject root = JsonConvert.DeserializeObject<JObject>(content) ?? throw new NullReferenceException(nameof(root));

        string firstname = root["dokubib:hasFirstname"]?["knora-api:valueAsString"]?.Value<string>() ?? "<kein Vorname>";
        string lastname = root["dokubib:hasLastname"]?["knora-api:valueAsString"]?.Value<string>() ?? "<kein Nachname>";
        string city = root["dokubib:hasCity"]?["knora-api:valueAsString"]?.Value<string>() ?? "<keine Ortsangabe>";
        string id = root["@id"]?.Value<string>() ?? "<keine Id>";

        return new Person
        {
            FirstName = firstname,
            LastName = lastname,
            City = city,
            Id = id,
        };
    }
}