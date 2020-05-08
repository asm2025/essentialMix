using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Asymmetric.Signature
{
	public class DSACryptoServiceProvider : DSAAlgorithmBase<System.Security.Cryptography.DSACryptoServiceProvider>
	{
		public DSACryptoServiceProvider()
			: base(new System.Security.Cryptography.DSACryptoServiceProvider())
		{
		}

		public DSACryptoServiceProvider([NotNull] System.Security.Cryptography.DSACryptoServiceProvider algorithm) 
			: base(algorithm)
		{
		}

		public DSACryptoServiceProvider([NotNull] Encoding encoding) 
			: base(new System.Security.Cryptography.DSACryptoServiceProvider(), encoding)
		{
		}

		public DSACryptoServiceProvider([NotNull] System.Security.Cryptography.DSACryptoServiceProvider algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new DSACryptoServiceProvider(Algorithm, Encoding); }
	}
}