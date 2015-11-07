using System;

namespace INFOIBV
{
	public static class ImageOperations
	{
		public static void applyKernel(double[,] image, double[,] kernel) {
			for (int x = 0; x < image.GetLength(0); x++) {
				for (int y = 0; y < image.GetLength(1); y++) {
					double val = 0;
					double bright = 0;
					for (int a = Math.Max(x - image.GetLength(0) + 1, -kernel.GetLength(0) / 2); a <= Math.Min(x, kernel.GetLength(0) / 2 - 1); a++) {
						for (int b = Math.Max(y - image.GetLength(1) + 1, -kernel.GetLength(1) / 2); b <= Math.Min(y, kernel.GetLength(1) / 2 - 1); b++) {
							val += image [x - a, y - b] * kernel[kernel.GetLength(0) / 2 + a, kernel.GetLength(1) / 2 + b];
							bright += kernel[kernel.GetLength(0) / 2 + a, kernel.GetLength(1) / 2 + b];
						}
					}
					image [x, y] = val / bright;
				}
			}
		}

		public static double[,] genGaussianKernel(double sigma, int width, int height) {
			double[,] gauss = new double[width, height];
			double norm = 0;
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					gauss [x, y] = Math.Exp (-(double)(Math.Pow (x - width / 2, 2) + Math.Pow (y - height / 2, 2)) / (2 * sigma * sigma)) 
						/ (2 * Math.PI * sigma * sigma);
					norm += gauss [x, y];
				}
			}
			double res = 0;
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					gauss [x, y] = gauss[x, y] / norm;
					res += gauss [x, y];
				}
			}
			return gauss;
		}
		
		public static void treshold(double[,] image, double treshold=0.5) 
			// Tresholds the image; makes every pixel with a brightness lower than
			// the treshold black, and every pixel with a brightness higher than
			// the treshold white.
		{
			for (int x = 0; x < image.GetLength(0); x++)
			{
				for (int y = 0; y < image.GetLength(1); y++)
				{
					double pixelColor = image[x, y];
					if (pixelColor > treshold)
					{
						image[x, y] = 1;
					}
					else
					{
						image[x, y] = 0;
					}
				}
			}
		}

		public static void complement(double[,] image)
			// Complements the image; R is now RMAX - R,
			// G is GMAX - G and B is BMAX - B
		{
			for (int x = 0; x < image.GetLength(0); x++)
			{
				for (int y = 0; y < image.GetLength(1); y++)
				{
					image [x, y] = Math.Max (0, 1 - image [x, y]);
				}
			}
		}

		public static void dilate(double[,] image, int offset = 1, bool reversed = false)
			// Dilates the image with a square struturing element of size (2*offset+1)x(2offset+1),
			// if reversed is true, this function is an erosion (a dilation of the complement)
		{
			double[,] orig = (double[,]) image.Clone();

			for (int x = 0; x < image.GetLength(0); x++)
			{
				for (int y = 0; y < image.GetLength(1); y++)
				{
					for (int i = 0; i <= offset; i++)
					{
						for (int j = 0; j <= offset; j++)
						{
							if (x + i < image.GetLength(0) && y + j < image.GetLength(1))
							{
								if ((reversed == false && image[x+i,y+j] < orig[x,y]) || 
								    (reversed && (image[x+i,y+j]> orig[x,y]))) {
									image [x + i, y + j] = orig [x, y];
								}
							}
							if (x + i < image.GetLength(0) && y - j >= 0)
							{
								if ((reversed == false && image[x+i,y-j] < orig[x,y]) || 
								    (reversed && (image[x+i,y-j] > orig[x,y]))) {
									image [x + i, y - j] = orig [x, y];
								}
							}
							if (x - i >= 0 && y + j < image.GetLength(1))
							{
								if ((reversed == false && image[x-i,y+j]< orig[x,y]) || 
								    (reversed && (image[x-i,y+j] > orig[x,y]))) {
									image [x - i, y + j] = orig [x, y];
								}
							}
							if (x - i >= 0 && y - j >= 0)
							{
								if ((reversed == false && image[x-i,y-j] < orig[x,y]) || 
								    (reversed && (image[x-i,y-j] > orig[x,y]))) {
									image [x - i, y - j] = orig [x, y];
								}
							}
						}
					}
				}
			}
		}

		public static void erode(double[,] image, int offset = 1)
			// This function erodes the image with a structuring element of size (2*offset+1)x(2*offset+1)
		{
			dilate(image, offset, true);
		}

		public static void close(double[,] image, int offset = 1)
			// This function does a closing on the image with a structuring element of size (2*offset+1)x(2*offset+1)
		{
			dilate(image, offset);
			erode(image, offset);
		}

		public static void open(double[,] image, int offset = 1)
			// This function does an opening on the image with a structuring element of size (2*offset+1)x(2*offset+1)
		{
			erode(image, offset);
			dilate(image, offset);
		}

		public static void findEdges(double[,] image)
			// This function finds edges by subtracting the erosion from the original image, on a black-and-white image
		{
			double[,] erosion = (double[,])image.Clone();
			erode(erosion);

			dilate(image);

			for (var x = 0; x < image.GetLength (0); x++) {
				for (var y = 0; y < image.GetLength (1); y++) {
					image [x, y] -= erosion [x, y];
				}
			}
		}
	}
}

