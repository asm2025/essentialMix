using System.Collections;
using JetBrains.Annotations;
using essentialMix.Numeric;

namespace essentialMix.Extensions
{
	public static class BitArrayExtension
	{
		public static BitVector AsBitVector([NotNull] this BitArray thisValue) { return (BitVector)thisValue; }
	}
}