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

public interface IQuickCipherService
{
	string Hash(string text, Encoding encoding = null);
	string Base64Encode(string text, Settings.Settings settings = null);
	string Base64Decode(string base64Text, Encoding encoding = null);
	string NumericEncode(string text, NumericSettings settings = null);
	string NumericDecode(string numericText, NumericSettings settings = null);
	string AsymmetricEncrypt([NotNull] string publicKeyXml, SecureString value, RSASettings settings = null);
	string AsymmetricEncrypt([NotNull] string publicKeyXml, string value, RSASettings settings = null);
	byte[] AsymmetricEncrypt([NotNull] string publicKeyXml, [NotNull] byte[] data, RSASettings settings = null);
	string AsymmetricEncrypt([NotNull] X509Certificate2 certificate, SecureString value, RSASettings settings = null);
	string AsymmetricEncrypt([NotNull] X509Certificate2 certificate, string value, RSASettings settings = null);
	byte[] AsymmetricEncrypt([NotNull] X509Certificate2 certificate, [NotNull] byte[] data, RSASettings settings = null);
	SecureString AsymmetricDecrypt([NotNull] string privateKeyXml, string value, RSASettings settings = null);
	byte[] AsymmetricDecrypt([NotNull] string privateKeyXml, [NotNull] byte[] data, RSASettings settings = null);
	SecureString AsymmetricDecrypt([NotNull] X509Certificate2 certificate, string value, RSASettings settings = null);
	byte[] AsymmetricDecrypt([NotNull] X509Certificate2 certificate, [NotNull] byte[] data, RSASettings settings = null);
	(string Public, string Private) GenerateAsymmetricKeys(RSASettings settings = null);
	(string Public, string Private) GenerateAsymmetricKeys(bool includeBitStrength, RSASettings settings = null);
	string SymmetricEncrypt([NotNull] string base64Key, SecureString value, SymmetricSettings settings = null);
	string SymmetricEncrypt([NotNull] string base64Key, string value, SymmetricSettings settings = null);
	byte[] SymmetricEncrypt([NotNull] byte[] key, [NotNull] byte[] data, SymmetricSettings settings = null);
	SecureString SymmetricDecrypt([NotNull] string base64Key, string value, SymmetricSettings settings = null);
	byte[] SymmetricDecrypt([NotNull] byte[] key, [NotNull] byte[] data, SymmetricSettings settings = null);
	[NotNull]
	byte[] GenerateSymmetricKey(SymmetricSettings settings = null);
	string HyperEncrypt([NotNull] string publicKeyXml, SecureString value, HyperSettings settings = null);
	string HyperEncrypt([NotNull] string publicKeyXml, string value, HyperSettings settings = null);
	byte[] HyperEncrypt([NotNull] string publicKeyXml, [NotNull] byte[] data, HyperSettings settings = null);
	SecureString HyperDecrypt([NotNull] string privateKeyXml, string value, HyperSettings settings = null);
	byte[] HyperDecrypt([NotNull] string privateKeyXml, [NotNull] byte[] data, HyperSettings settings = null);
	[NotNull]
	string RandomString(int length, NumericSettings settings = null);

	[NotNull]
	IRandomNumberGenerator CreateRandomNumberGenerator();
	[NotNull]
	INumericEncoder CreateNumericEncoder(NumericSettings settings = null);
	[NotNull]
	IHashAlgorithm CreateHashAlgorithm(Encoding encoding = null);
	[NotNull]
	IAsymmetricAlgorithm CreateAsymmetricAlgorithm(RSASettings settings = null);
	[NotNull]
	ISymmetricAlgorithm CreateSymmetricAlgorithm(SymmetricSettings settings = null);
}