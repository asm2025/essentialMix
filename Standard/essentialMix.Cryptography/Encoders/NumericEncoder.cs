using System;
using System.Text;
using essentialMix.Numeric;

namespace essentialMix.Cryptography.Encoders
{
	public class NumericEncoder : Encoder<ByteEncoder>, INumericEncoder
	{
		private BitVectorMode _mode;

		public NumericEncoder(BitVectorMode mode)
			: this(mode, null)
		{
		}

		public NumericEncoder(BitVectorMode mode, Encoding encoding) 
			: base(ByteEncoder.Create(mode, encoding))
		{
			_mode = mode;
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
}