using System.Drawing;

namespace asm.Drawing.Extensions
{
	public static class PointExtension
	{
		public static float DistanceTo(this Point thisValue, Point other)
		{
			int dx = thisValue.X - other.X;
			int dy = thisValue.Y - other.Y;
			return (float) System.Math.Sqrt( dx * dx + dy * dy );
		}

		public static Win32.POINT ToWin32Point(this Point thisValue)
		{
			return new Win32.POINT(thisValue.X, thisValue.Y);
		}
	}
}