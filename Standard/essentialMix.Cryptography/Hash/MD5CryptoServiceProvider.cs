using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash;

public class MD5CryptoServiceProvider : MD5<System.Security.Cryptography.MD5CryptoServiceProvider>
{
	public MD5CryptoServiceProvider()
		: base(new System.Security.Cryptography.MD5CryptoServiceProvider())
	{
	}

	public MD5CryptoServiceProvider([NotNull] System.Security.Cryptography.MD5CryptoServiceProvider algorithm)
		: base(algorithm)
	{
	}

	public MD5CryptoServiceProvider([NotNull] Encoding encoding) 
		: base(new System.Security.Cryptography.MD5CryptoServiceProvider(), encoding)
	{
	}

	public MD5CryptoServiceProvider([NotNull] System.Security.Cryptography.MD5CryptoServiceProvider algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new MD5CryptoServiceProvider(Algorithm, Encoding); }
}