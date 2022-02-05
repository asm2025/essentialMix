using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Symmetric;

public class DESCryptoServiceProvider : DES<System.Security.Cryptography.DESCryptoServiceProvider>
{
	public DESCryptoServiceProvider()
		: base(new System.Security.Cryptography.DESCryptoServiceProvider())
	{
	}

	public DESCryptoServiceProvider([NotNull] System.Security.Cryptography.DESCryptoServiceProvider algorithm) 
		: base(algorithm)
	{
	}

	public DESCryptoServiceProvider([NotNull] Encoding encoding) 
		: base(new System.Security.Cryptography.DESCryptoServiceProvider(), encoding)
	{
	}

	public DESCryptoServiceProvider([NotNull] System.Security.Cryptography.DESCryptoServiceProvider algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new DESCryptoServiceProvider(Algorithm, Encoding); }
}