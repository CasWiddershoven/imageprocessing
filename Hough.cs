using System;
using System.Collections.Generic;
using System.Drawing;

namespace INFOIBV
{
	public static class Hough
	{
		public static int[,] houghTransformLines(double[,] image, Point smallest, Point largest, int maxTheta, int maxR)
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

		public static int[,,] houghTransformCircles(double[,] image, Point smallest, Point largest, int minRadius, int maxRadius, int maxTheta, int maxR) 
		{

			int[,,] accum =
				new int[maxRadius - minRadius, largest.X -smallest.X, largest.Y - smallest.Y];

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
		public static List<Tuple<int,int,int>> FindCircles(int[,,] hough,  int threshold, int minRadius)
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
	}
}

