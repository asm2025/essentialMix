using System.Drawing;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class COLORREFExtension
{
	public static Color ToColor(this COLORREF thisValue)
	{
		return Color.FromArgb((int)(0x000000FFU & thisValue.Value), (int)(0x0000FF00U & thisValue.Value) >> 8, (int)(0x00FF0000U & thisValue.Value) >> 16);
	}
}