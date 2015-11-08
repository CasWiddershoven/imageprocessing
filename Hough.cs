using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;

namespace INFOIBV
{
	
	public struct Circle {
		public int R;
		public int A;
		public int B;

		public Circle(int R, int A, int B) {
			this.R = R;
			this.A = A;
			this.B = B;
		}

		public bool OverlapsWith(Circle c) {
			var r0 = R;
			var x0 = A;
			var y0 = B;
			var r1 = c.R;
			var x1 = c.A;
			var y1 = c.B;

			var rdiffsq = (r0 - r1) * (r0 - r1);
			var rsumsq = (r0 + r1) * (r0 + r1);

			var xdiffsq = (x0 - x1) * (x0 - x1);
			var ydiffsq = (y0 - y1) * (y0 - y1);


			return (xdiffsq + ydiffsq <= rsumsq);
			//return (rdiffsq <= xdiffsq + ydiffsq && xdiffsq + ydiffsq <= rsumsq) || Math.Sqrt(
				
		}
	}
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


			// Because every radius has an independent index, we can safely parralelize calculating solutions
			Parallel.For (minRadius, maxRadius, (radius) => {
				for (int x = smallest.X; x < largest.X; x++) {
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
			});
				


			return accum;
		}
			

		/// <summary>
		/// Discard overlapping circles.
		/// </summary>
		/// <param name="circles">Circles.</param>
		public static List<Circle> Discard(List<Circle> circles) {

			// TODO this can be more efficient. But meh don't care because hough transform is still the slowest factor
			bool[] discard = new bool[circles.Count];

			for (int i = 0; i < circles.Count; i++) {
				if (!discard [i]) {
					for (int j = 0; j < circles.Count; j++) {
						if (j != i) {
							if (!discard [j]) {
								discard [j] = circles [i].OverlapsWith (circles [j]);
							}
						}
					}
				}
			}

			List<Circle> res = new List<Circle> ();

			for (int i = 0; i < discard.Length; i++) {
				if (!discard [i]) {
					res.Add (circles [i]);
				}
			}


			return res;
		}

		// Given a hough transform, find the most prominent circles using a simple threshold.
		public static List<Circle> FindCircles(int[,,] hough,  int threshold, int minRadius)
		{
			var circles = new List<Circle> ();

			for (int r = 0; r < hough.GetLength (0); r++) {
				for (int a = 0; a < hough.GetLength (1); a++) {
					for (int b = 0; b < hough.GetLength (2); b++) {
						if (hough [r, a, b] >= threshold) {
							circles.Add (new Circle (r+minRadius,a,b));
						}
					}
				}
			}

			return circles;
		}
	}
}

