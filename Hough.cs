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
		public int X;
		public int Y;

		/// <summary>
		/// Initializes a new instance of the <see cref="INFOIBV.Circle"/> struct.
		/// </summary>
		/// <param name="R">The radius of the Circle.</param>
		/// <param name="X">The X coordinate of the center of the circle.</param>
		/// <param name="Y">The Y coordinate of the center of the circle.</param>
		public Circle(int R, int X, int Y) {
			this.R = R;
			this.X = X;
			this.Y = Y;
		}

		/// <summary>
		/// Checks if it overlaps with another circle.
		/// </summary>
		/// <returns><c>true</c>, if it overlapses, <c>false</c> otherwise.</returns>
		/// <param name="c">Another circle.</param>
		public bool OverlapsWith(Circle c) {
			var r0 = R;
			var x0 = X;
			var y0 = Y;
			var r1 = c.R;
			var x1 = c.X;
			var y1 = c.Y;

			var rsumsq = (r0 + r1) * (r0 + r1);

			var xdiffsq = (x0 - x1) * (x0 - x1);
			var ydiffsq = (y0 - y1) * (y0 - y1);


			return (xdiffsq + ydiffsq <= rsumsq);
			//return (rdiffsq <= xdiffsq + ydiffsq && xdiffsq + ydiffsq <= rsumsq) || Math.Sqrt(
				
		}
	}

	public static class Hough
	{
		/// <summary>
		/// Performs a Hough transformation for lines
		/// </summary>
		/// <returns>The transform lines.</returns>
		/// <param name="image">The image containing the lines.</param>
		/// <param name="smallest">The left top corner of the rectangle containing the lines.</param>
		/// <param name="largest">The right bottom cornerof the rectangle containing the lines.</param>
		/// <param name="maxTheta">The maximum angle from the origin up to which we look for lines.</param>
		/// <param name="maxR">The maximum distance from the origin up to which we look for lines.</param>
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

		/// <summary>
		/// Performs a Hough transformation for circles
		/// </summary>
		/// <returns>The resulting Hough space.</returns>
		/// <param name="image">The image containing the circles.</param>
		/// <param name="smallest">The left top corner of the rectangle containing the circles.</param>
		/// <param name="largest">The right bottom cornerof the rectangle containing the circles.</param>
		/// <param name="minRadius">Minimum radius of the circles.</param>
		/// <param name="maxRadius">Max radius of the circles.</param>
		/// <param name="maxTheta">The maximum angle from the origin up to which we look for circles.</param>
		/// <param name="maxR">The maximum distance from the origin up to which we look for circles.</param>
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
		public static List<Circle> DiscardOverlapping(List<Circle> circles) {

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

		/// <summary>
		/// Given a hough transform, find the most prominent circles using a simple threshold.
		/// </summary>
		/// <returns>The circles.</returns>
		/// <param name="hough">The Hough space.</param>
		/// <param name="threshold">A threshold.</param>
		/// <param name="minRadius">The minimum radius.</param>
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

