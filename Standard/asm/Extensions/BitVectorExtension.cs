using System.Collections;
using System.Collections.Specialized;
using asm.Numeric;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class BitVectorExtension
	{
		public static byte AsByte(this BitVector thisValue) { return thisValue; }

		[NotNull]
		public static bool[] AsBools(this BitVector thisValue) { return (bool[])thisValue; }

		[NotNull]
		public static BitArray AsBitArray(this BitVector thisValue) { return (BitArray)thisValue; }

		public static BitVector32 AsBitVector32(this BitVector thisValue) { return (BitVector32)thisValue; }
	}
}