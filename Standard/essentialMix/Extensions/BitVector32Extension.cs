using System.Collections.Specialized;
using essentialMix.Numeric;

namespace essentialMix.Extensions
{
	public static class BitVector32Extension
	{
		public static BitVector AsBitVector(this BitVector32 thisValue) { return new BitVector(thisValue); }
	}
}