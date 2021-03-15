using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Symmetric
{
	public class AESCng : AES<AesCng>
	{
		public AESCng()
			: base(new AesCng())
		{
		}

		public AESCng([NotNull] AesCng algorithm) 
			: base(algorithm)
		{
		}

		public AESCng([NotNull] Encoding encoding) 
			: base(new AesCng(), encoding)
		{
		}

		public AESCng([NotNull] AesCng algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new AESCng(Algorithm, Encoding); }
	}
}
