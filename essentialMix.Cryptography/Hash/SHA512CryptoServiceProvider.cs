using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash;

public class SHA512CryptoServiceProvider : SHA512<System.Security.Cryptography.SHA512CryptoServiceProvider>
{
	public SHA512CryptoServiceProvider()
		: base(new System.Security.Cryptography.SHA512CryptoServiceProvider())
	{
	}

	public SHA512CryptoServiceProvider([NotNull] System.Security.Cryptography.SHA512CryptoServiceProvider algorithm)
		: base(algorithm)
	{
	}

	public SHA512CryptoServiceProvider([NotNull] Encoding encoding) 
		: base(new System.Security.Cryptography.SHA512CryptoServiceProvider(), encoding)
	{
	}

	public SHA512CryptoServiceProvider([NotNull] System.Security.Cryptography.SHA512CryptoServiceProvider algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new SHA512CryptoServiceProvider(Algorithm, Encoding); }
}