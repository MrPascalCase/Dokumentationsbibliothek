using System.Net;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ImageSearch.Services;

[Serializable]
public class Image
{
    // rdfs:label
    public string Title { get; }

    // dokubib:hasDescription/knora-api:valueAsString
    public string Description { get; }

    // dokubib:hasJahrExakt/knora-api:valueAsString
    public string Year { get; }

    // dokubib:hasBildnummer/"knora-api:intValueAsInt
    public int? ImageNr { get; }

    // knora-api:creationDate
    public DateTime? CreationDate { get; }

    // dokubib:hasJahrzehnt/knora-api:dateValueHasStartYear
    public int? Decade { get; }

    // knora-api:hasStillImageFileValue/knora-api:fileValueAsUrl/@value
    public string ImageUrl { get; }

    // knora-api:hasStillImageFileValue/knora-api:stillImageFileValueHasDimX
    public int Width { get; }

    // knora-api:hasStillImageFileValue/knora-api:stillImageFileValueHasDimY
    public int Height { get; }

    // dokubib:hasLizenz/knora-api:valueAsString
    public string? License { get; }

    // dokubib:hasId/knora-api:valueAsString
    public string? DokubibId { get; }

    // dokubib:linkToUrheberValue/knora-api:linkValueHasTarget/rdfs:label
    public string? Urheber { get; }

    // dokubib:linkToBildformatValue/knora-api:linkValueHasTarget/rdfs:label
    public string? Bildform { get; }

    // dokubib:hasNegativnummer/knora-api:valueAsString
    public string? Negativnummer { get; }

    // dokubib:hasErfassungsdatum/knora-api:valueAsString ("GREGORIAN:2017-07-27 CE")
    public string? Erfassungsdatum { get; }

    // dokubib:hasJahreszeit/knora-api:valueHasUUID
    // H_0_Dzc8Qr2g8C9MMVqTAQ = Winter
    public string? Season { get; }

    // dokubib:linkToCopyrightValue/knora-api:linkValueHasTarget/rdfs:label
    public string? CopyRight { get; }

    // Derived Properties
    public string Caption { get; } = string.Empty;
    public string[] Subject { get; } = Array.Empty<string>();

    public Image(string content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        JObject root = JsonConvert.DeserializeObject<JObject>(content) ?? throw new NullReferenceException(nameof(root));

        ImageNr = root["dokubib:hasBildnummer"]?["knora-api:intValueAsInt"]?.Value<int?>();
        Year = root["dokubib:hasJahrExakt"]?["knora-api:valueAsString"]?.Value<string?>() ?? string.Empty;
        CreationDate = root["knora-api:creationDate"]?["@value"]?.Value<DateTime?>();
        Description = root["dokubib:hasDescription"]?["knora-api:valueAsString"]?.Value<string>() ?? "<keine Beschreibung>";
        Decade = root["dokubib:hasJahrzehnt"]?["knora-api:dateValueHasStartYear"]?.Value<int?>();
        Title = root["rdfs:label"]?.Value<string?>() ?? "<kein Titel>";
        License = root["dokubib:hasLizenz"]?["knora-api:valueAsString"]?.Value<string?>();
        DokubibId = root["dokubib:hasLizenz"]?["knora-api:valueAsString"]?.Value<string?>();
        Urheber = root["dokubib:linkToUrheberValue"]?["knora-api:linkValueHasTarget"]?["rdfs:label"]?.Value<string?>();
        // Bildform = root["dokubib:linkToBildformatValue"]?["knora-api:linkValueHasTarget"]?["rdfs:label"]?.Value<string?>();
        Negativnummer = root["dokubib:hasNegativnummer"]?["knora-api:valueAsString"]?.Value<string?>();
        Erfassungsdatum = root["dokubib:hasErfassungsdatum"]?["knora-api:valueAsString"]?.Value<string?>();
        CopyRight = root["dokubib:linkToCopyrightValue"]?["knora-api:linkValueHasTarget"]?["rdfs:label"]?.Value<string?>();

        Season = root["dokubib:hasJahreszeit"]?["knora-api:valueHasUUID"]?.Value<string?>();
        if (string.Equals(Season?.Trim(), "H_0_Dzc8Qr2g8C9MMVqTAQ", StringComparison.InvariantCultureIgnoreCase))
        {
            Season = "Winter";
        }

        ImageUrl = root["knora-api:hasStillImageFileValue"]?["knora-api:fileValueAsUrl"]?["@value"]?.Value<string?>()
                   ?? throw new NullReferenceException(nameof(ImageUrl));
        Width = root["knora-api:hasStillImageFileValue"]?["knora-api:stillImageFileValueHasDimX"]?.Value<int?>()
                ?? throw new NullReferenceException(nameof(Width));
        Height = root["knora-api:hasStillImageFileValue"]?["knora-api:stillImageFileValueHasDimY"]?.Value<int?>()
                 ?? throw new NullReferenceException(nameof(Height));

        if (ProcessTitle(Title, ImageNr, out string[]? subject, out string caption))
        {
            Caption = caption;
            Subject = subject;
        }
    }

    public string GetThumbnailUrl(int? maxWidth, int? maxHeight)
    {
        if (maxWidth == null && maxHeight == null)
        {
            throw new ArgumentException($"At least {nameof(maxWidth)} or {nameof(maxHeight)} must be provided.");
        }

        if (maxWidth != null) maxWidth = Math.Min(maxWidth!.Value, Width);
        if (maxHeight != null) maxHeight = Math.Min(maxHeight!.Value, Height);

        if (maxHeight == null)
        {
            double ratioWidth = (double)maxWidth!.Value / Width;
            return GetThumbnailUrl(maxWidth.Value, (int)Math.Round(ratioWidth * Height));
        }

        if (maxWidth == null)
        {
            double ratioHeight = (double)maxHeight / Height;
            return GetThumbnailUrl((int)Math.Round(ratioHeight * Width), maxHeight.Value);
        }

        {
            double ratioWidth = (double)maxWidth / Width;
            double ratioHeight = (double)maxHeight / Height;

            if (ratioWidth < ratioHeight)
            {
                return GetThumbnailUrl(maxWidth.Value, (int)Math.Round(ratioWidth * Height));
            }

            return GetThumbnailUrl((int)Math.Round(ratioHeight * Width), maxWidth!.Value);
        }
    }

    private string GetThumbnailUrl(int maxWidth, int maxHeight)
    {
        string pattern = $"{Width},{Height}";
        if (!ImageUrl.Contains(pattern)) throw new Exception($"The image url must contain the pattern '{pattern}' (width,height).");

        string replacement = $"{maxWidth},{maxHeight}";
        return ImageUrl.Replace(pattern, replacement);
    }

    public override string ToString()
    {
        return
            $"{nameof(Title)}: {Title}, "
            + $"{nameof(Description)}: {Description}, "
            + $"{nameof(Year)}: {Year}, "
            + $"{nameof(ImageNr)}: {ImageNr}, "
            + $"{nameof(CreationDate)}: {CreationDate}, "
            + $"{nameof(Decade)}: {Decade}, "
            + $"{nameof(ImageUrl)}: {ImageUrl}, "
            + $"{nameof(Width)}: {Width}, "
            + $"{nameof(Height)}: {Height}, "
            + $"{nameof(License)}: {License}, "
            + $"{nameof(DokubibId)}: {DokubibId}, "
            + $"{nameof(Urheber)}: {Urheber}, "
            + $"{nameof(Bildform)}: {Bildform}, "
            + $"{nameof(Negativnummer)}: {Negativnummer}, "
            + $"{nameof(Erfassungsdatum)}: {Erfassungsdatum}, "
            + $"{nameof(Season)}: {Season}, "
            + $"{nameof(CopyRight)}: {CopyRight}, "
            + $"{nameof(Caption)}: {Caption}, "
            + $"{nameof(Subject)}: {string.Join(">", Subject)}";
    }

    public MarkupString ExplainSearch(string searchText, int contextLength,  string matchStyle)
    {
        if (string.IsNullOrWhiteSpace(searchText)) throw new ArgumentNullException(nameof(searchText));
        if (string.IsNullOrWhiteSpace(matchStyle)) throw new ArgumentNullException(nameof(matchStyle));

        // assume searchText is contained in this.Description (ignore case)
        string description = Description ?? string.Empty;

        int index = description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return new MarkupString(WebUtility.HtmlEncode(description));
        }

        int matchStart = index;
        int matchEnd = index + searchText.Length;

        // Determine context bounds
        // contextLength = characters of context on each side (approximate) 
        int contextStart = Math.Max(0, matchStart - contextLength);
        int contextEnd = Math.Min(description.Length, matchEnd + contextLength);
        
        // Adjust contextStart to sentence boundary
        if (contextStart > 0)
        {
            int sentenceIndex = description.LastIndexOfAny(new[] { '.', '!', '?' }, matchStart);
            if (sentenceIndex >= 0 && sentenceIndex + 1 > contextStart)
            {
                contextStart = sentenceIndex + 1;
            }
        }

// Adjust contextEnd to sentence boundary
        if (contextEnd < description.Length)
        {
            int sentenceIndex = description.IndexOfAny(new[] { '.', '!', '?' }, matchEnd);
            if (sentenceIndex >= 0 && sentenceIndex < contextEnd)
            {
                contextEnd = sentenceIndex;
            }
        }

        // Adjust contextStart to word boundary
        if (contextStart > 0)
        {
            int spaceIndex = description.LastIndexOf(' ', contextStart);
            if (spaceIndex >= 0)
            {
                contextStart = spaceIndex + 1;
            }
        }

        // Adjust contextEnd to word boundary
        if (contextEnd < description.Length)
        {
            int spaceIndex = description.IndexOf(' ', contextEnd);
            if (spaceIndex >= 0)
            {
                contextEnd = spaceIndex;
            }
        }

        string before = description.Substring(contextStart, matchStart - contextStart);
        string match = description.Substring(matchStart, searchText.Length);
        string after = description.Substring(matchEnd, contextEnd - matchEnd);

        bool hasLeadingEllipsis = contextStart > 0;
        bool hasTrailingEllipsis = contextEnd < description.Length;

        string result =
            $"{(hasLeadingEllipsis ? "... " : string.Empty)}" +
            $"{WebUtility.HtmlEncode(before)}" +
            $"<span style=\"{WebUtility.HtmlEncode(matchStyle)}\">{WebUtility.HtmlEncode(match)}</span>" +
            $"{WebUtility.HtmlEncode(after)}" +
            $"{(hasTrailingEllipsis ? " ..." : string.Empty)}";

        return new MarkupString(result);
    }

    internal static bool ProcessTitle(string title, int? number, out string[] subject, out string caption)
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

    private static bool IsAllCaps(string value)
    {
        return value.All(c => !char.IsLetter(c) || char.IsUpper(c));
    }

    private static string ToTitleCase(string value)
    {
        return string.Join(
            " ",
            value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => char.ToUpper(w[0]) + w[1..].ToLower())
        );
    }
}