using System.CodeDom.Compiler;
using ImageSearch.Services.Dto;

namespace ImageSearch.Services.QueryProcessing;

public class AuthorFilter : FilterComponent
{
    // Template: Search by the person 'Maennchen, Albert'
    //
    // PREFIX knora-api: <http://api.knora.org/ontology/knora-api/v2#>
    // PREFIX dokubib: <http://api.dasch.swiss/ontology/0804/dokubib/v2#>
    // CONSTRUCT {
    //     ?mainRes knora-api:isMainResource true .
    //     ?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#linkToUrheber> ?prop0 .
    // } WHERE {
    //     ?mainRes a knora-api:Resource .
    //     { ?mainRes a dokubib:Bild . } UNION { ?mainRes a dokubib:Bildformat . } UNION { ?mainRes a dokubib:Person . }
    //     ?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#linkToUrheber> ?prop0 .
    //         ?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#linkToUrheber> <http://rdfh.ch/0804/629EFJ4oTDK9daLn3UV8NQ> .
    // }

    private readonly Person _author;

    public AuthorFilter(int filterNumber, Person author) : base(filterNumber)
    {
        _author = author;
    }

    internal override void AddConstruct(IndentedTextWriter writer)
    {
        writer.WriteLine($"?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#linkToUrheber> ?prop{FilterNumber} .");
    }

    internal override void AddFilter(IndentedTextWriter writer)
    {
        writer.WriteLine($"?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#linkToUrheber> ?prop{FilterNumber} .");
        writer.WriteLine($"?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#linkToUrheber> <{_author.Id}> .");
    }
}