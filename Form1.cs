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
					image [x, y] = Color.FromArgb ((byte)~pixelColor.R, (byte)~pixelColor.G, (byte)~pixelColor.B);
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
			findEdges (Image);
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
		private IList<Point> MarchSquares(bool[,] image, int x, int y) {
			var px = x;
			var py = y;
			var stepX = 0;
			var stepY = 0;
			var prevX = 0;
			var prevY = 0;

			var closed = false;

			IList<Point> points = new List<Point> ();

			double length = 0;

			/* +---+---+
			 * | 1 | 2 |
			 * +---+---+
			 * | 8 | 4 |
			 * +---+---+
			 */
			while (!closed) {
				var val = getMarchingSquare (image, x, y);

				length += getMarchingSquareLength (val);

				switch (val) {

				/* +---+---+
				*  | 1 |   |
				*  +---+---+
				*  |   |   |
				*  +---+---+
				*/
				case 1:
				/* +---+---+
				*  | 1 |   |
				*  +---+---+
				*  | 8 |   |
				*  +---+---+
				*/
				case 9:
				/* +---+---+
				*  | 1 |   |
				*  +---+---+
				*  | 8 | 4 |
				*  +---+---+
				*/
				case 13:
					stepX = 0;
					stepY = -1;
					break;
				
				/* +---+---+
				*  | 1 | 2 |
				*  +---+---+
				*  |   | 4 |
				*  +---+---+
				*/
				case 7:
				/* +---+---+
				*  |   | 2 |
				*  +---+---+
				*  |   | 4 |
				*  +---+---+
				*/
				case 6:
				/* +---+---+
				*  |   |   |
				*  +---+---+
				*  |   | 4 |
				*  +---+---+
				*/
				case 4:
					stepX = 0;
					stepY = 1;
					break;


				
				/* +---+---+
				*  |   |   |
				*  +---+---+
				*  | 8 |   |
				*  +---+---+
				*/
				case 8:
				/* +---+---+
				*  |   |   |
				*  +---+---+
				*  | 8 | 4 |
				*  +---+---+
				*/
				case 12:
				/* +---+---+
				*  |   | 2 |
				*  +---+---+
				*  | 8 | 4 |
				*  +---+---+
				*/
				case 14:
					stepX = -1;
					stepY = 0;
					break;


				/* +---+---+
				*  |   | 2 |
				*  +---+---+
				*  |   |   |
				*  +---+---+
				*/
				case 2:
				/* +---+---+
				*  | 1 | 2 |
				*  +---+---+
				*  |   |   |
				*  +---+---+
				*/
				case 3:
				/* +---+---+
				*  | 1 | 2 |
				*  +---+---+
				*  | 8 |   |
				*  +---+---+
				*/
				case 11:
					stepX = 1;
					stepY = 0;
					break;

				/* +---+---+
				*  |   | 2 |
				*  +---+---+
				*  | 8 |   |
				*  +---+---+
				*/
				case 10: // saddle point

					// left after up
					if (prevX == 0 && prevY == -1) {
						stepX = -1;
						stepY = 0;

						// else right
					} else {
						stepX = 1;
						stepY = 0;
					}
					break;
				
				

				/* +---+---+
				*  | 1 |   |
				*  +---+---+
				*  |   | 4 |
				*  +---+---+
				*/
				case 5:
					// right after up
					if (prevX == 1 && prevY == 0) {
						stepX = 0;
						stepY = -1;
						// else left
					} else {
						stepX = 0;
						stepY = 1;
					}
					break;
				}

				px += stepX;
				py += stepY;

				points.Add (new Point (px, py));



				prevX = stepX;
				prevY = stepY;

				if (x == px && y == py) {
					closed = true;
				}

			}
			return points;
		}

		/**
		 * Returns one of 16 marching squares given a position
		 */

		private double getMarchingSquareLength(int marchingSquare) {
			switch (marchingSquare) {
			case 1:
			case 2:
			case 4:
			case 5:
			case 7:
			case 8:
			case 10:
			case 11:
			case 13:
			case 14:
				return Math.Sqrt (2);
			case 3:
			case 6:
			case 9:
			case 12:
				return 2;
			case 15:
			default:
				return 0;
			
			
			}
		}
		private int getMarchingSquare(bool[,] image, int x, int y)
		{
			/* +---+---+
			 * | 1 | 2 |
			 * +---+---+
			 * | 8 | 4 |
			 * +---+---+
			 * 
			 * A value between 0 and 15 describes each possible square configuration
			 * */

			int res = 0;
			if (image [x-1, y-1]) {
				res += 1;
			}
			if (image [x, y - 1]) {
				res += 2;
			}
			if (image [x - 1, y]) {
				res += 8;
			}

			if (image [x, y]) {
				res += 4;
			}
			return res;
		}


		/// <summary>
		/// Find the top left pixel of an object. Discards and previously visited pixels to make it possible to find a new object
		/// every time
		/// </summary>
		/// <returns>The starting pixel.</returns>
		/// <param name="image">Image.</param>
		/// <param name="discardedSet">Discarded set.</param>
		private Point findStartingPixel(Color[,] image,  ISet<Point> discardedSet)
		{
			for (int i = 0; i < image.GetLength (0); i++) {
				for (int j = 0; j < image.GetLength (1); i++) {
					if (image [i, j] == Color.White && !discardedSet.Contains(new Point(i,j))) {
						return new Point (i, j);
					}
				}
			}
			throw new Exception ("Lolwat");
		
		}
    }
}
