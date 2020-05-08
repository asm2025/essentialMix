using System.Collections.Specialized;
using asm.Numeric;

namespace asm.Extensions
{
	public static class BitVector32Extension
	{
		public static BitVector AsBitVector(this BitVector32 thisValue) { return (BitVector)thisValue; }
	}
}