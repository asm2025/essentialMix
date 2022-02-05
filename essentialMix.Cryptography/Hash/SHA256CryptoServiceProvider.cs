using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash;

public class SHA256CryptoServiceProvider : SHA256<System.Security.Cryptography.SHA256CryptoServiceProvider>
{
	public SHA256CryptoServiceProvider()
		: base(new System.Security.Cryptography.SHA256CryptoServiceProvider())
	{
	}

	public SHA256CryptoServiceProvider([NotNull] System.Security.Cryptography.SHA256CryptoServiceProvider algorithm)
		: base(algorithm)
	{
	}

	public SHA256CryptoServiceProvider([NotNull] Encoding encoding) 
		: base(new System.Security.Cryptography.SHA256CryptoServiceProvider(), encoding)
	{
	}

	public SHA256CryptoServiceProvider([NotNull] System.Security.Cryptography.SHA256CryptoServiceProvider algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new SHA256CryptoServiceProvider(Algorithm, Encoding); }
}