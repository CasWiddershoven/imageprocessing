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

        private void treshold(Color[,] image, double treshold=0.5)
        {
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = image[x, y];                         // Get the pixel color at coordinate (x,y)
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

        private void complement(Color[,] image)
        {
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = image[x, y];                         // Get the pixel color at coordinate (x,y)
                    if (pixelColor == Color.Black)
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

        private void dilate(Color[,] image, int offset = 1, bool reversed = false)
        {
            Color full = Color.White;
            if (reversed)
            {
                full = Color.Black;
            }
            Color[,] orig = (Color[,]) image.Clone();

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    if (orig[x, y] == full)
                    {
                        for (int i = 0; i <= offset; i++)
                        {
                            for (int j = 0; j <= offset; j++)
                            {
                                if (x + i < InputImage.Size.Width && y + j < InputImage.Size.Height)
                                {
                                    image[x + i, y + j] = full;
                                }
                                if (x + i < InputImage.Size.Width && y - j >= 0)
                                {
                                    image[x + i, y - j] = full;
                                }
                                if (x - i >= 0 && y + j < InputImage.Size.Height)
                                {
                                    image[x - i, y + j] = full;
                                }
                                if (x - i >= 0 && y - j >= 0)
                                {
                                    image[x - i, y - j] = full;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void erode(Color[,] image, int offset = 1)
        {
            dilate(image, offset, true);
        }

        private void close(Color[,] image, int offset = 1)
        {
            dilate(image, offset);
            erode(image, offset);
        }

        private void open(Color[,] image, int offset = 1)
        {
            erode(image, offset);
            dilate(image, offset);
        }

        private void findEdges(Color[,] image)
        {
            Color[,] erosion = (Color[,])image.Clone();
            erode(erosion);
            dilate(image);

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
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
            treshold(Image);
            findEdges(Image);
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

    }
}
