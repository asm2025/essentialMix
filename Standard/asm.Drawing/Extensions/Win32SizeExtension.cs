using System.Drawing;

namespace asm.Drawing.Extensions
{
	public static class Win32SizeExtension
	{
		public static Size ToSize(this Win32.SIZE thisValue)
		{
			return new Size(thisValue.CX, thisValue.CY);
		}
	}
}