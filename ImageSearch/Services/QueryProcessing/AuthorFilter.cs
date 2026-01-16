using System.CodeDom.Compiler;
using ImageSearch.Services.Dto;

namespace ImageSearch.Services.QueryProcessing;

public class AuthorFilter : FilterComponent
{
    private readonly IReadOnlyList<Person> _authors;

    public AuthorFilter(int filterNumber, IEnumerable<Person> authors) : base(filterNumber)
    {
        _authors = authors.Reverse().ToArray();
    }

    internal override void AddConstruct(IndentedTextWriter writer)
    {
        writer.WriteLine($"?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#linkToUrheber> ?urheber{FilterNumber} .");
    }

    internal override void AddFilter(IndentedTextWriter writer)
    {
        writer.WriteLine($"?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#linkToUrheber> ?urheber{FilterNumber} .");
        writer.WriteLine("FILTER (");
        writer.Indent++;

        foreach (Person author in _authors.Skip(1))
        {
            writer.WriteLine($"?urheber{FilterNumber} = <{author.Id}> || ");
        }

        writer.WriteLine($"?urheber{FilterNumber} = <{_authors[0].Id}>");

        writer.Indent--;
        writer.WriteLine(")");
    }
}