using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using essentialMix.Cryptography.Asymmetric;
using essentialMix.Cryptography.Encoders;
using essentialMix.Cryptography.Hash;
using essentialMix.Cryptography.RandomNumber;
using essentialMix.Cryptography.Settings;
using essentialMix.Cryptography.Symmetric;
using JetBrains.Annotations;

namespace essentialMix.Cryptography;

public static class QuickCipher
{
	private static readonly IQuickCipherService __quickCipher = new QuickCipherService();

	public static string Hash(string text, Encoding encoding = null)
	{
		return __quickCipher.Hash(text, encoding);
	}

	public static string Base64Encode(string text, Settings.Settings settings = null)
	{
		return __quickCipher.Base64Encode(text, settings);
	}

	public static string Base64Decode(string base64Text, Encoding encoding = null)
	{
		return __quickCipher.Base64Decode(base64Text, encoding);
	}

	public static string NumericEncode(string text, NumericSettings settings = null)
	{
		return __quickCipher.NumericEncode(text, settings);
	}

	public static string NumericDecode(string numericText, NumericSettings settings = null)
	{
		return __quickCipher.NumericDecode(numericText, settings);
	}

	public static string AsymmetricEncrypt([NotNull] string publicKeyXml, SecureString value, RSASettings settings = null)
	{
		return __quickCipher.AsymmetricEncrypt(publicKeyXml, value, settings);
	}

	public static string AsymmetricEncrypt([NotNull] string publicKeyXml, string value, RSASettings settings = null)
	{
		return __quickCipher.AsymmetricEncrypt(publicKeyXml, value, settings);
	}

	public static byte[] AsymmetricEncrypt([NotNull] string publicKeyXml, [NotNull] byte[] data, RSASettings settings = null)
	{
		return __quickCipher.AsymmetricEncrypt(publicKeyXml, data, settings);
	}

	public static string AsymmetricEncrypt([NotNull] X509Certificate2 certificate, SecureString value, RSASettings settings = null)
	{
		return __quickCipher.AsymmetricEncrypt(certificate, value, settings);
	}

	public static string AsymmetricEncrypt([NotNull] X509Certificate2 certificate, string value, RSASettings settings = null)
	{
		return __quickCipher.AsymmetricEncrypt(certificate, value, settings);
	}

	public static byte[] AsymmetricEncrypt([NotNull] X509Certificate2 certificate, [NotNull] byte[] data, RSASettings settings = null)
	{
		return __quickCipher.AsymmetricEncrypt(certificate, data, settings);
	}

	public static SecureString AsymmetricDecrypt([NotNull] string privateKeyXml, string value, RSASettings settings = null)
	{
		return __quickCipher.AsymmetricDecrypt(privateKeyXml, value, settings);
	}

	public static byte[] AsymmetricDecrypt([NotNull] string privateKeyXml, [NotNull] byte[] data, RSASettings settings = null)
	{
		return __quickCipher.AsymmetricDecrypt(privateKeyXml, data, settings);
	}

	public static SecureString AsymmetricDecrypt([NotNull] X509Certificate2 certificate, string value, RSASettings settings = null)
	{
		return __quickCipher.AsymmetricDecrypt(certificate, value, settings);
	}

	public static byte[] AsymmetricDecrypt([NotNull] X509Certificate2 certificate, [NotNull] byte[] data, RSASettings settings = null)
	{
		return __quickCipher.AsymmetricDecrypt(certificate, data, settings);
	}

	public static string SymmetricEncrypt([NotNull] string base64Key, SecureString value, SymmetricSettings settings = null)
	{
		return __quickCipher.SymmetricEncrypt(base64Key, value, settings);
	}

	public static string SymmetricEncrypt([NotNull] string base64Key, string value, SymmetricSettings settings = null)
	{
		return __quickCipher.SymmetricEncrypt(base64Key, value, settings);
	}

	public static byte[] SymmetricEncrypt([NotNull] byte[] key, [NotNull] byte[] data, SymmetricSettings settings = null)
	{
		return __quickCipher.SymmetricEncrypt(key, data, settings);
	}

	public static SecureString SymmetricDecrypt([NotNull] string base64Key, string value, SymmetricSettings settings = null)
	{
		return __quickCipher.SymmetricDecrypt(base64Key, value, settings);
	}

	public static byte[] SymmetricDecrypt([NotNull] byte[] key, [NotNull] byte[] data, SymmetricSettings settings = null)
	{
		return __quickCipher.SymmetricDecrypt(key, data, settings);
	}

	public static string HyperEncrypt([NotNull] string publicKeyXml, SecureString value, HyperSettings settings = null)
	{
		return __quickCipher.HyperEncrypt(publicKeyXml, value, settings);
	}

	public static string HyperEncrypt([NotNull] string publicKeyXml, string value, HyperSettings settings = null)
	{
		return __quickCipher.HyperEncrypt(publicKeyXml, value, settings);
	}

	public static byte[] HyperEncrypt([NotNull] string publicKeyXml, [NotNull] byte[] data, HyperSettings settings = null)
	{
		return __quickCipher.HyperEncrypt(publicKeyXml, data, settings);
	}

	public static SecureString HyperDecrypt([NotNull] string privateKeyXml, string value, HyperSettings settings = null)
	{
		return __quickCipher.HyperDecrypt(privateKeyXml, value, settings);
	}

	public static byte[] HyperDecrypt([NotNull] string privateKeyXml, [NotNull] byte[] data, HyperSettings settings = null)
	{
		return __quickCipher.HyperDecrypt(privateKeyXml, data, settings);
	}

	[NotNull]
	public static string RandomString(int length, NumericSettings settings = null)
	{
		return __quickCipher.RandomString(length, settings);
	}

	[NotNull]
	public static IRandomNumberGenerator CreateRandomNumberGenerator() { return __quickCipher.CreateRandomNumberGenerator(); }

	[NotNull]
	public static INumericEncoder CreateNumericEncoder(NumericSettings settings = null)
	{
		return __quickCipher.CreateNumericEncoder(settings);
	}

	[NotNull]
	public static IHashAlgorithm CreateHashAlgorithm(Encoding encoding = null) { return __quickCipher.CreateHashAlgorithm(encoding); }

	public static (string Public, string Private) GenerateAsymmetricKeys(RSASettings settings = null) { return GenerateAsymmetricKeys(true, settings); }
	public static (string Public, string Private) GenerateAsymmetricKeys(bool includeBitStrength, RSASettings settings = null)
	{
		return __quickCipher.GenerateAsymmetricKeys(includeBitStrength, settings);
	}

	[NotNull]
	public static byte[] GenerateSymmetricKey(SymmetricSettings settings = null)
	{
		return __quickCipher.GenerateSymmetricKey(settings);
	}

	[NotNull]
	public static IAsymmetricAlgorithm CreateAsymmetricAlgorithm(RSASettings settings = null)
	{
		return __quickCipher.CreateAsymmetricAlgorithm(settings);
	}

	[NotNull]
	public static ISymmetricAlgorithm CreateSymmetricAlgorithm(SymmetricSettings settings = null)
	{
		return __quickCipher.CreateSymmetricAlgorithm(settings);
	}
}