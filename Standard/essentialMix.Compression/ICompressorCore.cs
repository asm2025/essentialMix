using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace essentialMix.Compression;

public interface ICompressorCore : IDisposable
{
	CompressionLevel CompressionLevel { get; set; }
	CompressionMode CompressionMode { get; set; }

	byte[] Compress([NotNull] byte[] value);
	byte[] Compress([NotNull] byte[] value, int startIndex);
	byte[] Compress([NotNull] byte[] value, int startIndex, int count);
	void Compress([NotNull] Stream source, [NotNull] Stream target);
	Task CompressAsync([NotNull] Stream source, [NotNull] Stream target, CancellationToken token = default(CancellationToken));
	void Compress([NotNull] string sourcePath, [NotNull] string destinationPath);
	Task CompressAsync([NotNull] string sourcePath, [NotNull] string destinationPath, CancellationToken token = default(CancellationToken));
	byte[] Decompress([NotNull] byte[] value);
	byte[] Decompress([NotNull] byte[] value, int startIndex);
	byte[] Decompress([NotNull] byte[] value, int startIndex, int count);
	void Decompress([NotNull] Stream source, [NotNull] Stream target);
	Task DecompressAsync([NotNull] Stream source, [NotNull] Stream target, CancellationToken token = default(CancellationToken));
	void Decompress([NotNull] string sourcePath, [NotNull] string destinationPath);
	Task DecompressAsync([NotNull] string sourcePath, [NotNull] string destinationPath, CancellationToken token = default(CancellationToken));
}