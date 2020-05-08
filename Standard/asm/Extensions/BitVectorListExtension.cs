using JetBrains.Annotations;
using asm.Collections;

namespace asm.Extensions
{
	public static class BitVectorListExtension
	{
		[NotNull] public static byte[] AsBytes([NotNull] this BitVectorList thisValue) { return (byte[])thisValue; }

		[NotNull] public static bool[] AsBools([NotNull] this BitVectorList thisValue) { return (bool[])thisValue; }

		[NotNull] public static string AsString([NotNull] this BitVectorList thisValue) { return (string)thisValue; }
	}
}