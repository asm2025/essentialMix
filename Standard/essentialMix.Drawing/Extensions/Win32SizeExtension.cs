using System.Drawing;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class Win32SizeExtension
	{
		public static Size ToSize(this SIZE thisValue)
		{
			return new Size(thisValue.CX, thisValue.CY);
		}
	}
}