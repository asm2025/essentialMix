using System;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Settings
{
	[Serializable]
	public class RSASettings : CryptographySettings
	{
		public const int DEFAULT_KEY_SIZE = 2048;
		public static readonly RSAEncryptionPadding DEFAULT_PADDING = RSAEncryptionPadding.Pkcs1;
		public static readonly RSASignaturePadding DEFAULT_SIGNATURE_PADDING = RSASignaturePadding.Pkcs1;
		public static readonly HashAlgorithmName DEFAULT_HASH_ALGORITHM = HashAlgorithmName.SHA1;

		/// <inheritdoc />
		public RSASettings()
			: this(DEFAULT_KEY_SIZE, null)
		{
		}

		/// <inheritdoc />
		public RSASettings(int keySize)
			: this(keySize, null)
		{
		}

		/// <inheritdoc />
		public RSASettings(int keySize, Encoding encoding)
			: base(keySize, encoding)
		{
		}

		[NotNull]
		public virtual RSAEncryptionPadding Padding { get; set; } = DEFAULT_PADDING;

		[NotNull]
		public virtual RSASignaturePadding SignaturePadding { get; set; } = DEFAULT_SIGNATURE_PADDING;

		public virtual HashAlgorithmName HashAlgorithm { get; set; } = DEFAULT_HASH_ALGORITHM;
	}
}