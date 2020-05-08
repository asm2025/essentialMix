using System.Drawing;

namespace asm.Drawing.Extensions
{
	public static class COLORREFExtension
	{
		public static Color ToColor(this Win32.COLORREF thisValue)
		{
			return Color.FromArgb((int)(0x000000FFU & thisValue.Value), (int)(0x0000FF00U & thisValue.Value) >> 8, (int)(0x00FF0000U & thisValue.Value) >> 16);
		}
	}
}