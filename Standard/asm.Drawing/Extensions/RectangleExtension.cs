using System.Drawing;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class RectangleExtension
	{
		public static Win32.RECT ToWin32Rect(this Rectangle thisValue)
		{
			return new Win32.RECT(thisValue.X, thisValue.Y, thisValue.Width, thisValue.Height);
		}
	}
}