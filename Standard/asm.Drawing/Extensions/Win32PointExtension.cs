using System.Drawing;

namespace asm.Drawing.Extensions
{
	public static class Win32PointExtension
	{
		public static Point ToPoint(this Win32.POINT thisValue)
		{
			return new Point(thisValue.X, thisValue.Y);
		}
	}
}