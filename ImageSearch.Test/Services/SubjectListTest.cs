using ImageSearch.Services.Subjects;

namespace ImageSearch.Test.Services;

[TestClass]
public class SubjectListTest
{
    [TestMethod]
    [Ignore]
    public void Create_the_list_of_subjects()
    {
        // Arrange
        SubjectList list = new(new HttpClient());

        // Act
        File.WriteAllText("Subjects.txt", list.ToString());
    }
}