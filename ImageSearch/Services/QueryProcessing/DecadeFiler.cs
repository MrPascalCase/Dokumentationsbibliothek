using System.CodeDom.Compiler;

namespace ImageSearch.Services.QueryProcessing;

internal class DecadeFiler : FilterComponent
{
    private readonly int _decade;

    internal DecadeFiler(QueryProcessor processor, int decade) : base(processor)
    {
        _decade = decade;
    }

    internal override void AddConstruct(IndentedTextWriter writer)
    {
        writer.WriteLine($"?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasJahrzehnt> ?prop{FilterNumber} .");
    }

    internal override void AddFilter(IndentedTextWriter writer)
    {
        writer.WriteLine($"?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasJahrzehnt> ?prop{FilterNumber} .");
        writer.WriteLine(
            $"FILTER(knora-api:toSimpleDate(?prop{FilterNumber}) = \"GREGORIAN:{_decade}-01-01\"^^<http://api.knora.org/ontology/knora-api/simple/v2#Date>) .");
    }
}