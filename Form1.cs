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
			double[,] gaussKernel = ImageOperations.genGaussianKernel (3, 9, 9);
			double res = 0;
			for (int x = 0; x < 5; x++) {
				for (int y = 0; y < 1; y++) {
					res += gaussKernel [x, y];
				}
			}
			ImageOperations.applyKernel (imgArr, gaussKernel);
			ImageOperations.findEdges (imgArr);
			ImageOperations.treshold (imgArr,0.1);

			var minR = 10;
			var maxR = 80;

			var hough =  Hough.houghTransformCircles (imgArr, new Point (0, 0), new Point (imgArr.GetLength(0), imgArr.GetLength(1)), minR, maxR, 128,128);
			imgFromArr (imgArr, Image);

			/*var circles = FindCircles (hough, 400, minR);
		

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
								Image [i, j] = Color.Red;
							}
						}
					}
				}
				count++;
			}*/




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
			ImageOperations.treshold (imgArr, 0.1);
			//dilate (imgArr, 2);
			//imgFromIntArr (imgArr, Image);
			//findEdges (imgArr);
			//treshold (imgArr, 40);
			//complement (Image);
		

			//var img = toGrayArray (Image);

			var pixel = EdgeDetection.findStartingPixel (imgArr);
		
			var dirs = EdgeDetection.MarchSquares (imgArr, pixel.X, pixel.Y);

			int rx = pixel.X;
			int ry = pixel.Y;
			Point point = new Point (rx, ry);
		
		
			
			imgFromArr (imgArr, Image);
	
			HashSet<Point> edge = new HashSet<Point> ();
			Dictionary<int, Point> inner = new Dictionary<int, Point> (); // Yeah, point.X is now the min x on that y coordinate and point.Y the max x.
			BoundingBox bbox = new BoundingBox (Image.GetLength (0), Image.GetLength (1));
			foreach (var dir in dirs) {
				Image [rx, ry] = Color.Red;
				point = new Point (rx, ry);
				edge.Add (point);
				bbox.addPoint (point);
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
			BoundingBox.MinBox minBox = bbox.getMinBoundingBox ();
			for (float y = minBox.box.top.Y; y <= minBox.box.bottom.Y; y++) {
				for (float x = minBox.box.left.X; x <= minBox.box.right.X; x++) {
					PointF origPix = BoundingBox.rotatePoint (new PointF (x, y), -minBox.rotation);
					origPix.X = Math.Max (0, origPix.X);
					origPix.X = Math.Min (Image.GetLength (0) - 1, origPix.X);
					origPix.Y = Math.Max (0, origPix.Y);
					origPix.Y = Math.Min (Image.GetLength (1) - 1, origPix.Y);
					Image [(int)origPix.X, (int)origPix.Y] = Color.Blue;
				}
			}
			int innerVolume = 0;
			for (int y = (int)bbox.boxes[0].top.Y; y < bbox.boxes[0].bottom.Y; y++) {
				innerVolume += inner [y].Y - inner [y].X + 1;
				for (int x = (int)inner[y].X; x < inner[y].Y; x++) {
					Image [x, y] = Color.Green;
				}
			}

			if (innerVolume > Math.Pow(edge.Count/4, 2)*0.9 &&
			    innerVolume < Math.Pow(edge.Count/4, 2)*1.1) {
				MessageBox.Show ("We found a square!");
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
