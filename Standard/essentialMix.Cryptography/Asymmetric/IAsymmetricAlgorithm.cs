using System.Security.Cryptography;

namespace essentialMix.Cryptography.Asymmetric
{
	public interface IAsymmetricAlgorithm : IEncrypt
	{
		int KeySize { get; set; }
		KeySizes[] KeySizes { get; }

		void Clear();
		void FromXmlString(string value);
		string ToXmlString(bool includePrivateParameters);
	}
}