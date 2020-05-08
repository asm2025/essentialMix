using JetBrains.Annotations;
using asm.Text;

namespace asm.Cryptography.Encoders
{
	public interface IEncode : IEncoding
	{
		byte[] EncodeToBytes(string value);
		string Encode(string value);
		string Encode([NotNull] byte[] buffer);
		string Encode([NotNull] byte[] buffer, int startIndex, int count);

		byte[] DecodeToBytes(string value);
		string Decode(string value);
	}
}