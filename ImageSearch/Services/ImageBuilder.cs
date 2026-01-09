using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ImageSearch.Services;

public class ImageBuilder
{
    private readonly IImageDetailResolver? _resolver;

    public ImageBuilder(IImageDetailResolver? resolver)
    {
        _resolver = resolver;
    }

    public async Task<Image> BuildImage(string content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        JObject root = JsonConvert.DeserializeObject<JObject>(content) ?? throw new NullReferenceException(nameof(root));

        int? imageNr = root["dokubib:hasBildnummer"]?["knora-api:intValueAsInt"]?.Value<int?>();
        string year = root["dokubib:hasJahrExakt"]?["knora-api:valueAsString"]?.Value<string?>() ?? string.Empty;
        DateTime? creationDate = root["knora-api:creationDate"]?["@value"]?.Value<DateTime?>();
        string description = root["dokubib:hasDescription"]?["knora-api:valueAsString"]?.Value<string>() ?? "<keine Beschreibung>";
        int? decade = root["dokubib:hasJahrzehnt"]?["knora-api:dateValueHasStartYear"]?.Value<int?>();
        string title = root["rdfs:label"]?.Value<string?>() ?? "<kein Titel>";
        string? license = root["dokubib:hasLizenz"]?["knora-api:valueAsString"]?.Value<string?>();
        string? dokubibId = root["dokubib:hasId"]?["knora-api:valueAsString"]?.Value<string?>();
        string? urheber = root["dokubib:linkToUrheberValue"]?["knora-api:linkValueHasTarget"]?["rdfs:label"]?.Value<string?>();
        // Bildform = root["dokubib:linkToBildformatValue"]?["knora-api:linkValueHasTarget"]?["rdfs:label"]?.Value<string?>();
        string? negativnummer = root["dokubib:hasNegativnummer"]?["knora-api:valueAsString"]?.Value<string?>();
        string? erfassungsdatum = root["dokubib:hasErfassungsdatum"]?["knora-api:valueAsString"]?.Value<string?>();
        string? copyRight = root["dokubib:linkToCopyrightValue"]?["knora-api:linkValueHasTarget"]?["rdfs:label"]?.Value<string?>();

        string? seasonNodeId = root["dokubib:hasJahreszeit"]?["knora-api:listValueAsListNode"]?["@id"]?.Value<string?>();
        string season = "<nicht definiert>";
        if (seasonNodeId != null && _resolver != null)
        {
            season = await _resolver.ResolveSeasonNodeLabel(seasonNodeId);
        }

        string imageUrl = root["knora-api:hasStillImageFileValue"]?["knora-api:fileValueAsUrl"]?["@value"]?.Value<string?>()
                          ?? throw new NullReferenceException(nameof(imageUrl));
        int width = root["knora-api:hasStillImageFileValue"]?["knora-api:stillImageFileValueHasDimX"]?.Value<int?>()
                    ?? throw new NullReferenceException(nameof(width));
        int height = root["knora-api:hasStillImageFileValue"]?["knora-api:stillImageFileValueHasDimY"]?.Value<int?>()
                     ?? throw new NullReferenceException(nameof(height));

        ProcessTitle(title, imageNr, out string[]? subject, out string caption);

        return new Image
        {
            ImageNr = imageNr,
            Year = year,
            CreationDate = creationDate,
            Description = description,
            Decade = decade,
            Title = title,
            License = license,
            DokubibId = dokubibId,
            Urheber = urheber,
            Negativnummer = negativnummer,
            Erfassungsdatum = erfassungsdatum,
            CopyRight = copyRight,
            Season = season,
            ImageUrl = imageUrl,
            Width = width,
            Height = height,
            Caption = caption,
            Subject = subject,
        };
    }

    public bool IsValidImage(string content, out string error)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        JObject root = JsonConvert.DeserializeObject<JObject>(content) ?? throw new NullReferenceException(nameof(root));

        if (root["knora-api:hasStillImageFileValue"]?["knora-api:fileValueAsUrl"]?["@value"]?.Value<string?>() == null)
        {
            error = "Image file URL is missing (knora-api:hasStillImageFileValue\\knora-api:fileValueAsUrl\\@value)";
            return false;
        }

        if (root["knora-api:hasStillImageFileValue"]?["knora-api:stillImageFileValueHasDimX"]?.Value<int?>() == null)
        {
            error = "Image dim x is missing (knora-api:hasStillImageFileValue\\knora-api:stillImageFileValueHasDimX)";
            return false;
        }

        if (root["knora-api:hasStillImageFileValue"]?["knora-api:stillImageFileValueHasDimY"]?.Value<int?>() == null)
        {
            error = "Image dim y is missing (knora-api:hasStillImageFileValue\\knora-api:stillImageFileValueHasDimY)";
            return false;
        }

        error = string.Empty;
        return true;
    }

    internal bool ProcessTitle(string title, int? number, out string[] subject, out string caption)
    {
        if (title == null) throw new ArgumentNullException(nameof(title));

        subject = Array.Empty<string>();
        caption = string.Empty;
        title = title.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            return false;
        }

        if (number != null)
        {
            for (int i = 0; i < 20; i++)
            {
                // Expected numeric suffix
                string expectedSuffix = $"({number.ToString()!.PadLeft(i, '0')})";

                if (title.EndsWith(expectedSuffix))
                {
                    // Remove numeric suffix
                    title = title[..^expectedSuffix.Length].Trim();
                    break;
                }
            }
        }

        string[] parts = title.Split('-', StringSplitOptions.TrimEntries);
        List<string> subjectParts = new();

        int index = 0;

        // Collect ALL-CAPS parts as subject
        for (; index < parts.Length; index++)
        {
            string part = parts[index];

            if (IsAllCaps(part))
            {
                subjectParts.Add(ToTitleCase(part));
            }
            else
            {
                break;
            }
        }

        // There must be something left for the caption
        if (index >= parts.Length)
        {
            return false;
        }

        // Remaining parts form the caption
        caption = string.Join("-", parts[index..]);
        subject = subjectParts.ToArray();

        return true;
    }

    private bool IsAllCaps(string value)
    {
        return value.All(c => !char.IsLetter(c) || char.IsUpper(c));
    }

    private string ToTitleCase(string value)
    {
        return string.Join(
            " ",
            value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => char.ToUpper(w[0]) + w[1..].ToLower())
        );
    }
}