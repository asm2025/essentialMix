using System.Drawing;
using essentialMix.Windows;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class Win32RectExtension
{
	public static Rectangle ToRectangle(this RECT thisValue)
	{
		return new Rectangle(thisValue.X, thisValue.Y, thisValue.Width, thisValue.Height);
	}

	public static RECT ToWin32Rect(this Rectangle thisValue)
	{
		return new RECT(thisValue.X, thisValue.Y, thisValue.Width, thisValue.Height);
	}
}