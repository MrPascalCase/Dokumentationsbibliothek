using System.Net;
using System.Text.Encodings.Web;

namespace ImageSearch.Services.Dto;

[Serializable]
public class Query : IEquatable<Query>
{
    public IReadOnlyList<string> Terms { get; init; } = Array.Empty<string>();

    public int? ImageNr { get; init; }
    public int? Decade { get; init; }
    public IReadOnlyList<string> Subject { get; init; } = Array.Empty<string>();
    public string? Author { get; init; }
    
    public IReadOnlyList<Person>? CachedAuthors { get; set; } 
    
    private static readonly string[] DecadeIdentifiers = { "dekade", "dec", "decade", "dek", };
    private static readonly string[] ImageNrIdentifiers = { "bildnr", "img", "image", "bild", };
    private static readonly string[] SubjectIdentifiers = { "thema", "subj", "subject", };
    private static readonly string[] AuthorIdentifiers = { "autor", "author", };

    private static readonly char[] DescriptionSeparators = { ',', ' ', '\t', '\n', '\r', ';', };
    private static readonly char[] SubjectSeparators = { '>', ',', };

    public static Query FromText(string text)
    {
        return new Query { Terms = new[] { text, }, };
    }

    public override string ToString()
    {
        return ToCanonicalSearchText();
    }

    public static Query? ParseSearchText(string text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        if (string.IsNullOrWhiteSpace(text)) return null;

        string[] elements = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        int? decade = null;
        int? imageNr = null;
        List<string> description = new();
        string[] subject = Array.Empty<string>();
        string? author = null;

        foreach (string element in elements)
        {
            if (TryParseDecade(element, out int dec))
            {
                decade = dec;
            }
            else if (TryParseImageNr(element, out int img))
            {
                imageNr = img;
            }
            else if (TryParseSubject(element, out string[] sub))
            {
                subject = sub;
            }
            else if (TryParseAuthor(element, out string aut))
            {
                author = aut;
            }
            else
            {
                description.Add(element);
            }
        }

        return new Query { Decade = decade, ImageNr = imageNr, Terms = description, Subject = subject, Author = author, };
    }

    public string ToCanonicalSearchText()
    {
        string result = string.Empty;

        if (Decade != null) result += $" {DecadeIdentifiers[0]}:{Decade}";
        if (ImageNr != null) result += $" {ImageNrIdentifiers[0]}:{ImageNr}";

        if (Subject.Any())
        {
            result += $" {SubjectIdentifiers[0]}:{string.Join(SubjectSeparators[0], Subject)}";
        }

        if (!string.IsNullOrWhiteSpace(Author))
        {
            result += $" {AuthorIdentifiers[0]}:'{Author}'";
        }

        foreach (string description in Terms)
        {
            result += $" {description}";
        }

        return result.Trim();
    }

    public static Query? ParseUrl(string query)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (string.IsNullOrWhiteSpace(query)) return null;

        if (query.StartsWith("?"))
        {
            query = query.Substring(1);
        }

        int? decade = null;
        int? imageNr = null;
        List<string> description = new();
        string[] subject = Array.Empty<string>();
        string? author = null;

        string[] components = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (string component in components)
        {
            string[] parts = component.Split('=', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new Exception($"Invalid url query ('{query}'): component '{component}' must by split by '=' into 2 parts.");
            }

            string key = parts[0].ToLowerInvariant();
            if (DecadeIdentifiers.Contains(key))
            {
                decade = int.TryParse(parts[1], out int dec) ? dec : throw new Exception($"'{parts[1]}' is not a valid decade.");
            }

            if (ImageNrIdentifiers.Contains(key))
            {
                imageNr = int.TryParse(parts[1], out int img) ? img : throw new Exception($"'{parts[1]}' is not a valid image number.");
            }

            if (SubjectIdentifiers.Contains(key))
            {
                string value = parts[1];
                subject = value.Split(SubjectSeparators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            }

            if (AuthorIdentifiers.Contains(key))
            {
                string value = parts[1];
                author = WebUtility.UrlDecode(value);
            }

            if (key == "query")
            {
                description.AddRange(
                    parts[1]
                        .Split(DescriptionSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(WebUtility.UrlDecode)!);
            }
        }

        return new Query { Decade = decade, ImageNr = imageNr, Terms = description, Subject = subject, Author = author, };
    }

    public string ToUrl()
    {
        string url = "?";

        if (Terms.Any()) url += "query=" + string.Join(",", Terms.Select(UrlEncoder.Default.Encode));
        if (ImageNr != null) url += $"&{ImageNrIdentifiers[0]}={ImageNr}";
        if (Decade != null) url += $"&{DecadeIdentifiers[0]}={Decade}";
        if (Subject.Any()) url += $"&{SubjectIdentifiers[0]}={string.Join(",", Subject)}";
        if (!string.IsNullOrWhiteSpace(Author)) url += $"&{AuthorIdentifiers[0]}={UrlEncoder.Default.Encode(Author)}";

        if (url == "?") return string.Empty;
        return url;
    }

    public string ToDescription()
    {
        // Is used for the text
        // <p>Für die Suche nach <ToDescription()> wurden <Count> Bilder gefunden.</p>

        string searchTerms = string.Empty;
        if (Terms.Any()) searchTerms += $"\"{Terms[0]}\"";
        foreach (string elem in Terms.Skip(1).Reverse().Skip(1).Reverse()) searchTerms += $", \"{elem}\"";
        if (Terms.Skip(1).Any()) searchTerms += $" und \"{Terms.Last()}\"";

        List<string> elements = new() { searchTerms, };
        if (Decade != null)
        {
            elements.Add(InParenthesis($"Dekade: {Decade}"));
        }

        if (ImageNr != null)
        {
            elements.Add(InParenthesis($"Bildnummer: {ImageNr}"));
        }

        if (Subject.Any())
        {
            elements.Add(InParenthesis($"Thema: {string.Join(SubjectSeparators[0], Subject)}"));
        }

        if (!string.IsNullOrWhiteSpace(Author))
        {
            elements.Add(InParenthesis($"Author: '{Author}'"));
        }

        return string.Join(" ", elements);

        string InParenthesis(string text)
        {
            bool useParenthesis = Terms.Any();
            return useParenthesis ? $"({text})" : text;
        }
    }

    private static bool TryParseDecade(string element, out int decade)
    {
        string lower = element.ToLowerInvariant();
        foreach (string identifier in DecadeIdentifiers)
        {
            if (lower.StartsWith(identifier + ":"))
            {
                string value = element.Substring((identifier + ":").Length);
                if (int.TryParse(value, out decade)) return true;
            }
        }

        decade = 0;
        return false;
    }

    private static bool TryParseImageNr(string element, out int imageNr)
    {
        string lower = element.ToLowerInvariant();
        foreach (string identifier in ImageNrIdentifiers)
        {
            if (lower.StartsWith(identifier + ":"))
            {
                string value = element.Substring((identifier + ":").Length);
                if (int.TryParse(value, out imageNr)) return true;
            }
        }

        imageNr = 0;
        return false;
    }

    private static bool TryParseSubject(string element, out string[] subject)
    {
        string lower = element.ToLowerInvariant();
        foreach (string identifier in SubjectIdentifiers)
        {
            if (lower.StartsWith(identifier + ":"))
            {
                string value = element.Substring((identifier + ":").Length);
                subject = value.Split(SubjectSeparators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                return true;
            }
        }

        subject = Array.Empty<string>();
        return false;
    }

    private static bool TryParseAuthor(string element, out string? author)
    {
        string lower = element.ToLowerInvariant();
        foreach (string identifier in AuthorIdentifiers)
        {
            if (lower.StartsWith(identifier + ":"))
            {
                string value = element.Substring((identifier + ":").Length);
                if (value.ToCharArray().All(c => char.IsWhiteSpace(c) || c == '\'' || c == '"'))
                {
                    author = null;
                    return true; // author is not set, however do not treat "autor:" as a search term.
                }
                
                while (true)
                {
                    if ((value.StartsWith("\"") && value.EndsWith("\""))
                        || (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                        continue;
                    }

                    break;
                }

                author = value;
                return true;
            }
        }

        author = null;
        return false;
    }

    public bool Equals(Query? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return ImageNr == other.ImageNr
               && Decade == other.Decade
               && Author == other.Author
               && Terms
                   .OrderBy(d => d, StringComparer.InvariantCultureIgnoreCase)
                   .SequenceEqual(other.Terms.OrderBy(d => d, StringComparer.InvariantCultureIgnoreCase))
               && Subject
                   .Select(s => s.ToLowerInvariant())
                   .SequenceEqual(other.Subject.Select(s => s.ToLowerInvariant()));
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Query)obj);
    }

    public override int GetHashCode()
    {
        HashCode hash = new();

        hash.Add(ImageNr);
        hash.Add(Decade);
        hash.Add(Author);

        foreach (string description in Terms.OrderBy(d => d, StringComparer.InvariantCultureIgnoreCase))
        {
            hash.Add(description, StringComparer.InvariantCultureIgnoreCase);
        }

        foreach (string subject in Subject)
        {
            hash.Add(subject, StringComparer.InvariantCultureIgnoreCase);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(Query? left, Query? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Query? left, Query? right)
    {
        return !Equals(left, right);
    }
}