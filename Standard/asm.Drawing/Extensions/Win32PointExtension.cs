using System.Drawing;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class Win32PointExtension
	{
		public static Point ToPoint(this POINT thisValue)
		{
			return new Point(thisValue.X, thisValue.Y);
		}
	}
}