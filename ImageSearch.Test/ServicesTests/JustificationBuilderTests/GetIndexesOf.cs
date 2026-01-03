using ImageSearch.Services.Justifications;

namespace ImageSearch.Test.ServicesTests.JustificationBuilderTests;

[TestClass]
public class GetIndexesOf
{
    [TestMethod]
    public void TestGetIndexesOf()
    {
        // Arrange
        string input = "abcbabcab";
        //              012345678
        //              '   '  '

        // Act
        int[] result = JustificationBuilder.GetIndexesOf("ab", input);

        // Assert
        CollectionAssert.AreEqual(new[] { 0, 4, 7, }, result);
    }
}