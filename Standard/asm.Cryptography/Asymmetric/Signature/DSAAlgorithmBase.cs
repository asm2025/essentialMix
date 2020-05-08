using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Asymmetric.Signature
{
	/// <summary>
	/// DSA is very old algorithm and cannot be used for encryption / decryption
	/// It's just used for creating and verifying digital signatures
	/// 
	/// Newer asymmetric algorithms are available.
	/// Consider using the RSA class, the ECDsa class, or the ECDiffieHellman class instead of the DSA class.
	/// Use DSA only for compatibility with legacy applications and data.
	/// https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.dsa
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class DSAAlgorithmBase<T> : SignatureAlgorithm<T, DSAParameters>, IDSASignatureAlgorithm
		where T : System.Security.Cryptography.DSA
	{
		protected DSAAlgorithmBase([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected DSAAlgorithmBase([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}

		public override DSAParameters ExportParameters(bool includePrivateParameters) { return Algorithm.ExportParameters(includePrivateParameters); }

		public override void ImportParameters(DSAParameters parameters) { Algorithm.ImportParameters(parameters); }

		public override byte[] SignHash([NotNull] byte[] hash) { return Algorithm.CreateSignature(hash); }

		public override bool VerifyHash([NotNull] byte[] hash, [NotNull] byte[] signature) { return Algorithm.VerifySignature(hash, signature); }
	}
}