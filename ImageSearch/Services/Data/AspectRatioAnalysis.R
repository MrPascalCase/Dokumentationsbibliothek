path = "C:\\Users\\User\\RiderProjects\\Dokumentationsbibliothek\\ImageSearch.GetImageAspectRatio\\bin\\Debug\\net9.0\\img_ratios.csv"

data <- read.csv(
  file = path,
  sep = ";",
  header = TRUE,
  stringsAsFactors = FALSE
  )

head(data)

ratios <- data$Ratio.W.H.
summary(ratios)

hist(
  ratios,
  breaks = 100,
  main = "Image Aspect Ratios (W/H)",
  xlab = "Aspect Ratio",
  col = "lightgray",
  border = "white"
  )


set.seed(42)          # reproducibility
N <- 100000           # number of simulated rows
k <- 1                # images per row

row_widths <- replicate(
  N,
  sum(sample(ratios, k, replace = TRUE))
)

hist(
  row_widths,
  breaks = 200,
  main = "Distribution of Row Width (4 Images)",
  xlab = "Total Width (sum of aspect ratios)",
  col = "lightgray",
  border = "white"
)