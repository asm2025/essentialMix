using System;
using System.Text;
using essentialMix.Numeric;

namespace essentialMix.Cryptography.Encoders;

public class NumericEncoder(BitVectorMode mode, Encoding encoding)
	: Encoder<ByteEncoder>(ByteEncoder.Create(mode, encoding)), INumericEncoder
{
	private BitVectorMode _mode = mode;

	public NumericEncoder(BitVectorMode mode)
		: this(mode, null)
	{
	}

	public override object Clone() { return new NumericEncoder(Mode, Encoding); }

	public BitVectorMode Mode
	{
		get => _mode;
		set
		{
			if (!CanChange) throw new NotSupportedException("Cannot change mode.");
			_mode = value;
		}
	}

	public virtual bool CanChange { get; } = true;
}