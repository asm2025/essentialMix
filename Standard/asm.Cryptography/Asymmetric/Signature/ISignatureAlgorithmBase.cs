using System.Security.Cryptography;

namespace asm.Cryptography.Asymmetric.Signature
{
	public interface ISignatureAlgorithmBase : IAlgorithmBase
	{
		int KeySize { get; set; }
		KeySizes[] KeySizes { get; }
		KeySizes[] LegalKeySizes { get; }
		string SignatureAlgorithm { get; }
		string KeyExchangeAlgorithm { get; }

		void Clear();

		void FromXmlString(string value);
		string ToXmlString(bool includePrivateParameters);
	}
}