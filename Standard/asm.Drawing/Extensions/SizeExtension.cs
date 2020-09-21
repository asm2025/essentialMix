using System.Drawing;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class SizeExtension
	{
		public static Win32.SIZE ToWin32Size(this Size thisValue)
		{
			return new Win32.SIZE(thisValue.Width, thisValue.Height);
		}
	}
}