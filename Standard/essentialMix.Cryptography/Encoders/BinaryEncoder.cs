using System.Text;
using JetBrains.Annotations;
using essentialMix.Numeric;

namespace essentialMix.Cryptography.Encoders;

public sealed class BinaryEncoder : NumericEncoder
{
	public BinaryEncoder() 
		: base(BitVectorMode.Binary)
	{
	}

	public BinaryEncoder([NotNull] Encoding encoding) 
		: base(BitVectorMode.Binary, encoding)
	{
	}

	public override bool CanChange => false;

	public override object Clone() { return new BinaryEncoder(Encoding); }
}