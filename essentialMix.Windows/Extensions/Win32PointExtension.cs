using System.Drawing;
using essentialMix.Windows;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class Win32PointExtension
{
	public static Point ToPoint(this POINT thisValue)
	{
		return new Point(thisValue.X, thisValue.Y);
	}

	public static POINT ToWin32Point(this Point thisValue)
	{
		return new POINT(thisValue.X, thisValue.Y);
	}
}