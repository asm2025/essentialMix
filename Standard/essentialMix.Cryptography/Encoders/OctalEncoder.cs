using System.Text;
using JetBrains.Annotations;
using essentialMix.Numeric;

namespace essentialMix.Cryptography.Encoders;

public sealed class OctalEncoder : NumericEncoder
{
	public OctalEncoder()
		: base(BitVectorMode.Octal)
	{
	}

	public OctalEncoder([NotNull] Encoding encoding) 
		: base(BitVectorMode.Octal, encoding)
	{
	}

	public override bool CanChange => false;

	public override object Clone() { return new OctalEncoder(Encoding); }
}