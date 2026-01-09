namespace ImageSearch.Services;

[Serializable]
public class Image
{
    // rdfs:label
    public string Title { get; init; }

    // dokubib:hasDescription/knora-api:valueAsString
    public string Description { get; init; }

    // dokubib:hasJahrExakt/knora-api:valueAsString
    public string Year { get; init; }

    // dokubib:hasBildnummer/"knora-api:intValueAsInt
    public int? ImageNr { get; init; }

    // knora-api:creationDate
    public DateTime? CreationDate { get; init; }

    // dokubib:hasJahrzehnt/knora-api:dateValueHasStartYear
    public int? Decade { get; init; }

    // knora-api:hasStillImageFileValue/knora-api:fileValueAsUrl/@value
    public string ImageUrl { get; init; }

    // knora-api:hasStillImageFileValue/knora-api:stillImageFileValueHasDimX
    public int Width { get; init; }

    // knora-api:hasStillImageFileValue/knora-api:stillImageFileValueHasDimY
    public int Height { get; init; }

    // dokubib:hasLizenz/knora-api:valueAsString
    public string? License { get; init; }

    // dokubib:hasId/knora-api:valueAsString
    public string? DokubibId { get; init; }

    // dokubib:linkToUrheberValue/knora-api:linkValueHasTarget/rdfs:label
    public string? Urheber { get; init; }

    // dokubib:linkToBildformatValue/knora-api:linkValueHasTarget/rdfs:label
    public string? Bildform { get; init; }

    // dokubib:hasNegativnummer/knora-api:valueAsString
    public string? Negativnummer { get; init; }

    // dokubib:hasErfassungsdatum/knora-api:valueAsString ("GREGORIAN:2017-07-27 CE")
    public string? Erfassungsdatum { get; init; }

    // dokubib:hasJahreszeit/knora-api:listValueAsListNode/@id -> then resolve the label using the node endpoint
    public string? Season { get; init; }

    // dokubib:linkToCopyrightValue/knora-api:linkValueHasTarget/rdfs:label
    public string? CopyRight { get; init; }

    // Derived Properties
    public string Caption { get; init; } = string.Empty;
    public string[] Subject { get; init; } = Array.Empty<string>();

    public Image()
    {
        Title = string.Empty;
        Description = string.Empty;
        Year = string.Empty;
        ImageUrl = string.Empty;
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
}