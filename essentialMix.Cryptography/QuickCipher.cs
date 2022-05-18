using System;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using essentialMix.Cryptography.Asymmetric;
using essentialMix.Cryptography.Encoders;
using essentialMix.Cryptography.Hash;
using essentialMix.Cryptography.RandomNumber;
using essentialMix.Cryptography.Settings;
using essentialMix.Cryptography.Symmetric;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Numeric;
using JetBrains.Annotations;
using RNGCryptoServiceProvider = essentialMix.Cryptography.RandomNumber.RNGCryptoServiceProvider;
using RSACng = essentialMix.Cryptography.Asymmetric.RSACng;
using SHA256CryptoServiceProvider = essentialMix.Cryptography.Hash.SHA256CryptoServiceProvider;

namespace essentialMix.Cryptography;

public static class QuickCipher
{
	public static string Hash(string text, Encoding encoding = null)
	{
		if (string.IsNullOrEmpty(text)) return text;

		IHashAlgorithm hashAlgorithm = null;

		try
		{
			hashAlgorithm = CreateHashAlgorithm(encoding);
			return hashAlgorithm.ComputeHash(text);
		}
		finally
		{
			ObjectHelper.Dispose(ref hashAlgorithm);
		}
	}

	public static string Base64Encode(string text, Settings.Settings settings = null)
	{
		/*
		* finalBytes will contain the following:
		* +---------+-------+------+
		* | Salt    | Salt  | Data |
		* | bytes   | bytes |      |
		* | Length  |       |      |
		* | 2 bytes |       |      |
		* +---------+-------+------+
		*/
		if (string.IsNullOrEmpty(text)) return text;
		settings ??= new Settings.Settings();

		ushort saltSize = ByteHelper.GetByteSize(settings.SaltSize);
		byte[] bytes = settings.Encoding.GetBytes(text);
		byte[] finalBytes = new byte[Constants.SHORT_SIZE + saltSize // Salt
														+ bytes.Length // Data
		];
		int n = 0;
		BitConverter.GetBytes(saltSize).CopyTo(finalBytes, n);
		n = Constants.SHORT_SIZE;

		if (saltSize > 0)
		{
			byte[] saltBytes = new byte[saltSize];

			using (IRandomNumberGenerator random = CreateRandomNumberGenerator())
				random.GetNonZeroBytes(saltBytes);

			saltBytes.CopyTo(finalBytes, n);
			n += saltBytes.Length;
		}

		bytes.CopyTo(finalBytes, n);
		return Convert.ToBase64String(finalBytes);
	}

	public static string Base64Decode(string base64Text, Encoding encoding = null)
	{
		// See Base64Encode for details
		if (string.IsNullOrEmpty(base64Text)) return base64Text;
		byte[] finalBytes = Convert.FromBase64String(base64Text);
		ushort size = BitConverter.ToUInt16(finalBytes, 0);
		// Skip Salt
		int n = Constants.SHORT_SIZE + size;
		encoding ??= Encoding.Unicode;
		int len = finalBytes.ReverseIndexOf(byte.MinValue, n);
		if (len == n) return null;
		len = len < 0
				? finalBytes.Length - n
				: len - n;
		// Convert to string
		return encoding.GetString(finalBytes, n, len);
	}

	public static string NumericEncode(string text, NumericSettings settings = null)
	{
		if (string.IsNullOrEmpty(text)) return text;
		INumericEncoder encoder = null;

		try
		{
			encoder = CreateNumericEncoder(settings);
			return encoder.Encode(text);
		}
		finally
		{
			ObjectHelper.Dispose(ref encoder);
		}
	}

	public static string NumericDecode(string numericText, NumericSettings settings = null)
	{
		if (string.IsNullOrEmpty(numericText)) return numericText;
		INumericEncoder encoder = null;

		try
		{
			encoder = CreateNumericEncoder(settings);
			return encoder.Decode(numericText);
		}
		finally
		{
			ObjectHelper.Dispose(ref encoder);
		}
	}

	public static string AsymmetricEncrypt([NotNull] string publicKeyXml, SecureString value, RSASettings settings = null)
	{
		return value.IsNullOrEmpty()
					? null
					: AsymmetricEncrypt(publicKeyXml, value.UnSecure(), settings);
	}

	public static string AsymmetricEncrypt([NotNull] string publicKeyXml, string value, RSASettings settings = null)
	{
		if (publicKeyXml.Length == 0) throw new ArgumentNullException(nameof(publicKeyXml));
		if (string.IsNullOrEmpty(value)) return value;
		settings ??= new RSASettings();
		byte[] data = settings.Encoding.GetBytes(value);
		byte[] encrypted = AsymmetricEncrypt(publicKeyXml, data, settings);
		Array.Clear(data, 0, data.Length);
		return encrypted == null ? null : Convert.ToBase64String(encrypted);
	}

	public static byte[] AsymmetricEncrypt([NotNull] string publicKeyXml, [NotNull] byte[] data, RSASettings settings = null)
	{
		if (publicKeyXml.Length == 0) throw new ArgumentNullException(nameof(publicKeyXml));
		if (data.Length == 0) return null;

		IAsymmetricAlgorithm asymmetric = null;

		try
		{
			asymmetric = CreateAsymmetricAlgorithm(settings);
			asymmetric.FromXmlString(publicKeyXml);
			return AsymmetricEncrypt(asymmetric, data, settings);
		}
		finally
		{
			ObjectHelper.Dispose(ref asymmetric);
		}
	}

	public static string AsymmetricEncrypt([NotNull] X509Certificate2 certificate, SecureString value, RSASettings settings = null)
	{
		return value.IsNullOrEmpty()
					? null
					: AsymmetricEncrypt(certificate, value.UnSecure(), settings);
	}

	public static string AsymmetricEncrypt([NotNull] X509Certificate2 certificate, string value, RSASettings settings = null)
	{
		if (string.IsNullOrEmpty(value)) return value;
		Encoding encoding = settings?.Encoding ?? Encoding.UTF8;
		byte[] data = encoding.GetBytes(value);
		byte[] encrypted = AsymmetricEncrypt(certificate, data, settings);
		Array.Clear(data, 0, data.Length);
		return encrypted == null
					? null
					: Convert.ToBase64String(encrypted);
	}

	public static byte[] AsymmetricEncrypt([NotNull] X509Certificate2 certificate, [NotNull] byte[] data, RSASettings settings = null)
	{
		if (data.Length == 0) return null;
		IAsymmetricAlgorithm asymmetric = null;

		try
		{
			System.Security.Cryptography.RSA algorithm = certificate.GetPublicEncryptor<System.Security.Cryptography.RSA>();
			asymmetric = new RSAAlgorithm<System.Security.Cryptography.RSA>(algorithm);
			return AsymmetricEncrypt(asymmetric, data, settings);
		}
		finally
		{
			ObjectHelper.Dispose(ref asymmetric);
		}
	}

	private static byte[] AsymmetricEncrypt([NotNull] IAsymmetricAlgorithm asymmetric, [NotNull] byte[] data, RSASettings settings = null)
	{
		if (data.Length == 0) return null;

		if (settings is { UseExpiration: true })
		{
			byte[] expiration = BitConverter.GetBytes(settings.GetExpiration().Ticks);
			byte[] buffer = new byte[expiration.Length + data.Length];
			expiration.CopyTo(buffer, 0);
			data.CopyTo(buffer, expiration.Length);
			data = buffer;
		}

		return asymmetric.Encrypt(data);
	}

	public static SecureString AsymmetricDecrypt([NotNull] string privateKeyXml, string value, RSASettings settings = null)
	{
		if (privateKeyXml.Length == 0) throw new ArgumentNullException(nameof(privateKeyXml));
		if (string.IsNullOrEmpty(value)) return null;
		settings ??= new RSASettings();
		byte[] encrypted = Convert.FromBase64String(value);
		byte[] data = AsymmetricDecrypt(privateKeyXml, encrypted, settings);
		if (data == null) return null;
		int len = data.ReverseIndexOf(byte.MinValue);
		if (len == 0) return null;
		return (len > -1
					? settings.Encoding.GetString(data, 0, len)
					: settings.Encoding.GetString(data)).Secure();
	}

	public static byte[] AsymmetricDecrypt([NotNull] string privateKeyXml, [NotNull] byte[] data, RSASettings settings = null)
	{
		if (privateKeyXml.Length == 0) throw new ArgumentNullException(nameof(privateKeyXml));
		if (data.Length == 0) return null;

		IAsymmetricAlgorithm asymmetric = null;
		settings ??= new RSASettings();

		try
		{
			asymmetric = CreateAsymmetricAlgorithm(settings);
			asymmetric.FromXmlString(privateKeyXml);

			byte[] decrypted = asymmetric.Decrypt(data);

			if (settings.UseExpiration)
			{
				long ticks = BitConverter.ToInt64(decrypted, 0);
				DateTime expiration = new DateTime(ticks);
				if (DateTime.UtcNow > expiration) return null;
				decrypted = decrypted.GetRange(sizeof(long), -1);
			}

			return decrypted;
		}
		finally
		{
			ObjectHelper.Dispose(ref asymmetric);
		}
	}

	public static SecureString AsymmetricDecrypt([NotNull] X509Certificate2 certificate, string value, RSASettings settings = null)
	{
		if (string.IsNullOrEmpty(value)) return null;
		byte[] encrypted = Convert.FromBase64String(value);
		byte[] data = AsymmetricDecrypt(certificate, encrypted, settings);
		if (data == null) return null;
		int len = data.ReverseIndexOf(byte.MinValue);
		if (len == 0) return null;
		Encoding encoding = settings?.Encoding ?? Encoding.UTF8;
		return (len > -1
					? encoding.GetString(data, 0, len)
					: encoding.GetString(data)).Secure();
	}

	public static byte[] AsymmetricDecrypt([NotNull] X509Certificate2 certificate, [NotNull] byte[] data, RSASettings settings = null)
	{
		if (data.Length == 0) return null;
		IAsymmetricAlgorithm asymmetric = null;

		try
		{
			System.Security.Cryptography.RSA algorithm = certificate.GetPrivateDecryptor<System.Security.Cryptography.RSA>();
			asymmetric = new RSAAlgorithm<System.Security.Cryptography.RSA>(algorithm);
			return AsymmetricDecrypt(asymmetric, data, settings);
		}
		finally
		{
			ObjectHelper.Dispose(ref asymmetric);
		}
	}

	private static byte[] AsymmetricDecrypt([NotNull] IAsymmetricAlgorithm asymmetric, [NotNull] byte[] data, RSASettings settings = null)
	{
		if (data.Length == 0) return null;

		byte[] decrypted = asymmetric.Decrypt(data);

		if (settings is { UseExpiration: true })
		{
			long ticks = BitConverter.ToInt64(decrypted, 0);
			DateTime expiration = new DateTime(ticks);
			if (DateTime.UtcNow > expiration) return null;
			decrypted = decrypted.GetRange(sizeof(long), -1);
		}

		return decrypted;
	}

	public static string SymmetricEncrypt([NotNull] string base64Key, SecureString value, SymmetricSettings settings = null)
	{
		return value.IsNullOrEmpty()
					? null
					: SymmetricEncrypt(base64Key, value.UnSecure(), settings);
	}

	public static string SymmetricEncrypt([NotNull] string base64Key, string value, SymmetricSettings settings = null)
	{
		if (base64Key.Length == 0) throw new ArgumentNullException(nameof(base64Key));
		if (string.IsNullOrEmpty(value)) return value;

		byte[] key = Convert.FromBase64String(base64Key);
		settings ??= new SymmetricSettings();
		byte[] data = settings.Encoding.GetBytes(value);
		byte[] encrypted = SymmetricEncrypt(key, data, settings);
		Array.Clear(data, 0, data.Length);
		return encrypted == null ? null : Convert.ToBase64String(encrypted);
	}

	public static byte[] SymmetricEncrypt([NotNull] byte[] key, [NotNull] byte[] data, SymmetricSettings settings = null)
	{
		if (key.Length == 0) throw new ArgumentNullException(nameof(key));
		if (data.Length == 0) return null;
		int keySize = BitHelper.GetBitSize(key.Length);

		if (settings == null) settings = new SymmetricSettings(keySize);
		else settings.KeySize = keySize;

		ISymmetricAlgorithm symmetric = null;

		try
		{
			symmetric = CreateSymmetricAlgorithm(settings);
			symmetric.Key = key;

			if (settings.UseExpiration)
			{
				byte[] expiration = BitConverter.GetBytes(settings.GetExpiration().Ticks);
				byte[] buffer = new byte[expiration.Length + data.Length];
				expiration.CopyTo(buffer, 0);
				data.CopyTo(buffer, expiration.Length);
				data = buffer;
			}

			return symmetric.Encrypt(data);
		}
		finally
		{
			ObjectHelper.Dispose(ref symmetric);
		}
	}

	public static SecureString SymmetricDecrypt([NotNull] string base64Key, string value, SymmetricSettings settings = null)
	{
		if (base64Key.Length == 0) throw new ArgumentNullException(nameof(base64Key));
		if (string.IsNullOrEmpty(value)) return null;

		byte[] key = Convert.FromBase64String(base64Key);
		settings ??= new SymmetricSettings();
		byte[] encrypted = Convert.FromBase64String(value);
		byte[] data = SymmetricDecrypt(key, encrypted, settings);
		if (data == null) return null;
		int len = data.ReverseIndexOf(byte.MinValue);
		if (len == 0) return null;
		return (len > -1
					? settings.Encoding.GetString(data, 0, len)
					: settings.Encoding.GetString(data)).Secure();
	}

	public static byte[] SymmetricDecrypt([NotNull] byte[] key, [NotNull] byte[] data, SymmetricSettings settings = null)
	{
		if (key.Length == 0) throw new ArgumentNullException(nameof(key));
		if (data.Length == 0) return null;
		int keySize = BitHelper.GetBitSize(key.Length);

		if (settings == null) settings = new SymmetricSettings(keySize);
		else settings.KeySize = keySize;

		ISymmetricAlgorithm symmetric = null;

		try
		{
			symmetric = CreateSymmetricAlgorithm(settings);
			symmetric.Key = key;

			byte[] decrypted = symmetric.Decrypt(data);

			if (settings.UseExpiration)
			{
				long ticks = BitConverter.ToInt64(decrypted, 0);
				DateTime expiration = new DateTime(ticks);
				if (DateTime.UtcNow > expiration) return null;
				decrypted = decrypted.GetRange(sizeof(long), -1);
			}

			return decrypted;
		}
		finally
		{
			ObjectHelper.Dispose(ref symmetric);
		}
	}

	public static string HyperEncrypt([NotNull] string publicKeyXml, SecureString value, HyperSettings settings = null)
	{
		return value.IsNullOrEmpty()
					? null
					: HyperEncrypt(publicKeyXml, value.UnSecure(), settings);
	}

	public static string HyperEncrypt([NotNull] string publicKeyXml, string value, HyperSettings settings = null)
	{
		if (publicKeyXml.Length == 0) throw new ArgumentNullException(nameof(publicKeyXml));
		if (string.IsNullOrEmpty(value)) return value;
		settings ??= new HyperSettings();
		byte[] data = settings.Encoding.GetBytes(value);
		byte[] encrypted = HyperEncrypt(publicKeyXml, data, settings);
		Array.Clear(data, 0, data.Length);
		return encrypted == null ? null : Convert.ToBase64String(encrypted);
	}

	public static byte[] HyperEncrypt([NotNull] string publicKeyXml, [NotNull] byte[] data, HyperSettings settings = null)
	{
		/*
		* finalBytes will contain the following:
		* +-----------+-----------+--------+
		* | Symmetric | Symmetric | Data   |
		* | key       | key       | bytes  |
		* | length    | bytes     |        |
		* | 4 bytes   |           |        |
		* +-----------+-----------+--------+
		*/
		if (publicKeyXml.Length == 0) throw new ArgumentNullException(nameof(publicKeyXml));
		if (data.Length == 0) return null;

		// Key
		byte[] key = GenerateSymmetricKey(settings);
		byte[] encryptedKey = AsymmetricEncrypt(publicKeyXml, key, settings?.RSASettings);
		if (encryptedKey == null) return null;

		// Data
		byte[] encryptedData = SymmetricEncrypt(key, data, settings);
		if (encryptedData == null) return null;

		int n = 0;
		byte[] finalBytes = new byte[Constants.INT_SIZE + encryptedKey.Length // Key
														+ encryptedData.Length // Data
		];

		// Key
		BitConverter.GetBytes(encryptedKey.Length).CopyTo(finalBytes, n);
		n = Constants.INT_SIZE;
		encryptedKey.CopyTo(finalBytes, n);
		n += encryptedKey.Length;

		// Data
		encryptedData.CopyTo(finalBytes, n);
		return finalBytes;
	}

	public static SecureString HyperDecrypt([NotNull] string privateKeyXml, string value, HyperSettings settings = null)
	{
		if (privateKeyXml.Length == 0) throw new ArgumentNullException(nameof(privateKeyXml));
		if (string.IsNullOrEmpty(value)) return null;
		settings ??= new HyperSettings();
		byte[] encrypted = Convert.FromBase64String(value);
		byte[] data = HyperDecrypt(privateKeyXml, encrypted, settings);
		if (data == null) return null;
		int len = data.ReverseIndexOf(byte.MinValue);
		if (len == 0) return null;
		return (len > -1
					? settings.Encoding.GetString(data, 0, len)
					: settings.Encoding.GetString(data)).Secure();
	}

	public static byte[] HyperDecrypt([NotNull] string privateKeyXml, [NotNull] byte[] data, HyperSettings settings = null)
	{
		// see Encrypt method for details
		if (data.Length < Constants.INT_SIZE) return null;

		// Key
		int n = 0;
		int size = BitConverter.ToInt32(data, n);
		n = Constants.INT_SIZE;
		if (n + size > data.Length) return null;

		byte[] encryptedKey = data.GetRange(n, size);
		// Decrypt key
		byte[] key = AsymmetricDecrypt(privateKeyXml, encryptedKey, settings?.RSASettings);
		if (key == null) return null;
		n += size;

		byte[] encryptedData = data.GetRange(n, size);
		return SymmetricDecrypt(key, encryptedData, settings);
	}

	[NotNull]
	public static string RandomString(int length, NumericSettings settings = null)
	{
		if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
		if (length == 0) return string.Empty;

		INumericEncoder encoder = null;
		IRandomNumberGenerator random = null;

		try
		{
			encoder = CreateNumericEncoder(settings);

			int unitLength = BitVector.GetUnitLength(encoder.Mode);
			int n = length * unitLength;
			byte[] bytes = new byte[n];
			random = CreateRandomNumberGenerator();
			random.GetNonZeroBytes(bytes);
			return encoder.Encode(bytes).Left(length);
		}
		finally
		{
			ObjectHelper.Dispose(ref random);
			ObjectHelper.Dispose(ref encoder);
		}
	}

	[NotNull]
	public static IRandomNumberGenerator CreateRandomNumberGenerator() { return new RNGCryptoServiceProvider(); }

	[NotNull]
	public static INumericEncoder CreateNumericEncoder(NumericSettings settings = null)
	{
		settings ??= new NumericSettings();
		return new NumericEncoder(settings.Mode, settings.Encoding);
	}

	[NotNull]
	public static IHashAlgorithm CreateHashAlgorithm(Encoding encoding = null) { return new SHA256CryptoServiceProvider(encoding ?? Encoding.Unicode); }

	public static (string Public, string Private) GenerateAsymmetricKeys(RSASettings settings = null) { return GenerateAsymmetricKeys(true, settings); }
	public static (string Public, string Private) GenerateAsymmetricKeys(bool includeBitStrength, RSASettings settings = null)
	{
		using (IAsymmetricAlgorithm asymmetric = CreateAsymmetricAlgorithm(settings))
		{
			string bitStrength = includeBitStrength ? $"<BitStrength>{asymmetric.KeySize}</BitStrength>" : string.Empty;
			string privateKey = $"{bitStrength}{asymmetric.ToXmlString(true)}";
			string publicKey = $"{asymmetric.ToXmlString(false)}";
			return (publicKey, privateKey);
		}
	}

	[NotNull]
	public static byte[] GenerateSymmetricKey(SymmetricSettings settings = null)
	{
		byte[] keyBytes;
		settings ??= new SymmetricSettings();

		using (ISymmetricAlgorithm symmetric = CreateSymmetricAlgorithm(settings))
		{
			int keySize = ByteHelper.GetByteSize(symmetric.KeySize);
			string passphraseStr = symmetric.RandomString(keySize);
			symmetric.GenerateKey(passphraseStr.Secure(), settings.SaltSize, settings.RFC2898Iterations);
			keyBytes = new byte[ByteHelper.GetByteSize(symmetric.KeySize)];
			Buffer.BlockCopy(symmetric.Key, 0, keyBytes, 0, keyBytes.Length);
		}

		return keyBytes;
	}

	[NotNull]
	public static IAsymmetricAlgorithm CreateAsymmetricAlgorithm(RSASettings settings = null)
	{
		settings ??= new RSASettings();
		IRSAAlgorithm algorithm = new RSACng(settings.Encoding)
		{
			KeySize = settings.KeySize,
			Padding = settings.Padding,
			SignaturePadding = settings.SignaturePadding,
			HashAlgorithm = settings.HashAlgorithm
		};
		return algorithm;
	}

	[NotNull]
	public static ISymmetricAlgorithm CreateSymmetricAlgorithm(SymmetricSettings settings = null)
	{
		settings ??= new SymmetricSettings();
		ISymmetricAlgorithm algorithm = new AESCryptoServiceProvider(settings.Encoding)
		{
			KeySize = settings.KeySize,
			BlockSize = settings.BlockSize,
			Mode = settings.Mode,
			Padding = settings.PaddingMode
		};
		return algorithm;
	}
}