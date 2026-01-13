using System.CodeDom.Compiler;
using System.Text;
using ImageSearch.Services.Dto;

namespace ImageSearch.Services.QueryProcessing;

public class QueryProcessor
{
    private int _nextFilterNumber;

    public string BuildQuery(ImageQuery query)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));

        List<FilterComponent> components = GetComponents(query);
        StringBuilder sb = new();
        IndentedTextWriter writer = new(new StringWriter(sb));

        writer.WriteLine("PREFIX knora-api: <http://api.knora.org/ontology/knora-api/v2#>");
        writer.WriteLine("PREFIX dokubib: <http://api.dasch.swiss/ontology/0804/dokubib/v2#>");

        writer.WriteLine("CONSTRUCT {");
        writer.Indent++;
        writer.WriteLine("?mainRes knora-api:isMainResource true .");
        foreach (FilterComponent component in components) component.AddConstruct(writer);
        writer.Indent--;
        writer.Write("} ");

        writer.WriteLine("WHERE {");
        writer.Indent++;
        writer.WriteLine("?mainRes a knora-api:Resource .");
        writer.WriteLine("{ ?mainRes a dokubib:Bild . } UNION { ?mainRes a dokubib:Bildformat . } UNION { ?mainRes a dokubib:Person . }");
        foreach (FilterComponent component in components) component.AddFilter(writer);
        writer.Indent--;
        writer.WriteLine("}");

        writer.Flush();
        return sb.ToString();
    }

    internal int ReserveFilterNumber()
    {
        return _nextFilterNumber++;
    }

    private List<FilterComponent> GetComponents(ImageQuery query)
    {
        List<FilterComponent> components = new();

        if (query.ImageNr != null) components.Add(new ImageNrFilter(this, query.ImageNr.Value));
        if (query.Decade != null) components.Add(new DecadeFiler(this, query.Decade.Value));
        foreach (string text in query.Description) components.Add(new DescriptionFilter(this, text));

        return components;
    }
}