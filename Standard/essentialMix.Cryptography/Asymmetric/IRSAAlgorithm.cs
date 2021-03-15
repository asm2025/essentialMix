using System.IO;
using System.Security.Cryptography;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Asymmetric
{
	public interface IRSAAlgorithm : IAsymmetricAlgorithm
	{
		[NotNull]
		RSAEncryptionPadding Padding { get; set; }

		[NotNull]
		RSASignaturePadding SignaturePadding { get; set; }

		HashAlgorithmName HashAlgorithm { get; set; }

		[NotNull]
		KeySizes[] LegalKeySizes { get; }

		byte[] Encrypt([NotNull] byte[] buffer, RSAEncryptionPadding padding);
		byte[] Encrypt([NotNull] byte[] buffer, int startIndex, int count, RSAEncryptionPadding padding);
		byte[] Decrypt([NotNull] byte[] buffer, RSAEncryptionPadding padding);
		byte[] Decrypt([NotNull] byte[] buffer, int startIndex, int count, RSAEncryptionPadding padding);
		byte[] SignData([NotNull] byte[] buffer);
		byte[] SignData([NotNull] byte[] buffer, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding);
		byte[] SignData([NotNull] byte[] buffer, int offset, int count, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding);
		byte[] SignData([NotNull] Stream data);
		byte[] SignData([NotNull] Stream data, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding);
		bool VerifyData([NotNull] byte[] buffer, [NotNull] byte[] signature);
		bool VerifyData([NotNull] byte[] buffer, [NotNull] byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding);
		bool VerifyData([NotNull] byte[] buffer, int offset, int count, [NotNull] byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding);
		bool VerifyData([NotNull] Stream data, [NotNull] byte[] signature);
		bool VerifyData([NotNull] Stream data, [NotNull] byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding);
		byte[] SignHash([NotNull] byte[] buffer);
		byte[] SignHash([NotNull] byte[] buffer, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding);
		bool VerifyHash([NotNull] byte[] buffer, [NotNull] byte[] signature);
		bool VerifyHash([NotNull] byte[] buffer, [NotNull] byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding);
		RSAParameters ExportParameters(bool includePrivateParameters);
		void ImportParameters(RSAParameters parameters);
		void SetPublicKey([NotNull] byte[] modulus, [NotNull] byte[] exponent);
	}
}