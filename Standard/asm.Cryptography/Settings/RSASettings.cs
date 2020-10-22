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
		public static readonly RSAEncryptionPadding DefaultPadding = RSAEncryptionPadding.Pkcs1;
		public static readonly RSASignaturePadding DefaultSignaturePadding = RSASignaturePadding.Pkcs1;
		public static readonly HashAlgorithmName DefaultHashAlgorithm = HashAlgorithmName.SHA1;

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
		public virtual RSAEncryptionPadding Padding { get; set; } = DefaultPadding;

		[NotNull]
		public virtual RSASignaturePadding SignaturePadding { get; set; } = DefaultSignaturePadding;

		public virtual HashAlgorithmName HashAlgorithm { get; set; } = DefaultHashAlgorithm;
	}
}