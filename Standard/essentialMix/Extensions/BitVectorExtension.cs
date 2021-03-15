using System.Collections;
using System.Collections.Specialized;
using essentialMix.Numeric;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class BitVectorExtension
	{
		public static byte AsByte(this BitVector thisValue) { return thisValue.Data; }

		[NotNull]
		public static bool[] AsBoolArray(this BitVector thisValue) { return (bool[])thisValue; }

		[NotNull]
		public static BitArray AsBitArray(this BitVector thisValue) { return (BitArray)thisValue; }

		public static BitVector32 AsBitVector32(this BitVector thisValue) { return (BitVector32)thisValue; }
	}
}