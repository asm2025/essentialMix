using System.Text;
using JetBrains.Annotations;
using asm.Numeric;

namespace asm.Cryptography.Encoders
{
	public sealed class DecimalEncoder : NumericEncoder
	{
		public DecimalEncoder()
			: base(BitVectorMode.Decimal)
		{
		}

		public DecimalEncoder([NotNull] Encoding encoding) 
			: base(BitVectorMode.Decimal, encoding)
		{
		}

		public override bool CanChange => false;

		public override object Clone() { return new DecimalEncoder(Encoding); }
	}
}