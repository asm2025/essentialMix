using System.Drawing;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class Win32SizeExtension
	{
		public static Size ToSize(this Win32.SIZE thisValue)
		{
			return new Size(thisValue.CX, thisValue.CY);
		}
	}
}