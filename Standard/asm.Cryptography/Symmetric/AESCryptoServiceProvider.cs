using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Symmetric
{
	public class AESCryptoServiceProvider : AES<AesCryptoServiceProvider>
	{
		public AESCryptoServiceProvider()
			: base(new AesCryptoServiceProvider())
		{
		}

		public AESCryptoServiceProvider([NotNull] AesCryptoServiceProvider algorithm) 
			: base(algorithm)
		{
		}

		public AESCryptoServiceProvider([NotNull] Encoding encoding) 
			: base(new AesCryptoServiceProvider(), encoding)
		{
		}

		public AESCryptoServiceProvider([NotNull] AesCryptoServiceProvider algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new AESCryptoServiceProvider(Algorithm, Encoding); }
	}
}