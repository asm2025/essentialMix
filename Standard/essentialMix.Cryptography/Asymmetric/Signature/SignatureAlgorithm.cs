using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Asymmetric.Signature;

public abstract class SignatureAlgorithm<T, TParam> : SignatureAlgorithmBase<T, TParam>, ISignatureAlgorithm
	where T : AsymmetricAlgorithm
{
	protected SignatureAlgorithm([NotNull] T algorithm)
		: base(algorithm)
	{
	}

	protected SignatureAlgorithm([NotNull] T algorithm, [NotNull] Encoding encoding)
		: base(algorithm, encoding)
	{
	}

	public abstract byte[] SignHash(byte[] hash);
	public abstract bool VerifyHash(byte[] hash, byte[] signature);
}