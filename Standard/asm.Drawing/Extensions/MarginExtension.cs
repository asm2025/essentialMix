using asm.Drawing;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class MarginExtension
	{
		public static Offset AsOffset(this Margin thisValue) { return (Offset)thisValue; }
	}
}