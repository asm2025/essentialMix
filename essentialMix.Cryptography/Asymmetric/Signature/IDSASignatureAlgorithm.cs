using System.Security.Cryptography;

namespace essentialMix.Cryptography.Asymmetric.Signature;

public interface IDSASignatureAlgorithm : ISignatureAlgorithm
{
	DSAParameters ExportParameters(bool includePrivateParameters);
	void ImportParameters(DSAParameters parameters);
}