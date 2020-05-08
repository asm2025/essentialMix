namespace asm.Cryptography.Asymmetric.Signature
{
	public interface ISignatureAlgorithm : ISignatureAlgorithmBase
	{
		byte[] SignHash(byte[] hash);

		bool VerifyHash(byte[] hash, byte[] signature);
	}
}