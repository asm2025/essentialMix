using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash;

public class SHA384CryptoServiceProvider : SHA384<System.Security.Cryptography.SHA384CryptoServiceProvider>
{
	public SHA384CryptoServiceProvider()
		: base(new System.Security.Cryptography.SHA384CryptoServiceProvider())
	{
	}

	public SHA384CryptoServiceProvider([NotNull] System.Security.Cryptography.SHA384CryptoServiceProvider algorithm)
		: base(algorithm)
	{
	}

	public SHA384CryptoServiceProvider([NotNull] Encoding encoding) 
		: base(new System.Security.Cryptography.SHA384CryptoServiceProvider(), encoding)
	{
	}

	public SHA384CryptoServiceProvider([NotNull] System.Security.Cryptography.SHA384CryptoServiceProvider algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new SHA384CryptoServiceProvider(Algorithm, Encoding); }
}