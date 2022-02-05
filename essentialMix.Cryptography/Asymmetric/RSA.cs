using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Asymmetric;

public class RSA : RSAAlgorithmBase<System.Security.Cryptography.RSA>
{
	public RSA()
		: base(new System.Security.Cryptography.RSACng())
	{
	}

	public RSA([NotNull] System.Security.Cryptography.RSA algorithm) 
		: base(algorithm)
	{
	}

	public RSA([NotNull] Encoding encoding) 
		: base(new System.Security.Cryptography.RSACng(), encoding)
	{
	}

	public RSA([NotNull] System.Security.Cryptography.RSA algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new RSA(Algorithm, Encoding); }
}