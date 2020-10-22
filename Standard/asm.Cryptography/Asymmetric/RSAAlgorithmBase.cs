using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using asm.Cryptography.Settings;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Cryptography.Asymmetric
{
	/*
		Usage
			x.ExportParameters(bool)
			x.ImportParameters(RSAParameters)

			x.Encrypt(plainText);
			x.Encrypt(byte[]);
			x.Encrypt(byte[], RSAEncryptionPadding);

			x.Decrypt(encryptedText);
			x.Decrypt(byte[]);
			x.Decrypt(byte[], RSAEncryptionPadding);

			x.SignData(byte[], HashAlgorithmName, RSASignaturePadding);
			x.SignData(byte[], int, int, HashAlgorithmName, RSASignaturePadding);
			x.SignData(Stream, HashAlgorithmName, RSASignaturePadding);
			x.VerifyData(byte[], HashAlgorithmName, RSASignaturePadding);
			x.VerifyData(byte[], int, int, HashAlgorithmName, RSASignaturePadding);
			x.VerifyData(Stream, HashAlgorithmName, RSASignaturePadding);

			x.SignHash(byte[], HashAlgorithmName, RSASignaturePadding);
			x.VerifyHash(byte[], byte[], HashAlgorithmName, RSASignaturePadding);
	*/

	public abstract class RSAAlgorithmBase<T> : AsymmetricAlgorithmBase<T>, IRSAAlgorithm 
		where T : System.Security.Cryptography.RSA
	{
		protected RSAAlgorithmBase([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected RSAAlgorithmBase([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}

		/// <summary>
		/// https://github.com/dotnet/corefx/blob/master/Documentation/architecture/cross-platform-cryptography.md#rsa
		/// </summary>
		public RSAEncryptionPadding Padding { get; set; } = RSASettings.DefaultPadding;
		public RSASignaturePadding SignaturePadding { get; set; } = RSASettings.DefaultSignaturePadding;
		public HashAlgorithmName HashAlgorithm { get; set; } = RSASettings.DefaultHashAlgorithm;

		public KeySizes[] LegalKeySizes => Algorithm.LegalKeySizes;

		/// <summary>
		/// Computes the maximum chunk size that can be encrypted/decrypted in bytes
		/// https://tools.ietf.org/html/rfc8017
		/// https://crypto.stackexchange.com/questions/42097/what-is-the-maximum-size-of-the-plaintext-message-for-rsa-oaep
		/// The output of this method should be calculated against the encoding char size.
		/// Use <see cref="GetSegmentLength" /> to get the length in characters
		/// i.e. RSA with key size of 2048 and hash SHA-256 will produce 190 bytes.
		/// The characters lengths result must be a multiplication of 8.
		/// </summary>
		public virtual int GetSegmentByteLength()
		{
			int mLen;

			switch (Padding.Mode)
			{
				case RSAEncryptionPaddingMode.Pkcs1:
					/*
					 * PKCS#1	https://en.wikipedia.org/wiki/PKCS
					 *			https://www.cryptsoft.com/pkcs11doc/v230/group__SEC__11__1__17__PKCS____1__RSA__PSS__SIGNATURE__WITH__SHA__1____SHA__256____SHA__384__OR__SHA__512.html
					 *			The operations performed are as described in PKCS #1 with
					 *			the object identifier id-RSASSA-PSS, i.e., as in the scheme
					 *			RSASSA-PSS in PKCS #1 where the underlying hash function is SHA-1.
					 *
					 *			SHA-1 produces a 160-bit (20-byte) hash value
					 *			https://en.wikipedia.org/wiki/SHA-1
					 *
					 * https://tools.ietf.org/html/rfc8017#section-7.2.2
					 * M		message to be encrypted, an octet string of length mLen,
					 *			where mLen <= k - 11
					 * k		length in octets of the RSA modulus n
					 * hLen		length in octets of a message M
					 * mLen		length in octets of a message M
					 *
					 *			k = ceil(kLenBits / 8)
					 *			where kLenBits = key size in bits
					 *
					 *			mLen = k - 11
					 *			     = kLenBits / 8 - 11
					 */
					mLen = (int)Math.Floor(KeySize / 8.0d) - 11;
					break;
				case RSAEncryptionPaddingMode.Oaep:
					/*
					 * https://tools.ietf.org/html/rfc8017#section-7.1.2
					 * M		message to be encrypted, an octet string of length mLen,
					 *			where mLen <= k - 2hLen - 2
					 * k		length in octets of the RSA modulus n
					 * hLen		length in octets of a message M
					 * mLen		length in octets of a message M
					 *
					 *			k = ceil(kLenBits / 8)
					 *			where kLenBits = key size in bits
					 *
					 *			hLen = ceil(hLenBits / 8)
					 *			where hLenBits = hash output in bits
					 *
					 *			mLen = k - 2 * hLen - 2
					 *			     = kLenBits / 8 - 2 * hLenBits / 8 - 2
					 */
					int hLenBits;

					if (Padding.OaepHashAlgorithm == HashAlgorithmName.MD5)
					{
						/*
						 * MD5 processes a variable-length message into a fixed-length output of 128 bits
						 * giving 16 bytes (128 / 2) or 32 hex characters (128 / 2 * 2)
						 * https://en.wikipedia.org/wiki/MD5
						 */
						hLenBits = 128;
					}
					else if (Padding.OaepHashAlgorithm == HashAlgorithmName.SHA1)
					{
						/*
						 * produces a 160-bit (20-byte) hash value
						 * https://en.wikipedia.org/wiki/SHA-1
						 */
						hLenBits = 160;
					}
					else if (Padding.OaepHashAlgorithm == HashAlgorithmName.SHA256)
					{
						/*
						 * https://en.wikipedia.org/wiki/SHA-2
						 * Obviously 256 bits
						 */
						hLenBits = 256;
					}
					else if (Padding.OaepHashAlgorithm == HashAlgorithmName.SHA384)
					{
						// 384 bits
						hLenBits = 384;
					}
					else if (Padding.OaepHashAlgorithm == HashAlgorithmName.SHA512)
					{
						// 512 bits
						hLenBits = 512;
					}
					else
					{
						throw new ArgumentOutOfRangeException(nameof(Padding.OaepHashAlgorithm));
					}

					mLen = (int)Math.Floor(KeySize / 8.0d) - 2 * (int)Math.Ceiling(hLenBits / 8.0d) - 2;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(Padding.Mode));
			}

			return mLen;
		}

		/// <summary>
		/// Computes the maximum chunk size that can be encrypted/decrypted in characters using the current encoding.
		/// <seealso cref="GetSegmentByteLength" /> to get the length in bytes
		/// </summary>
		/// <returns></returns>
		public int GetSegmentLength() { return Encoding.GetMaxCharCount(GetSegmentByteLength()); }

		public byte[] Encrypt(byte[] buffer, RSAEncryptionPadding padding) { return Encrypt(buffer, 0, buffer.Length, padding); }

		/// <inheritdoc />
		public override byte[] Encrypt(byte[] buffer, int startIndex, int count) { return Encrypt(buffer, startIndex, count, Padding); }

		public virtual byte[] Encrypt(byte[] buffer, int startIndex, int count, RSAEncryptionPadding padding)
		{
			buffer = ArrayHelper.ValidateAndGetRange(buffer, ref startIndex, ref count);
			if (buffer.Length == 0) return buffer;
			if (count == 0) return Array.Empty<byte>();
			return Algorithm.Encrypt(buffer, padding);
		}

		public byte[] Decrypt(byte[] buffer, RSAEncryptionPadding padding) { return Decrypt(buffer, 0, buffer.Length, padding); }

		/// <inheritdoc />
		public override byte[] Decrypt(byte[] buffer, int startIndex, int count) { return Decrypt(buffer, startIndex, count, Padding); }

		public virtual byte[] Decrypt(byte[] buffer, int startIndex, int count, RSAEncryptionPadding padding)
		{
			buffer = ArrayHelper.ValidateAndGetRange(buffer, ref startIndex, ref count);
			if (buffer.Length == 0) return buffer;
			if (count == 0) return Array.Empty<byte>();
			return Algorithm.Decrypt(buffer, padding);
		}

		public byte[] SignData(byte[] buffer) { return SignData(buffer, 0, buffer.Length, HashAlgorithm, SignaturePadding); }

		public byte[] SignData(byte[] buffer, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) { return SignData(buffer, 0, buffer.Length, hashAlgorithm, padding); }

		public virtual byte[] SignData(byte[] buffer, int offset, int count, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
		{
			return Algorithm.SignData(buffer, offset, count, hashAlgorithm, padding);
		}

		public byte[] SignData(Stream data) { return SignData(data, HashAlgorithm, SignaturePadding); }

		public virtual byte[] SignData(Stream data, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) { return Algorithm.SignData(data, hashAlgorithm, padding); }

		public bool VerifyData(byte[] buffer, byte[] signature) { return VerifyData(buffer, signature, HashAlgorithm, SignaturePadding); }

		public bool VerifyData(byte[] buffer, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
		{
			return VerifyData(buffer, 0, buffer.Length, signature, hashAlgorithm, padding);
		}

		public virtual bool VerifyData(byte[] buffer, int offset, int count, byte[] signature, HashAlgorithmName hashAlgorithm,
			RSASignaturePadding padding)
		{
			return Algorithm.VerifyData(buffer, offset, count, signature, hashAlgorithm, padding);
		}

		public bool VerifyData(Stream data, byte[] signature) { return VerifyData(data, signature, HashAlgorithm, SignaturePadding); }

		public virtual bool VerifyData(Stream data, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
		{
			return Algorithm.VerifyData(data, signature, hashAlgorithm, padding);
		}

		public byte[] SignHash(byte[] buffer) { return SignHash(buffer, HashAlgorithm, SignaturePadding); }

		public virtual byte[] SignHash(byte[] buffer, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) { return Algorithm.SignHash(buffer, hashAlgorithm, padding); }

		public bool VerifyHash(byte[] buffer, byte[] signature) { return VerifyHash(buffer, signature, HashAlgorithm, SignaturePadding); }

		public virtual bool VerifyHash(byte[] buffer, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
		{
			return Algorithm.VerifyHash(buffer, signature, hashAlgorithm, padding);
		}

		public RSAParameters ExportParameters(bool includePrivateParameters) { return Algorithm.ExportParameters(includePrivateParameters); }

		public void ImportParameters(RSAParameters parameters) { Algorithm.ImportParameters(parameters); }

		public void SetPublicKey(byte[] modulus, byte[] exponent)
		{
			RSAParameters parameters = ExportParameters(false);
			parameters.Modulus = modulus;
			parameters.Exponent = exponent;
			ImportParameters(parameters);
		}
	}
}