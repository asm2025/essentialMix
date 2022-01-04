using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Asymmetric;

public class RSACryptoServiceProvider : RSAAlgorithmBase<System.Security.Cryptography.RSACryptoServiceProvider>
{
	public RSACryptoServiceProvider()
		: base(new System.Security.Cryptography.RSACryptoServiceProvider())
	{
	}

	public RSACryptoServiceProvider([NotNull] System.Security.Cryptography.RSACryptoServiceProvider algorithm)
		: base(algorithm)
	{
	}

	public RSACryptoServiceProvider([NotNull] Encoding encoding)
		: base(new System.Security.Cryptography.RSACryptoServiceProvider(), encoding)
	{
	}

	public RSACryptoServiceProvider([NotNull] System.Security.Cryptography.RSACryptoServiceProvider algorithm, [NotNull] Encoding encoding)
		: base(algorithm, encoding)
	{
	}

	[NotNull]
	public CspKeyContainerInfo CspKeyInfo => Algorithm.CspKeyContainerInfo;

	public bool PublicOnly => Algorithm.PublicOnly;

	public bool PersistKeyInCsp
	{
		get => Algorithm.PersistKeyInCsp;
		set => Algorithm.PersistKeyInCsp = value;
	}

	public override object Clone() { return new RSACryptoServiceProvider(Algorithm, Encoding); }
}