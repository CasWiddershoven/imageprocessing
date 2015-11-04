using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace INFOIBV
{
	public enum Dir { N, E , S , W, Stay

	}
	public static class Ext {
		public static int GetDX(this Dir dir) {
			switch (dir) {
			case Dir.N:
			case Dir.S:
				return 0;
			case Dir.W:
				return -1;
			case Dir.E:
				return 1;
			default:
				return 0;
			}
		}
		public static int GetDY(this Dir dir) {
			switch (dir) {
			case Dir.W:
			case Dir.E:
				return 0;
			case Dir.N:
				return -1;
			case Dir.S:
				return 1;
			default:
				return 0;
			}
		}
	}
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;

        public INFOIBV()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
           if (openImageDialog.ShowDialog() == DialogResult.OK)             // Open File Dialog
            {
                string file = openImageDialog.FileName;                     // Get the file name
                imageFileName.Text = file;                                  // Show file name
                if (InputImage != null) InputImage.Dispose();               // Reset image
                InputImage = new Bitmap(file);                              // Create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // Dimension check
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                    pictureBox1.Image = (Image) InputImage;                 // Display input image
            }
        }


		private void applyKernel(int[,] image, double[,] kernel) {
			for (int x = 0; x < image.GetLength(0); x++) {
				for (int y = 0; y < image.GetLength(1); y++) {
					int val = 0;
					for (int a = Math.Max(x - image.GetLength(0) + 1, -kernel.GetLength(0) / 2); a <= Math.Min(x, kernel.GetLength(0) / 2 - 1); a++) {
						for (int b = Math.Max(y - image.GetLength(1) + 1, -kernel.GetLength(1) / 2); b <= Math.Min(y, kernel.GetLength(1) / 2 - 1); b++) {
							int imgvar = image [x - a, y - b];
							val += (int)(imgvar * kernel[kernel.GetLength(0) / 2 + a, kernel.GetLength(1) / 2 + b]);
						}
					}
					image [x, y] = val;
				}
			}
		}

		private double[,] genGaussianKernel(double sigma, int width, int height) {
			double[,] gauss = new double[2 * width - 1, 2 * height - 1];
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					gauss [x, y] = Math.Exp (-(double)(Math.Pow (x - width, 2) + Math.Pow (y - height, 2)) / (2 * sigma * sigma)) 
											* 256 / (2 * Math.PI * sigma * sigma);
					gauss [2 * width - x - 2, 2 * height - y - 2] = gauss[x, y];
				}
			}
			Console.WriteLine (gauss);
			return gauss;
		}

		private int[,] toGrayArray(Color[,] image) {
			int[,] grayArray = new int[image.GetLength(0), image.GetLength(1)];
			for (int x = 0; x < image.GetLength(0); x++)
			{
				for (int y = 0; y < image.GetLength(1); y++)
				{
					grayArray [x, y] = (int)(image [x, y].R * 0.3 + image [x, y].G * 0.59 + image [x, y].B * 0.11);
				}
			}
			return grayArray;
		}

		private void imgFromIntArr(int[,] values, Color[,] target) {
			for (int x = 0; x < target.GetLength(0); x++)
			{
				for (int y = 0; y < target.GetLength(1); y++)
				{
					int val = Math.Min (255, values [x, y]);
					target [x, y] = Color.FromArgb(val, val, val);
				}
			}
		}

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // Get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // Create array to speed-up operations (Bitmap functions are very slow)

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // Copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Image[x, y] = InputImage.GetPixel(x, y);                // Set pixel color in array at (x,y)
                }
            }

            //==========================================================================================
            // TODO: include here your own code
            // example: create a negative image
            /*for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image[x, y];                         // Get the pixel color at coordinate (x,y)
                    Color updatedColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B); // Negative image
                    Image[x, y] = updatedColor;                             // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                              // Increment progress bar
                }
            }*/


			int[,] imgArr = toGrayArray (Image);
			double[,] gaussKernel = new double[,] {
				{0.00000067,0.00002292,0.00019117,0.00038771,0.00019117,0.00002292,0.00000067},
				{0.00002292,0.00078634,0.00655965,0.01330373,0.00655965,0.00078633,0.00002292},
				{0.00019117,0.00655965,0.05472157,0.11098164,0.05472157,0.00655965,0.00019117},
				{0.00038771,0.01330373,0.11098164,0.22508352,0.11098164,0.01330373,0.00038771},
				{0.00019117,0.00655965,0.05472157,0.11098164,0.05472157,0.00655965,0.00019117},
				{0.00002292,0.00078633,0.00655965,0.01330373,0.00655965,0.00078633,0.00002292},
				{0.00000067,0.00002292,0.00019117,0.00038771,0.00019117,0.00002292,0.00000067},
			};
			applyKernel (imgArr, gaussKernel);
			//imgFromIntArr (imgArr, Image);
			findEdges (imgArr);
			treshold (imgArr, 40);
			//complement (Image);
		

			//var img = toGrayArray (Image);

			/*var pixel = findStartingPixel (img);
		
			var dirs = MarchSquares (img, pixel.X, pixel.Y);

			int rx = pixel.X;
			int ry = pixel.Y;
		
		
			
	
			foreach (var dir in dirs) {
				Image [rx, ry] = Color.Red;
				rx += dir.GetDX ();
				ry += dir.GetDY ();
			}*/

			imgFromIntArr (imgArr, Image);
            //==========================================================================================

            // Copy array to output Bitmap
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    OutputImage.SetPixel(x, y, Image[x, y]);               // Set the pixel color at coordinate (x,y)
                }
            }
            
            pictureBox2.Image = (Image)OutputImage;                         // Display output image
            progressBar.Visible = false;                                    // Hide progress bar
        }
        
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }

		private void treshold(int[,] image, int treshold=127) 
		// Tresholds the image; makes every pixel with a brightness lower than
		// the treshold black, and every pixel with a brightness higher than
		// the treshold white.
		{
			for (int x = 0; x < image.GetLength(0); x++)
			{
				for (int y = 0; y < image.GetLength(1); y++)
				{
					int pixelColor = image[x, y];
					if (pixelColor > treshold)
					{
						image[x, y] = 255;
					}
					else
					{
						image[x, y] = 0;
					}
				}
			}
		}
			

		private void complement(int[,] image)
		// Complements the image; R is now RMAX - R,
		// G is GMAX - G and B is BMAX - B
		{
			for (int x = 0; x < image.GetLength(0); x++)
			{
				for (int y = 0; y < image.GetLength(1); y++)
				{
					int pixelColor = image[x, y];
					image [x, y] = Math.Max (0, 255 - image [x, y]);
				}
			}
		}

		private void dilate(int[,] image, int offset = 1, bool reversed = false)
		// Dilates the image with a square struturing element of size (2*offset+1)x(2offset+1),
		// if reversed is true, this function is an erosion (a dilation of the complement)
		{
			int[,] orig = (int[,]) image.Clone();

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

		private void gaussian(int[,] imagE)
		{
		}

		private void gradient(int[,] image)
		{
			var image2 = (int[,]) image.Clone ();

			dilate (image);
			erode (image2);

			for (var x = 0; x < image.GetLength (0); x++) {
				for (var y = 0; y < image.GetLength (1); y++) { 
					image [x, y] -= image2 [x, y];
				}
			}
		}

		private void erode(int[,] image, int offset = 1)
		// This function erodes the image with a structuring element of size (2*offset+1)x(2*offset+1)
		{
			dilate(image, offset, true);
		}

		private void close(int[,] image, int offset = 1)
		// This function does a closing on the image with a structuring element of size (2*offset+1)x(2*offset+1)
		{
			dilate(image, offset);
			erode(image, offset);
		}

		private void open(int[,] image, int offset = 1)
		// This function does an opening on the image with a structuring element of size (2*offset+1)x(2*offset+1)
		{
			erode(image, offset);
			dilate(image, offset);
		}

		private void findEdges(int[,] image)
		// This function finds edges by subtracting the erosion from the original image, on a black-and-white image
		{
			var erosion = (int[,])image.Clone();
			erode(erosion);

			dilate(image);

			for (var x = 0; x < image.GetLength (0); x++) {
				for (var y = 0; y < image.GetLength (1); y++) {
					image [x, y] -= erosion [x, y];
				}
			}

		}




		private void MarchSquares(int[,] image) {
		}


		// finds the circumfrence of an object starting at x,y
		private IList<Dir> MarchSquares(int[,] image, int vx, int vy) {
			int val = getMarchingSquare (image, vx, vy);
			if (val == 0 || val == 15) {
				throw new Exception ("Initial coordinates don't start on a perimter");
			}

			int x = vx;
			int y = vy;

			IList<Dir> dirs = new List<Dir> ();
			Dir prev = Dir.Stay;

			do {
				Dir dir;
				switch(getMarchingSquare(image,x,y)) {
				case 1: dir = Dir.N; break;
				case 2: dir = Dir.E; break;
				case 3: dir = Dir.E; break;
				case 4: dir = Dir.W; break;
				case 5: dir = Dir.N; break;
				case 6: dir = /* saddle point */ prev == Dir.N ? Dir.W : Dir.E; break;
				case 7: dir = Dir.E; break;
				case 8: dir = Dir.S; break;
				case 9: dir = /* saddle point */ prev == Dir.E ? Dir.N : Dir.S; break;
				case 10: dir = Dir.S; break;
				case 11: dir = Dir.S; break;
				case 12: dir = Dir.W; break;
				case 13: dir = Dir.N; break;
				case 14: dir = Dir.W; break;
				default: throw new Exception("Wut");
				}
				dirs.Add(dir);

				x+= dir.GetDX();
				y+= dir.GetDY();
				prev = dir;


			} while (x != vx || y != vy);
			return dirs;
		}



		private int getMarchingSquare(int[,] image, int x, int y)
		{
			// TODO what if x and y are 0
			int res = 0;
			if (image [x-1, y-1] == 0)
				res |= 1;
			if (image [x, y-1] == 0)
				res |= 2;
			if (image [x-1, y] == 0)
				res |= 4;
			if (image [x, y] == 0)
				res |= 8;
			return res;
		}


		/// <summary>
		/// Find the top left pixel of an object. Discards and previously visited pixels to make it possible to find a new object
		/// every time
		/// </summary>
		/// <returns>The starting pixel.</returns>
		/// <param name="image">Image.</param>
		/// <param name="discardedSet">Discarded set.</param>
		private Point findStartingPixel(int[,] image, int x = 0)
		{
			for (int i = x; i < image.GetLength (0); i++) {
				for (int j = 0; j < image.GetLength (1); j++) {
					if (image [i, j] == 0) {
						return new Point (i, j);
					}
				}
			}
			throw new Exception ("Lolwat");

		}
    }
}
