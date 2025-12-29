namespace DokubibImageSearch.Services;

public class QueryBuilder
{
    public string BuildQuery(Query imgQuery, int page)
    {
        if (imgQuery.ImageNr != null)
        {
            return BuildQueryByImageNumber();
        }

        return BuildLikeQuery(imgQuery, page);
    }

    private static string BuildLikeQuery(Query imgQuery, int page)
    {
        string query =
            $$"""
              PREFIX knora-api: <http://api.knora.org/ontology/knora-api/v2#>
              PREFIX dokubib: <http://api.dasch.swiss/ontology/0804/dokubib/v2#>

              CONSTRUCT {
                ?mainRes knora-api:isMainResource true .
                ?mainRes dokubib:hasDescription ?prop0 .
              }
              WHERE {
                ?mainRes a knora-api:Resource .
                { ?mainRes a dokubib:Bild . }
                UNION { ?mainRes a dokubib:Bildformat . }
                UNION { ?mainRes a dokubib:Person . }
              
                ?mainRes dokubib:hasDescription ?prop0 .
                ?prop0 knora-api:valueAsString ?prop0Literal .
              
                FILTER regex(?prop0Literal, "{{imgQuery.Text}}", "i") .
              }
              
              OFFSET {{page}}
              """;

        return query;
    }

    private static string BuildQueryByImageNumber()
    {
        string query =
            """
            PREFIX knora-api: <http://api.knora.org/ontology/knora-api/v2#>
            PREFIX dokubib: <http://api.dasch.swiss/ontology/0804/dokubib/v2#>
            CONSTRUCT {
            ?mainRes knora-api:isMainResource true .
            ?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasBildnummer> ?prop0 .
            } WHERE {
            ?mainRes a knora-api:Resource .
            { ?mainRes a dokubib:Bild . } UNION { ?mainRes a dokubib:Bildformat . } UNION { ?mainRes a dokubib:Person . }
            ?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasBildnummer> ?prop0 .
            ?prop0 <http://api.knora.org/ontology/knora-api/v2#intValueAsInt> ?prop0Literal .
            FILTER (?prop0Literal = "13661"^^<http://www.w3.org/2001/XMLSchema#integer>) .
            }

            OFFSET 0
            """;

        return query;
    }
}