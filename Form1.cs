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
		
		public struct Square {
			public PointF left, top, bottom, right;
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

		private PointF rotatePoint(PointF point, int degrees) {
			PointF res = new PointF ();
			res.X = (float)(point.X * Math.Cos (degrees) - point.Y * Math.Sin (degrees));
			res.Y = (float)(point.X * Math.Sin (degrees) + point.Y * Math.Cos (degrees));
			return res;
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


			double[,] imgArr = toGrayArray (Image);
			double[,] gaussKernel = genGaussianKernel (3, 9, 9);
			/*double[,] gaussKernel = new double[,] {
				{0.00000067,0.00002292,0.00019117,0.00038771,0.00019117,0.00002292,0.00000067},
				{0.00002292,0.00078634,0.00655965,0.01330373,0.00655965,0.00078633,0.00002292},
				{0.00019117,0.00655965,0.05472157,0.11098164,0.05472157,0.00655965,0.00019117},
				{0.00038771,0.01330373,0.11098164,0.22508352,0.11098164,0.01330373,0.00038771},
				{0.00019117,0.00655965,0.05472157,0.11098164,0.05472157,0.00655965,0.00019117},
				{0.00002292,0.00078633,0.00655965,0.01330373,0.00655965,0.00078633,0.00002292},
				{0.00000067,0.00002292,0.00019117,0.00038771,0.00019117,0.00002292,0.00000067},
			};*/
			double res = 0;
			for (int x = 0; x < 5; x++) {
				for (int y = 0; y < 1; y++) {
					res += gaussKernel [x, y];
				}
			}
			applyKernel (imgArr, gaussKernel);
			findEdges (imgArr);
			treshold (imgArr,0.1);
			//var point = findStartingPixel (imgArr, 0);

			var minR = 10;
			var maxR = 80;

			var hough =  houghTransformCircles (imgArr, new Point (0, 0), new Point (imgArr.GetLength(0), imgArr.GetLength(1)), minR, maxR, 128, 128);


			var circles = FindCircles (hough, 100, minR);
		

			imgFromIntArr (imgArr, Image);
	
			int count = 0;
			foreach (var circle in circles) {
				var r = circle.Item1 + minR;
				var a = circle.Item2;
				var b = circle.Item3;

				for (int i = a - r; i < a + r; i++) {
					for (int j = b - r; j < b + r; j++) {
						{
							int k = i - a;
							int l = j - b;

							if (k * k + l * l <= r * r) {
								Image [i, j] = Color.FromArgb(new Random().Next
							}
						}
					}
				}
				count++;
			}




			/*var directions = marchSquares (imgArr, point.X, point.Y);


			int rx = point.X;
			int ry = point.Y;

			foreach (var dir in directions) {
				Image [rx, ry] = Color.Red;
				rx += dir.GetDX ();
				ry += dir.GetDY ();
			}*/
			//findEdges (imgArr);
			//treshold (imgArr, 0.1);
			/*	treshold (imgArr, 0.1);
			//dilate (imgArr, 2);
			//imgFromIntArr (imgArr, Image);
			//findEdges (imgArr);
			//treshold (imgArr, 40);
			//complement (Image);
		

			//var img = toGrayArray (Image);

			var pixel = findStartingPixel (imgArr);
		
			var dirs = MarchSquares (imgArr, pixel.X, pixel.Y);

			int rx = pixel.X;
			int ry = pixel.Y;
			Point point = new Point (rx, ry);
		
		
			
			imgFromIntArr (imgArr, Image);
	
			HashSet<Point> edge = new HashSet<Point> ();
			Dictionary<int, Point> inner = new Dictionary<int, Point> (); // Yeah, point.X is now the min x on that y coordinate and point.Y the max x.
			Square[] boundaries = new Square[90];
			for (int r = 0; r < 90; r++) {
				boundaries[r] = new Square();
				boundaries [r].left = new PointF (imgArr.GetLength(0), imgArr.GetLength(1));
				boundaries [r].top = new PointF (imgArr.GetLength(0), imgArr.GetLength(1));
				boundaries [r].right = new PointF (0, 0);
				boundaries [r].bottom = new PointF (0, 0);
			}
			foreach (var dir in dirs) {
				Image [rx, ry] = Color.Red;
				point = new Point (rx, ry);
				edge.Add (point);
				for (int r = 0; r < 90; r++) {
					PointF rotPoint = rotatePoint (new PointF (rx, ry), r);
					float rotX = rotPoint.X;
					float rotY = rotPoint.Y;
					float maxX = boundaries [r].right.X;
					float minX = boundaries [r].left.X;
					float maxY = boundaries [r].bottom.Y;
					float minY = boundaries [r].top.Y;
					if (rotX >= maxX) {
						if (rotX > maxX || rotY < boundaries[r].right.Y) {
							boundaries[r].right = rotPoint;
						}
					}
					if (rotY >= maxY) {
						if (rotY > maxY || rotX < boundaries[r].bottom.X) {
							boundaries[r].bottom = rotPoint;
						}
					}
					if (rotX <= minX) {
						if (rotX < minX || rotY < boundaries[r].left.Y) {
							boundaries[r].left = rotPoint;
						}
					}
					if (rotY <= minY) {
						if (rotY < minY || rotX < boundaries[r].top.X) {
							boundaries[r].top = rotPoint;
						}
					}
				}
				if (inner.ContainsKey (ry)) {
					int newX = Math.Min (rx, inner [ry].X);
					int newY = Math.Max (rx, inner [ry].Y);
					inner [ry] = new Point (newX, newY);
				} else {
					inner.Add (ry, new Point (rx, rx));
				}
				rx += dir.GetDX ();
				ry += dir.GetDY ();
			}
			float minArea = (boundaries [0].bottom.Y - boundaries [0].top.Y) * (boundaries [0].right.X - boundaries [0].left.X);
			int minRotation = 0;
			for (int r = 0; r < 90; r++) {
				float area = (boundaries [r].bottom.Y - boundaries [r].top.Y) * (boundaries [r].right.X - boundaries [r].left.X);
				if (area < minArea) {
					minArea = area;
					minRotation = r;
				}
			}
			for (float y = boundaries[minRotation].top.Y; y <= boundaries[minRotation].bottom.Y; y++) {
				for (float x = boundaries[minRotation].left.X; x <= boundaries[minRotation].right.X; x++) {
					PointF origPix = rotatePoint (new PointF (x, y), -minRotation);
					origPix.X = Math.Max (0, origPix.X);
					origPix.X = Math.Min (Image.GetLength (0) - 1, origPix.X);
					origPix.Y = Math.Max (0, origPix.Y);
					origPix.Y = Math.Min (Image.GetLength (1) - 1, origPix.Y);
					Image [(int)origPix.X, (int)origPix.Y] = Color.Blue;
				}
			}
			int innerVolume = 0;
			for (int y = (int)boundaries[0].top.Y; y < boundaries[0].bottom.Y; y++) {
				innerVolume += inner [y].Y - inner [y].X + 1;
				for (int x = (int)inner[y].X; x < inner[y].Y; x++) {
					Image [x, y] = Color.Green;
				}
			}

			if (innerVolume > Math.Pow(edge.Count/4, 2)*0.9 &&
			    innerVolume < Math.Pow(edge.Count/4, 2)*1.1) {
				MessageBox.Show ("We found a square!");
			}*/

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
        

		private void applyKernel(double[,] image, double[,] kernel) {
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

		private double[,] genGaussianKernel(double sigma, int width, int height) {
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

		private void imgFromIntArr(double[,] values, Color[,] target) {
			for (int x = 0; x < target.GetLength(0); x++)
			{
				for (int y = 0; y < target.GetLength(1); y++)
				{
					int val = (int)(Math.Min (1, Math.Max(0, values [x, y]))*255);
					target [x, y] = Color.FromArgb(val, val, val);
				}
			}
		}
		private void treshold(double[,] image, double treshold=0.5) 
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


		private int[,] houghTransformLines(double[,] image, Point smallest, Point largest, int maxTheta, int maxR)
		{
			int[,] accum = new int[largest.X - smallest.X, largest.Y - smallest.Y];
			var rMax = Math.Sqrt ((largest.X * largest.X) + (largest.Y * largest.Y));
			var dr = rMax / (double)(maxR / 2.0);
			var dtheta = Math.PI / (double)(maxTheta);
			for (int x = smallest.X; x < largest.X; x++) {
				for (int y = smallest.Y; y < largest.Y; y++) {
					if (image [x, y] != 0) {
						for (int t = 0; t < maxTheta; t++) {
							var theta = dtheta * (double)t;
							var r = x * Math.Cos (theta) + y * Math.Sin (theta);
							// centered
							var ir = maxR / 2 + (int)(r/dr +0.5);
							accum [t, ir] += 1;

						}
					}
				}
			}
			return null;
		}
			
		private int[,,] houghTransformCircles(double[,] image, Point smallest, Point largest, int minRadius, int maxRadius, int maxTheta, int maxR) 
		{

			int[,,] accum =
				new int[maxRadius - minRadius, largest.X -smallest.X, largest.Y - smallest.Y];

			//double?[] cos = new double[maxTheta];
		//	double?[] sin = new double[maxTheta];
			
			for(int radius = minRadius; radius < maxRadius; radius++) {
				for(int x = smallest.X; x < largest.X; x++) {
					for (int y = smallest.Y; y < largest.Y; y++) {
						if (image [x, y] != 0) {
							int indexR = radius - minRadius;
							for (var theta = 0; theta < maxTheta; theta++) {
								int a = x + (int)(radius * Math.Cos ((2 * Math.PI * theta) / maxTheta));
								int b = y + (int)(radius * Math.Sin ((2 * Math.PI * theta) / maxTheta));
								if ((b >= smallest.Y) && (b < largest.Y) && (a >= smallest.X) && (a < largest.X)) {
									accum [indexR, a, b] += 1;
								}
							}
						}
					}
				}
			}
			return accum;
		}


		// Given a hough transform, find the most prominent circles using a simple threshold.
		// it's not quick but it works!

		// It returns circles found sorted by first on X and then on Y.
		// this is easy if later you want to do clustering and labelling nicely.
		private List<Tuple<int,int,int>> FindCircles(int[,,] hough,  int threshold, int minRadius)
		{
			var circles = new List<Tuple<int,int,int>> ();
			var result = new List<Tuple<int,int,int>> ();

			for (int r = 0; r < hough.GetLength (0); r++) {
				for (int a = 0; a < hough.GetLength (1); a++) {
					for (int b = 0; b < hough.GetLength (2); b++) {
						if (hough [r, a, b] >= threshold) {
							circles.Add (Tuple.Create (r+minRadius,a,b));
						}
					}
				}
			}

			if (circles.Count == 0)
				return circles;
			// cluster circles on a and b
			circles.Sort ((tup1, tup2) => tup1.Item2 - tup2.Item2);
			circles.Sort ((tup1, tup2) => tup1.Item3 - tup2.Item3);

			int prevR = circles [0].Item1;
			int prevA = circles [0].Item2;
			int prevB = circles [0].Item3;

			result.Add (circles [0]);

			foreach (var circle in circles) {
				int r = circle.Item1;
				int a = circle.Item2;
				int b = circle.Item3;

				// if we're probably a new circle, yield this circle. Ignore other points
				if ((Math.Abs (a - prevA) > 2*prevR) || (Math.Abs (b - prevB) > 2*prevR)) {
					prevA = a;
					prevB = b;
					prevR = r;
					result.Add (Tuple.Create (r, a, b));
				}
			}
			return result;
		}


		private void complement(double[,] image)
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

		private void dilate(double[,] image, int offset = 1, bool reversed = false)
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

		private void erode(double[,] image, int offset = 1)
		// This function erodes the image with a structuring element of size (2*offset+1)x(2*offset+1)
		{
			dilate(image, offset, true);
		}

		private void close(double[,] image, int offset = 1)
		// This function does a closing on the image with a structuring element of size (2*offset+1)x(2*offset+1)
		{
			dilate(image, offset);
			erode(image, offset);
		}

		private void open(double[,] image, int offset = 1)
		// This function does an opening on the image with a structuring element of size (2*offset+1)x(2*offset+1)
		{
			erode(image, offset);
			dilate(image, offset);
		}

		private void findEdges(double[,] image)
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
		// finds the circumfrence of an object starting at vx,vy
		private IList<Dir> MarchSquares(double[,] image, int vx, int vy) {
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



		private int getMarchingSquare(double[,] image, int x, int y)
		{
			// TODO what if x and y are 0

			if (x == 0 || y == 0 || x == image.GetLength (0) || y == image.GetLength (1))
				return 0;
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
		private Point findStartingPixel(double[,] image, int x = 0)
		{
			for (int i = x; i < image.GetLength (0); i++) {
				for (int j = 0; j < image.GetLength (1); j++) {
					if (image [i, j] == 1) {
						return new Point (i, j);
					}
				}
			}
			throw new Exception ("Lolwat");

		}
    }
}
