using System.Security;
using System.Security.Cryptography;
using JetBrains.Annotations;

namespace asm.Cryptography.Symmetric
{
	public interface ISymmetricAlgorithm : IEncrypt
	{
		int BlockSize { get; set; }
		KeySizes[] BlockSizes { get; }
		int FeedbackSize { get; set; }
		byte[] IV { get; set; }
		byte[] Key { get; set; }
		int KeySize { get; set; }
		KeySizes[] KeySizes { get; }
		CipherMode Mode { get; set; }
		PaddingMode Padding { get; set; }

		void Clear();
		void GenerateIV();
		void GenerateKey();
		void GenerateKey(SecureString passphrase, ushort saltSize = 0, int iterations = 0);
		void GenerateKey(SecureString passphrase, [NotNull] byte[] salt, int iterations = 100);
		bool ValidKeySize(int bitLength);
	}
}