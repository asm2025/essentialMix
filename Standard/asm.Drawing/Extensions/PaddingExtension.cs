using asm.Drawing;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class PaddingExtension
	{
		public static Offset AsOffset(this Padding thisValue) { return (Offset)thisValue; }
	}
}