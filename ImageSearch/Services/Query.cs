namespace ImageSearch.Services;

public class Query
{
    public string? Text { get; set; }
    public int? ImageNr { get; set; }

    public override string ToString()
    {
        List<string> elements = new();
        if (!string.IsNullOrWhiteSpace(Text))
        {
            elements.Add($"Text={Text}");
        }

        if (ImageNr != null)
        {
            elements.Add($"ImageNr={ImageNr}");
        }

        return string.Join(" ,", elements);
    }
}