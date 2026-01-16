using System.CodeDom.Compiler;

namespace ImageSearch.Services.QueryProcessing;

internal class DescriptionFilter : FilterComponent
{
    private readonly string _query;

    internal DescriptionFilter(int filterNumber, string query) : base(filterNumber)
    {
        _query = query;
    }

    internal override void AddConstruct(IndentedTextWriter writer)
    {
        writer.WriteLine($"?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasDescription> ?prop{FilterNumber} .");
    }

    internal override void AddFilter(IndentedTextWriter writer)
    {
        writer.WriteLine($"?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasDescription> ?prop{FilterNumber} .");
        writer.WriteLine($"?prop{FilterNumber} <http://api.knora.org/ontology/knora-api/v2#valueAsString> ?prop{FilterNumber}Literal .");
        writer.WriteLine($"FILTER regex(?prop{FilterNumber}Literal, \"{_query}\"^^<http://www.w3.org/2001/XMLSchema#string>, \"i\") .");
    }
}