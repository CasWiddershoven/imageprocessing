using System;
using System.Collections.Generic;
using System.Drawing;

namespace INFOIBV
{
	public enum Dir { N, E , S , W, Stay

	}

	public static class Extension {
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

	public class EdgeDetection
	{
		// finds the circumfrence of an object starting at vx,vy
		public static IList<Dir> MarchSquares(double[,] image, int vx, int vy) {
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



		public static int getMarchingSquare(double[,] image, int x, int y)
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
		public static Point findStartingPixel(double[,] image, int x = 0)
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

