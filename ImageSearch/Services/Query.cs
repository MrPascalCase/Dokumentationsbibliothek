namespace ImageSearch.Services;

public class ImageQuery
{
    public IReadOnlyList<string> Texts { get; }
    public int? ImageNr { get; }
    public int? Decade { get; }

    public static ImageQuery FromText(string text)
    {
        return new ImageQuery(description: new[] { text });
    }

    public ImageQuery(int? decade = null, int? imageNr = null, IEnumerable<string>? description = null)
    {
        Texts = description?.ToArray() ?? Array.Empty<string>();
        ImageNr = imageNr;
        Decade = decade;
    }

    public override string ToString()
    {
        List<string> elements = new();
        foreach (string text in Texts)
        {
            elements.Add($"Text={text}");
        }

        if (ImageNr != null)
        {
            elements.Add($"ImageNr={ImageNr.Value}");
        }

        if (Decade != null)
        {
            elements.Add($"Decade={Decade.Value}");
        }

        return string.Join(" ,", elements);
    }
}