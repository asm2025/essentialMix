using System.Text;
using JetBrains.Annotations;
using essentialMix.Numeric;

namespace essentialMix.Cryptography.Encoders;

public sealed class HexadecimalEncoder : NumericEncoder
{
	public HexadecimalEncoder()
		: base(BitVectorMode.Hexadecimal)
	{
	}

	public HexadecimalEncoder([NotNull] Encoding encoding) 
		: base(BitVectorMode.Hexadecimal, encoding)
	{
	}

	public override bool CanChange => false;

	public override object Clone() { return new HexadecimalEncoder(Encoding); }
}