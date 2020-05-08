using System.Security.Cryptography;

namespace asm.Cryptography.Asymmetric.Signature
{
	public interface IECDsaAlgorithmBase : ISignatureAlgorithmBase
	{
		ECParameters ExportParameters(bool includePrivateParameters);
		void ImportParameters(ECParameters parameters);
	}
}