using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Asymmetric.Signature
{
	public abstract class SignatureAlgorithmBase<T, TParam> : AlgorithmEncodeBase<T>, ISignatureAlgorithmBase
		where T : AsymmetricAlgorithm
	{
		protected SignatureAlgorithmBase([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected SignatureAlgorithmBase([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}

		public int KeySize
		{
			get => Algorithm.KeySize;
			set => Algorithm.KeySize = value;
		}

		[NotNull]
		public KeySizes[] KeySizes => Algorithm.LegalKeySizes;

		[NotNull]
		public KeySizes[] LegalKeySizes => Algorithm.LegalKeySizes;

		public string KeyExchangeAlgorithm => Algorithm.KeyExchangeAlgorithm;

		[NotNull]
		public string SignatureAlgorithm => Algorithm.SignatureAlgorithm;

		public void FromXmlString([NotNull] string value) { Algorithm.FromXmlString(value); }
		[NotNull]
		public string ToXmlString(bool includePrivateParameters) { return Algorithm.ToXmlString(includePrivateParameters); }

		public abstract TParam ExportParameters(bool includePrivateParameters);
		public abstract void ImportParameters(TParam parameters);

		public void Clear() { Algorithm.Clear(); }
	}
}