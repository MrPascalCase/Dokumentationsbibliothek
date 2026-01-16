using ImageSearch.Services.Dto;
using ImageSearch.Services.QueryProcessing;

namespace ImageSearch.Test.Services.QueryProcessing;

[TestClass]
public class QueryProcessorTest
{
    [TestMethod]
    public void TestBuildQuery_search_for_schnee_and_post_in_the_1950s()
    {
        // Arrange
        QueryProcessor processor = new();
        Query query = new() { Decade = 1950, Terms = new[] { "schnee", "post", }, };

        // Act
        string sparqlQuery = processor.BuildQuery(query);
        Console.WriteLine(sparqlQuery);

        // Assert
        string expected =
            """
            PREFIX knora-api: <http://api.knora.org/ontology/knora-api/v2#>
            PREFIX dokubib: <http://api.dasch.swiss/ontology/0804/dokubib/v2#>
            CONSTRUCT {
                ?mainRes knora-api:isMainResource true .
                ?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasJahrzehnt> ?prop0 .
                ?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasDescription> ?prop1 .
                ?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasDescription> ?prop2 .
            } WHERE {
                ?mainRes a knora-api:Resource .
                { ?mainRes a dokubib:Bild . } UNION { ?mainRes a dokubib:Bildformat . } UNION { ?mainRes a dokubib:Person . }
                ?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasJahrzehnt> ?prop0 .
                FILTER(knora-api:toSimpleDate(?prop0) = "GREGORIAN:1950-01-01"^^<http://api.knora.org/ontology/knora-api/simple/v2#Date>) .
                ?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasDescription> ?prop1 .
                ?prop1 <http://api.knora.org/ontology/knora-api/v2#valueAsString> ?prop1Literal .
                FILTER regex(?prop1Literal, "schnee"^^<http://www.w3.org/2001/XMLSchema#string>, "i") .
                ?mainRes <http://api.dasch.swiss/ontology/0804/dokubib/v2#hasDescription> ?prop2 .
                ?prop2 <http://api.knora.org/ontology/knora-api/v2#valueAsString> ?prop2Literal .
                FILTER regex(?prop2Literal, "post"^^<http://www.w3.org/2001/XMLSchema#string>, "i") .
            }
            """;

        Assert.AreEqual(expected, sparqlQuery);
    }
}