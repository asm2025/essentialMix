using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Hash
{
	public class SHA1CryptoServiceProvider : SHA1<System.Security.Cryptography.SHA1CryptoServiceProvider>
	{
		public SHA1CryptoServiceProvider()
			: base(new System.Security.Cryptography.SHA1CryptoServiceProvider())
		{
		}

		public SHA1CryptoServiceProvider([NotNull] System.Security.Cryptography.SHA1CryptoServiceProvider algorithm)
			: base(algorithm)
		{
		}

		public SHA1CryptoServiceProvider([NotNull] Encoding encoding) 
			: base(new System.Security.Cryptography.SHA1CryptoServiceProvider(), encoding)
		{
		}

		public SHA1CryptoServiceProvider([NotNull] System.Security.Cryptography.SHA1CryptoServiceProvider algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new SHA1CryptoServiceProvider(Algorithm, Encoding); }
	}
}