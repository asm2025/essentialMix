using System.Drawing;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class Win32RectExtension
	{
		public static Rectangle ToRectangle(this Win32.RECT thisValue)
		{
			return new Rectangle(thisValue.X, thisValue.Y, thisValue.Width, thisValue.Height);
		}
	}
}