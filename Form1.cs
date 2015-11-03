﻿using System;
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

		private void treshold(Color[,] image, double treshold=0.5) 
			// Tresholds the image; makes every pixel with a brightness lower than
			// the treshold black, and every pixel with a brightness higher than
			// the treshold white.
        {
            for (int x = 0; x < image.GetLength(0); x++)
            {
                for (int y = 0; y < image.GetLength(1); y++)
                {
                    Color pixelColor = image[x, y];
                    if (pixelColor.GetBrightness() > treshold)
                    {
                        image[x, y] = Color.White;
                    }
                    else
                    {
                        image[x, y] = Color.Black;
                    }
                }
            }
        }

		private bool[,] imageToBoolArray(Color[,] image)
			// Converts a black-and-white image to a bool array,
			// true values correspond to black pixels in the image
			// and false values correspond to white pixels in the image
		{
			bool[,] boolArray = new bool[image.GetLength(0), image.GetLength(1)];
			for (int x = 0; x < image.GetLength(0); x++) {
				for (int y = 0; y < image.GetLength(1); y++) {
					if (image [x, y] == Color.White) {
						boolArray [x, y] = false;
					} else {
						boolArray [x, y] = true;
					}
				}
			}
			return boolArray;
		}

        private void complement(Color[,] image)
			// Complements the image; R is now RMAX - R,
			// G is GMAX - G and B is BMAX - B
        {
            for (int x = 0; x < image.GetLength(0); x++)
            {
                for (int y = 0; y < image.GetLength(1); y++)
                {
                    Color pixelColor = image[x, y];
					image [x, y] = pixelColor == Color.Black ? Color.White : Color.Black;
                }
            }
        }

        private void dilate(Color[,] image, int offset = 1, bool reversed = false)
			// Dilates the image with a square struturing element of size (2*offset+1)x(2offset+1),
			// if reversed is true, this function is an erosion (a dilation of the complement)
        {
            Color[,] orig = (Color[,]) image.Clone();

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
								if ((reversed == false && image[x+i,y+j].GetBrightness() < orig[x,y].GetBrightness()) || 
								    (reversed && (image[x+i,y+j].GetBrightness() > orig[x,y].GetBrightness()))) {
									image [x + i, y + j] = orig [x, y];
								}
                            }
                            if (x + i < image.GetLength(0) && y - j >= 0)
							{
								if ((reversed == false && image[x+i,y-j].GetBrightness() < orig[x,y].GetBrightness()) || 
								    (reversed && (image[x+i,y-j].GetBrightness() > orig[x,y].GetBrightness()))) {
									image [x + i, y - j] = orig [x, y];
								}
                            }
                            if (x - i >= 0 && y + j < image.GetLength(1))
							{
								if ((reversed == false && image[x-i,y+j].GetBrightness() < orig[x,y].GetBrightness()) || 
								    (reversed && (image[x-i,y+j].GetBrightness() > orig[x,y].GetBrightness()))) {
									image [x - i, y + j] = orig [x, y];
								}
                            }
                            if (x - i >= 0 && y - j >= 0)
							{
								if ((reversed == false && image[x-i,y-j].GetBrightness() < orig[x,y].GetBrightness()) || 
								    (reversed && (image[x-i,y-j].GetBrightness() > orig[x,y].GetBrightness()))) {
									image [x - i, y - j] = orig [x, y];
								}
                            }
                        }
                    }
                }
            }
        }

        private void erode(Color[,] image, int offset = 1)
			// This function erodes the image with a structuring element of size (2*offset+1)x(2*offset+1)
        {
            dilate(image, offset, true);
        }

        private void close(Color[,] image, int offset = 1)
			// This function does a closing on the image with a structuring element of size (2*offset+1)x(2*offset+1)
        {
            dilate(image, offset);
            erode(image, offset);
        }

        private void open(Color[,] image, int offset = 1)
			// This function does an opening on the image with a structuring element of size (2*offset+1)x(2*offset+1)
        {
            erode(image, offset);
            dilate(image, offset);
        }

        private void findEdges(Color[,] image)
			// This function finds edges by subtracting the erosion from the original image, on a black-and-white image
        {
            Color[,] erosion = (Color[,])image.Clone();
            erode(erosion);
            dilate(image);

            for (int x = 0; x < image.GetLength(0); x++)
            {
                for (int y = 0; y < image.GetLength(1); y++)
                {
                    if (erosion[x, y] == Color.White)
                    {
                        image[x, y] = Color.Black;
                    }
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



			treshold (Image);
			//complement (Image);
		


			var pixel = findStartingPixel (Image);
		
			var dirs = MarchSquares (Image, pixel.X, pixel.Y);

			int rx = pixel.X;
			int ry = pixel.Y;

			foreach (var dir in dirs) {
				Image [rx, ry] = Color.Red;
				rx += dir.GetDX ();
				ry += dir.GetDY ();
			}
		
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


		private void MarchSquares(Color[,] image) {
		}


		// finds the circumfrence of an object starting at x,y
 		private IList<Dir> MarchSquares(Color[,] image, int vx, int vy) {
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



		private int getMarchingSquare(Color[,] image, int x, int y)
		{
			int res = 0;
			if (image [x-1, y-1] == Color.Black)
				res |= 1;
			if (image [x, y-1] == Color.Black)
				res |= 2;
			if (image [x-1, y] == Color.Black)
				res |= 4;
			if (image [x, y] == Color.Black)
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
		private Point findStartingPixel(Color[,] image, int x = 0)
		{
			for (int i = x; i < image.GetLength (0); i++) {
				for (int j = 0; j < image.GetLength (1); j++) {
					if (image [i, j] == Color.Black) {
						return new Point (i, j);
					}
				}
			}
			throw new Exception ("Lolwat");
		
		}
    }
}
