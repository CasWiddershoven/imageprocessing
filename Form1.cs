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

		private void saveButton_Click(object sender, EventArgs e)
		{
			if (OutputImage == null) return;                                // Get out if no output image
			if (saveImageDialog.ShowDialog() == DialogResult.OK)
				OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
		}

		private Random random = new Random ();
		public Color GetNextColor()
		{
			Color color = Color.FromArgb(random.Next(256),random.Next(256),random.Next(256));
			return color;
		}

		public struct Dice
		{
			public BoundingBox.MinBox minBox;
			public HashSet<Point> innerPoints;
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
            progressBar.Maximum = 8;
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
			/*
			 * windowing ->
			 * gaussian -> 
			 * gradient -> 
			 * treshold -> 
			 * until all squares found:
			 	* findStartingPixel -> 
			 	* march squares -> 
			 	* calculate surface ->
			 	* fit square -> 
			 	* if surface : fitted square nearly 1:
					* Add object to squares
			 * for each square:
			 		* Hough transform over fitted square
			 		* Count circles
			 */
			double[,] imgArr = toGrayArray (Image);

			// windowing
			int[] values = new int[256];
			for (int x = 0; x < imgArr.GetLength(0); x++) {
				for (int y = 0; y < imgArr.GetLength(1); y++) {
					values [(int)(imgArr [x, y] * 256)] += 1;
				}
			}
			float total = imgArr.GetLength (0) * imgArr.GetLength (1);
			float cum = 0;
			float min = 0;
			float max = 1;
			for (int i = 0; i < 256; i++) {
				cum += values [i];
				if (min == 0 && cum / total >= 0.1) {
					min = (float)i / 256;
				}
				if (max == 0 && cum / total >= 0.9) {
					max = i / 256;
					break;
				}
			}
			ImageOperations.window (imgArr, min, max);
			progressBar.PerformStep ();

			// gaussian
			double[,] gaussKernel = ImageOperations.genGaussianKernel (3, 9, 9);
			ImageOperations.applyKernel (imgArr, gaussKernel);
			progressBar.PerformStep ();

			// gradient
			ImageOperations.findEdges (imgArr);
			progressBar.PerformStep ();

			// treshold
			ImageOperations.treshold (imgArr, 0.1);
			imgFromArr (imgArr, Image);
			progressBar.PerformStep ();

			// until all squares found, findStartingPixel:
			Point? startingPoint;
			int startx = 0;
			int starty = 0;
			HashSet<Point> discardedSet = new HashSet<Point> ();
			List<Dice> dice = new List<Dice> ();
			while ((startingPoint = BoundaryDetection.findStartingPixel(imgArr, discardedSet, startx, starty)).HasValue) {
				// march squares
				int x = startingPoint.Value.X;
				int y = startingPoint.Value.Y;
				Point point;
				IList<Dir> dirs;
				try {
					dirs = BoundaryDetection.MarchSquares (imgArr, x, y);
				} catch {
					starty++;
					discardedSet.Add(new Point(x, y));
					continue;
				}
				HashSet<Point> edge = new HashSet<Point> ();
				Dictionary<int, Point> inner = new Dictionary<int, Point> (); // Yeah, point.X is now the min x on that y coordinate and point.Y the max x.
				BoundingBox bbox = new BoundingBox (Image.GetLength (0), Image.GetLength (1));
				foreach (Dir dir in dirs) {
					Image [x, y] = Color.Red;
					point = new Point (x, y);
					edge.Add (point);
					bbox.addPoint (point);
					if (inner.ContainsKey (y)) {
						int newX = Math.Min (x, inner [y].X);
						int newY = Math.Max (x, inner [y].Y);
						inner [y] = new Point (newX, newY);
					} else {
						inner.Add (y, new Point (x, x));
					}
					x += dir.GetDX ();
					y += dir.GetDY ();
				}

				// calculate surface
				int innerVolume = 0;
				HashSet<Point> innerPoints = new HashSet<Point> ();
				for (int yi = (int)bbox.boxes[0].top.Y; yi <= bbox.boxes[0].bottom.Y; yi++) {
					innerVolume += inner [yi].Y - inner [yi].X + 1;
					for (int xi = (int)inner[yi].X; xi <= inner[yi].Y; xi++) {
						discardedSet.Add (new Point (xi, yi));
						innerPoints.Add (new Point (xi, yi));
					}
				}
				// fit square
				BoundingBox.MinBox minBox = bbox.getMinBoundingBox ();

				// if surface : fitted square nearly 1:
				double sideRatio = (minBox.box.right.X - minBox.box.left.X) / (minBox.box.bottom.Y - minBox.box.top.Y);
				if (innerVolume > 100 && innerVolume / minBox.area > 0.9 && sideRatio > 0.85 && sideRatio < 1.18) {
					dice.Add (new Dice { minBox = minBox, innerPoints = innerPoints});
					//MessageBox.Show ("Found a square!");
				}

				startx = startingPoint.Value.X;
				starty = startingPoint.Value.Y;
			}
			progressBar.PerformStep ();

			foreach (Point p in discardedSet) {
				Image [p.X, p.Y] = Color.Black;
			}

			// For each square
			foreach (Dice d in dice) {
				// Hough transform over fitted square
				PointF p1 = BoundingBox.rotatePoint (new PointF (d.minBox.box.left.X, d.minBox.box.top.Y), -d.minBox.rotation);
				PointF p2 = BoundingBox.rotatePoint (new PointF (d.minBox.box.right.X, d.minBox.box.top.Y), -d.minBox.rotation);
				PointF p3 = BoundingBox.rotatePoint (new PointF (d.minBox.box.left.X, d.minBox.box.bottom.Y), -d.minBox.rotation);
				PointF p4 = BoundingBox.rotatePoint (new PointF (d.minBox.box.right.X, d.minBox.box.bottom.Y), -d.minBox.rotation);
				int minY = (int)Math.Min (Math.Min (p1.Y, p2.Y), Math.Min (p3.Y, p4.Y));
				int maxY = (int)Math.Max (Math.Max (p1.Y, p2.Y), Math.Max (p3.Y, p4.Y));
				int minX = (int)Math.Min (Math.Min (p1.X, p2.X), Math.Min (p3.X, p4.X));
				int maxX = (int)Math.Max (Math.Max (p1.X, p2.X), Math.Max (p3.X, p4.X));

				int[,,] hough = Hough.houghTransformCircles (imgArr, new Point (minX, minY), new Point (maxX, maxY), (int)((maxX-minX)/10), maxX-minX, 128, 128);//(int)((maxX - minX) / 10), maxX - minX, 128, 128);
				int maxVal = 0;
				foreach (int c in hough) {
					if (c > maxVal) {
						maxVal = c;
					}
				}
				List<Circle> circles = Hough.FindCircles (hough, 128, (int)((maxX - minX) / 10));
				circles = Hough.DiscardOverlapping (circles);

				// count the circles
				/*if (circles.Count == 0 ||
					circles.Count > 6) {
					continue; // Not a standard dice
				}*/

				for (int y = Math.Max(0, minY); y <= Math.Min(Image.GetLength(1)-1, maxY); y++) {
					for (int x = Math.Max(0, minX); x <= Math.Min(Image.GetLength(0)-1, maxX); x++) {
						PointF rotatedPoint = BoundingBox.rotatePoint (new PointF (x, y), d.minBox.rotation);
						if (rotatedPoint.X >= d.minBox.box.left.X &&
							rotatedPoint.X <= d.minBox.box.right.X &&
							rotatedPoint.Y >= d.minBox.box.top.Y &&
							rotatedPoint.Y <= d.minBox.box.bottom.Y) {
							Image [x, y] = Color.White;
						}
					}
				}

				Color col = GetNextColor ();
				foreach (Point p in d.innerPoints) {
					Image [p.X, p.Y] = col;
				}
				foreach(Circle c in circles) {
					int r = c.R;
					int a = c.X + minX;
					int b = c.Y + minY;
					Color color = GetNextColor ();

					for (int i = a - r; i < a + r; i++) {
						for (int j = b - r; j < b + r; j++) {
							{
								int k = i - a;
								int l = j - b;

								if (k * k + l * l <= r * r) {
									Image [i, j] = color;
								}
							}
						}
					}
				}
			}
			progressBar.PerformStep ();

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
			progressBar.PerformStep ();
			//progressBar.Visible = false;                                    // Hide progress bar
        }

		private double[,] toGrayArray(Color[,] image) {
			double[,] grayArray = new double[image.GetLength(0), image.GetLength(1)];
			for (int x = 0; x < image.GetLength(0); x++)
			{
				for (int y = 0; y < image.GetLength(1); y++)
				{
					grayArray [x, y] = (double)(image [x, y].R * 0.3 + image [x, y].G * 0.59 + image [x, y].B * 0.11)/256;
				}
			}
			return grayArray;
		}

		private void imgFromArr(double[,] values, Color[,] target) {
			for (int x = 0; x < target.GetLength(0); x++)
			{
				for (int y = 0; y < target.GetLength(1); y++)
				{
					int val = (int)(Math.Min (1, Math.Max(0, values [x, y]))*255);
					target [x, y] = Color.FromArgb(val, val, val);
				}
			}
		}
    }
}
