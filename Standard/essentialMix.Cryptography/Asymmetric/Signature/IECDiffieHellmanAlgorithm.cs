using System.Security.Cryptography;

namespace essentialMix.Cryptography.Asymmetric.Signature
{
	public interface IECDiffieHellmanAlgorithm : IECDsaAlgorithmBase
	{
		byte[] DeriveKeyMaterial(ECDiffieHellmanPublicKey otherPartyPublicKey);
		byte[] DeriveKeyFromHash(ECDiffieHellmanPublicKey otherPartyPublicKey, HashAlgorithmName hashAlgorithm);
		byte[] DeriveKeyFromHash(ECDiffieHellmanPublicKey otherPartyPublicKey, HashAlgorithmName hashAlgorithm, byte[] secretPrepend, byte[] secretAppend);
		byte[] DeriveKeyFromHmac(ECDiffieHellmanPublicKey otherPartyPublicKey, HashAlgorithmName hashAlgorithm, byte[] hmacKey);
		byte[] DeriveKeyFromHmac(ECDiffieHellmanPublicKey otherPartyPublicKey, HashAlgorithmName hashAlgorithm, byte[] hmacKey, byte[] secretPrepend, byte[] secretAppend);
		byte[] DeriveKeyTls(ECDiffieHellmanPublicKey otherPartyPublicKey, byte[] prfLabel, byte[] prfSeed);
		ECParameters ExportExplicitParameters(bool includePrivateParameters);
		void GenerateKey(ECCurve curve);
		ECDiffieHellmanPublicKey PublicKey { get; }
	}
}