using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Symmetric;

public class TripleDESCryptoServiceProvider : TripleDES<System.Security.Cryptography.TripleDESCryptoServiceProvider>
{
	public TripleDESCryptoServiceProvider()
		: base(new System.Security.Cryptography.TripleDESCryptoServiceProvider())
	{
	}

	public TripleDESCryptoServiceProvider([NotNull] System.Security.Cryptography.TripleDESCryptoServiceProvider algorithm) 
		: base(algorithm)
	{
	}

	public TripleDESCryptoServiceProvider([NotNull] Encoding encoding) 
		: base(new System.Security.Cryptography.TripleDESCryptoServiceProvider(), encoding)
	{
	}

	public TripleDESCryptoServiceProvider([NotNull] System.Security.Cryptography.TripleDESCryptoServiceProvider algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new TripleDESCryptoServiceProvider(Algorithm, Encoding); }
}