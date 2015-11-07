using System;

namespace INFOIBV
{
	public static class ImageOperations
	{
		/// <summary>
		/// applyKernel applies a given kernel to a given image
		/// </summary>
		/// <returns>void</returns>
		/// <param name="image">The image the kernel is applied to</param>
		/// <param name="kernel">The kernel to apply</param>
		public static void applyKernel(double[,] image, double[,] kernel) {
			for (int x = 0; x < image.GetLength(0); x++) {
				for (int y = 0; y < image.GetLength(1); y++) {
					double val = 0; // Value the pixel should get after appliecation
					double bright = 0; // Total brightness of the applied kernel. To normalize, we must divide by this brightness.
					for (int a = Math.Max(x - image.GetLength(0) + 1, -kernel.GetLength(0) / 2); 
					     	a <= Math.Min(x, kernel.GetLength(0) / 2 - 1); 
					     		a++) { // From the most left pixel possible to the most right pixel possible
						for (int b = Math.Max(y - image.GetLength(1) + 1, -kernel.GetLength(1) / 2); 
						     	b <= Math.Min(y, kernel.GetLength(1) / 2 - 1); 
						     		b++) { // From the most top pixel possible to the most bottom pixel possible
							val += image [x - a, y - b] * kernel[kernel.GetLength(0) / 2 + a, kernel.GetLength(1) / 2 + b];
							bright += kernel[kernel.GetLength(0) / 2 + a, kernel.GetLength(1) / 2 + b];
						}
					}
					image [x, y] = val / bright; // Normalize and set value
				}
			}
		}

		/// <summary>
		/// genGaussianKernel creates a Gaussian kernel of specified width and height,
		/// and with a specified standard deviation sigma
		/// </summary>
		/// <returns>The Gaussian kernel</returns>
		/// <param name="sigma">The standard deviation of the kernel</param>
		/// <param name="width">The width of the kernel</param>
		/// <param name="height">The height of the kernel</param>
		public static double[,] genGaussianKernel(double sigma, int width, int height) {
			double[,] gauss = new double[width, height]; // The kernel
			double norm = 0; // The total brightness of the kernel, for normalization
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					gauss [x, y] = Math.Exp (-(double)(Math.Pow (x - width / 2, 2) + Math.Pow (y - height / 2, 2)) / (2 * sigma * sigma)) 
						/ (2 * Math.PI * sigma * sigma); // Gauss = exp((x^2+y^2)/(2sigma^2))/(2*sigma^2)
					norm += gauss [x, y];
				}
			}
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					gauss [x, y] = gauss[x, y] / norm; // Normalize
				}
			}
			return gauss;
		}

		/// <summary>
		/// Tresholds the image; makes every pixel with a brightness lower than
		/// the treshold black, and every pixel with a brightness higher than
		/// the treshold white.
		/// </summary>
		/// <returns>void</returns>
		/// <param name="image">The image that will be tresholded</param>
		/// <param name="treshold">The treshold value, between 0 (black) and 1 (white)</param>
		public static void treshold(double[,] image, double treshold=0.5) 
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

		/// <summary>
		/// Complements the (grey) image; its value is 
		/// now 1 (max) minus its original value
		/// </summary>
		/// <returns>void</returns>
		/// <param name="image">The image to complement</param>
		public static void complement(double[,] image)
		{
			for (int x = 0; x < image.GetLength(0); x++)
			{
				for (int y = 0; y < image.GetLength(1); y++)
				{
					image [x, y] = 1 - image [x, y];
				}
			}
		}

		/// <summary>
		/// Dilates the image with a square struturing element of size (2*offset+1)x(2offset+1),
		/// if reversed is true, this function is an erosion (a dilation of the complement)
		/// </summary>
		/// <returns>void</returns>
		/// <param name="image">The image to dilate</param>
		/// <param name="offset">The size of the structuring element</param>
		/// <param name="reversed">Whether the dilation should be reversed (erosion)</param>
		public static void dilate(double[,] image, int offset = 1, bool reversed = false)
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
							if (x + i < image.GetLength(0) && y + j < image.GetLength(1)) // If the pixel is still on the image
							{
								if ((reversed == false && image[x+i,y+j] < orig[x,y]) || 
								    (reversed && (image[x+i,y+j]> orig[x,y]))) { // If it is lighter (or if it's and erosion and it's darker)
									image [x + i, y + j] = orig [x, y];
								}
							}
							if (x + i < image.GetLength(0) && y - j >= 0) // If the pixel is still on the image
							{
								if ((reversed == false && image[x+i,y-j] < orig[x,y]) || 
								    (reversed && (image[x+i,y-j] > orig[x,y]))) { // If it is lighter (or if it's and erosion and it's darker)
									image [x + i, y - j] = orig [x, y];
								}
							}
							if (x - i >= 0 && y + j < image.GetLength(1)) // If the pixel is still on the image
							{
								if ((reversed == false && image[x-i,y+j]< orig[x,y]) || 
								    (reversed && (image[x-i,y+j] > orig[x,y]))) { // If it is lighter (or if it's and erosion and it's darker)
									image [x - i, y + j] = orig [x, y];
								}
							}
							if (x - i >= 0 && y - j >= 0) // If the pixel is still on the image
							{
								if ((reversed == false && image[x-i,y-j] < orig[x,y]) || 
								    (reversed && (image[x-i,y-j] > orig[x,y]))) { // If it is lighter (or if it's and erosion and it's darker)
									image [x - i, y - j] = orig [x, y];
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// This function erodes the image with a structuring element of size (2*offset+1)x(2*offset+1)
		/// </summary>
		/// <returns>void</returns>
		/// <param name="image">The image to erode</param>
		/// <param name="offset">The size of the structuring element</param>
		public static void erode(double[,] image, int offset = 1)
		{
			dilate(image, offset, true); // Erosion is just reversed dilation
		}

		/// <summary>
		/// This function does a closing on the image with a structuring element of size (2*offset+1)x(2*offset+1)
		/// </summary>
		/// <returns>void</returns>
		/// <param name="image">The image to close</param>
		/// <param name="offset">The size of the structuring element</param>
		public static void close(double[,] image, int offset = 1)
		{
			dilate(image, offset);
			erode(image, offset);
		}

		/// <summary>
		/// This function does an opening on the image with a structuring element of size (2*offset+1)x(2*offset+1)
		/// </summary>
		/// <returns>void</returns>
		/// <param name="image">The image to open</param>
		/// <param name="offset">The size of the structuring element</param>
		public static void open(double[,] image, int offset = 1)
		{
			erode(image, offset);
			dilate(image, offset);
		}

		/// <summary>
		/// This function finds edges by subtracting the erosion from the original image, on a black-and-white image
		/// </summary>
		/// <returns>void</returns>
		/// <param name="image">The image to find edges in</param>
		public static void findEdges(double[,] image)
		{
			double[,] erosion = (double[,])image.Clone();
			erode(erosion);

			dilate(image);

			for (var x = 0; x < image.GetLength (0); x++) {
				for (var y = 0; y < image.GetLength (1); y++) {
					image [x, y] -= erosion [x, y]; // Find edges is just dilation minus erosion
				}
			}
		}
	}
}

