using System.CodeDom.Compiler;

namespace ImageSearch.Services.QueryProcessing;

internal class ImageNrFilter : FilterComponent
{
    private readonly int _imageNr;

    internal ImageNrFilter(int filterNumber, int imageNr) : base(filterNumber)
    {
        _imageNr = imageNr;
    }

    internal override void AddConstruct(IndentedTextWriter writer)
    {
        writer.WriteLine($"?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasBildnummer> ?prop{FilterNumber} .");
    }

    internal override void AddFilter(IndentedTextWriter writer)
    {
        writer.WriteLine($"?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasBildnummer> ?prop{FilterNumber} .");
        writer.WriteLine($"?prop0 <http://api.knora.org/ontology/knora-api/v2#intValueAsInt> ?prop{FilterNumber}Literal .");
        writer.WriteLine($"FILTER (?prop{FilterNumber}Literal = \"{_imageNr}\"^^<http://www.w3.org/2001/XMLSchema#integer>) .");
    }
}