using System.Drawing;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class SizeExtension
{
	public static SIZE ToWin32Size(this Size thisValue)
	{
		return new SIZE(thisValue.Width, thisValue.Height);
	}
}