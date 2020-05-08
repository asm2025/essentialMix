using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Asymmetric.Signature
{
	public abstract class ECDsaAlgorithmBase<T> : SignatureAlgorithm<T, ECParameters>, IECDsaAlgorithm
		where T : System.Security.Cryptography.ECDsa
	{
		protected ECDsaAlgorithmBase([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected ECDsaAlgorithmBase([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}

		public virtual ECParameters ExportExplicitParameters(bool includePrivateParameters) { return Algorithm.ExportExplicitParameters(includePrivateParameters); }

		public override ECParameters ExportParameters(bool includePrivateParameters) { return Algorithm.ExportParameters(includePrivateParameters); }

		public override void ImportParameters(ECParameters parameters) { Algorithm.ImportParameters(parameters); }

		public virtual void GenerateKey(ECCurve curve) { Algorithm.GenerateKey(curve); }

		[NotNull] public override byte[] SignHash([NotNull] byte[] hash) { return Algorithm.SignHash(hash); }

		public override bool VerifyHash([NotNull] byte[] hash, [NotNull] byte[] signature) { return Algorithm.VerifyHash(hash, signature); }
	}
}