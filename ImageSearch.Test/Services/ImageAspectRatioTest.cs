using ImageSearch.Services;

namespace ImageSearch.Test.Services;

[TestClass]
public class ImageAspectRatioTest
{
    #region Tests for the method 'SuggestNumberOfImages'

    [TestMethod]
    public void TestSuggestNumberOfImages_one_image_would_not_fit_but_there_needs_to_be_one_image()
    {
        // Act
        int result = ImageAspectRatio.SuggestNumberOfImages(100, 0, 100);

        // Assert
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void TestSuggestNumberOfImages()
    {
        // Act
        int result = ImageAspectRatio.SuggestNumberOfImages(100, 0, 460);

        // Assert
        Assert.AreEqual(3, result);
    }
    #endregion
}