using System.Drawing;
using essentialMix.Windows;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class Win32SizeExtension
{
	public static Size ToSize(this SIZE thisValue)
	{
		return new Size(thisValue.CX, thisValue.CY);
	}

	public static SIZE ToWin32Size(this Size thisValue)
	{
		return new SIZE(thisValue.Width, thisValue.Height);
	}
}