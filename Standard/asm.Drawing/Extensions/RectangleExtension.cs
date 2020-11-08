using System.Drawing;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class RectangleExtension
	{
		public static RECT ToWin32Rect(this Rectangle thisValue)
		{
			return new RECT(thisValue.X, thisValue.Y, thisValue.Width, thisValue.Height);
		}
	}
}