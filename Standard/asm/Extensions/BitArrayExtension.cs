using System.Collections;
using JetBrains.Annotations;
using asm.Numeric;

namespace asm.Extensions
{
	public static class BitArrayExtension
	{
		public static BitVector AsBitVector([NotNull] this BitArray thisValue) { return (BitVector)thisValue; }
	}
}