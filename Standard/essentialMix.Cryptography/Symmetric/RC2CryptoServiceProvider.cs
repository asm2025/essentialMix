using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Symmetric
{
	public class RC2CryptoServiceProvider : RC2<System.Security.Cryptography.RC2CryptoServiceProvider>
	{
		public RC2CryptoServiceProvider()
			: base(new System.Security.Cryptography.RC2CryptoServiceProvider())
		{
		}

		public RC2CryptoServiceProvider([NotNull] System.Security.Cryptography.RC2CryptoServiceProvider algorithm) 
			: base(algorithm)
		{
		}

		public RC2CryptoServiceProvider([NotNull] Encoding encoding) 
			: base(new System.Security.Cryptography.RC2CryptoServiceProvider(), encoding)
		{
		}

		public RC2CryptoServiceProvider([NotNull] System.Security.Cryptography.RC2CryptoServiceProvider algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new RC2CryptoServiceProvider(Algorithm, Encoding); }
	}
}