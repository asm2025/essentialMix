using System.Drawing;

namespace asm.Drawing.Helpers
{
	public static class ColorHelper
	{
		public static Color ToColor(uint value)
		{
			return Color.FromArgb((byte)(value >> 24)
								, (byte)(value >> 16)
								, (byte)(value >> 8)
								, (byte)value);
		}
	}
}