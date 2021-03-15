using JetBrains.Annotations;
using essentialMix.Text;

namespace essentialMix.Cryptography
{
	public interface IEncrypt : IAlgorithmBase, IEncoding
	{
		string Encrypt(string value);
		byte[] Encrypt([NotNull] byte[] buffer);
		byte[] Encrypt([NotNull] byte[] buffer, int startIndex, int count);

		string Decrypt(string value);
		byte[] Decrypt([NotNull] byte[] buffer);
		byte[] Decrypt([NotNull] byte[] buffer, int startIndex, int count);

		string RandomString(int length);
	}
}