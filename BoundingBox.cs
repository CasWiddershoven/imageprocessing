using System;
using System.Drawing;

namespace INFOIBV
{
	public class BoundingBox
	{
		// A struct defining s square by its utmost left, top, bottom and right corners
		public struct Square {
			public PointF left, top, bottom, right;
		}

		// A struct containing a minimum bounding box and the rotation of that box
		public struct MinBox {
			public Square box;
			public int rotation;
		}

		public Square[] boxes = new Square[90];

		public BoundingBox (int maxWidth, int maxHeight)
		{
			for (int r = 0; r < 90; r++) {
				boxes[r] = new Square();
				boxes [r].left = new PointF (maxWidth, maxHeight);
				boxes [r].top = new PointF (maxWidth, maxHeight);
				boxes [r].right = new PointF (0, 0);
				boxes [r].bottom = new PointF (0, 0);
			}
		}

		/// <summary>
		/// Rotates the point r degrees.
		/// </summary>
		/// <returns>The rotated point.</returns>
		/// <param name="point">The point to be rotated.</param>
		/// <param name="r">The rotation, in degrees.</param>
		public static PointF rotatePoint(PointF point, float r) {
			PointF res = new PointF ();
			res.X = (float)(point.X * Math.Cos (r) - point.Y * Math.Sin (r));
			res.Y = (float)(point.X * Math.Sin (r) + point.Y * Math.Cos (r));
			return res;
		}

		/// <summary>
		/// Adds a point.
		/// </summary>
		/// <param name="point">The point.</param>
		public void addPoint(Point point) {
			for (int r = 0; r < 90; r++) {
				PointF rotPoint = rotatePoint (point, r);
				float x = rotPoint.X;
				float y = rotPoint.Y;
				float maxX = boxes[r].right.X;
				float minX = boxes[r].left.X;
				float maxY = boxes[r].bottom.Y;
				float minY = boxes[r].top.Y;
				
				if (x >= maxX) {
					if (x > maxX || y < boxes[r].right.Y) {
						boxes[r].right = rotPoint;
					}
				}
				if (y >= maxY) {
					if (y > maxY || x < boxes[r].bottom.X) {
						boxes[r].bottom = rotPoint;
					}
				}
				if (x <= minX) {
					if (x < minX || y < boxes[r].left.Y) {
						boxes[r].left = rotPoint;
					}
				}
				if (y <= minY) {
					if (y < minY || x < boxes[r].top.X) {
						boxes[r].top = rotPoint;
					}
				}
			}
		}

		/// <summary>
		/// Gets the minimum bounding box.
		/// </summary>
		/// <returns>The minimum bounding box.</returns>
		public MinBox getMinBoundingBox() {
			float minArea = (boxes [0].bottom.Y - boxes [0].top.Y) * (boxes [0].right.X - boxes [0].left.X);
			int minRotation = 0;
			for (int r = 0; r < 90; r++) {
				float area = (boxes [r].bottom.Y - boxes [r].top.Y) * (boxes [r].right.X - boxes [r].left.X);
				if (area < minArea) {
					minArea = area;
					minRotation = r;
				}
			}
			return new MinBox { rotation = minRotation, box = boxes[minRotation] };
		}
	}
}

