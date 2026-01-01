namespace ImageSearch.Services;

public static class AspectRatios
{
    public static int SuggestNumberOfImages(int imgHeight, int imgGap, int screenWidth)
    {
        int numberOfImages = 2;
        while (true)
        {
            double size = (numberOfImages - 1) * imgGap + imgHeight * PredictMaxImageWidth(numberOfImages);
            if (size > screenWidth) return numberOfImages - 1;
            numberOfImages++;
        }
    }

    private static double PredictMaxImageWidth(int imgCount)
    {
        // Predicts the maximum width of multiple images in terms of the constant image height.
        // The distribution is initially (1 image) tri-modal (3 clearly distinct peaks). But
        // behaves almost linear by 4–5. Values are taken for the 90% quantile.

        if (imgCount < 10)
        {
            return new[]
                {
                    /* 0 images are */ 0, /* px wide */
                    /* 1 image is at most */ 1.553515, /* as wide as it is high */
                    /* 2 images are at most */ 3.047214, /* times as wide as they are high */
                    /* ... */ 4.504681,
                    5.925536,
                    7.321462,
                    8.696879,
                    10.05039,
                    11.39046,
                    12.74229,
                }
                [imgCount];
        }

        return 0.690588 + 1.337451 * imgCount;
    }
}