using System.IO;
using JetBrains.Annotations;
using asm.Text;

namespace asm.Cryptography.Hash
{
	public interface IHashAlgorithm : IAlgorithmBase, IEncoding
	{
		bool CanReuseTransform { get; }
		bool CanTransformMultipleBlocks { get; }
		byte[] Hash { get; }
		int HashSize { get; }
		int InputBlockSize { get; }
		int OutputBlockSize { get; }

		void Clear();
		string ComputeHash(string value);
		byte[] ComputeHash([NotNull] byte[] buffer);
		byte[] ComputeHash([NotNull] byte[] buffer, int offset, int count);
		byte[] ComputeHash([NotNull] Stream inputStream);
		int TransformBlock([NotNull] byte[] buffer, int offset, int count, byte[] outputBuffer, int outputOffset);
		byte[] TransformFinalBlock([NotNull] byte[] buffer, int offset, int count);
	}
}