using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace essentialMix.Compression
{
	public interface ICompressorEx : ICompressor
	{
		bool IncludeEmptyDirectories { get; set; }
		bool PreserveDirectoryStructure { get; set; }

		void Compress([NotNull] Stream source, [NotNull] Stream target, string password);
		Task CompressAsync([NotNull] Stream source, [NotNull] Stream target, string password, CancellationToken token = default(CancellationToken));
		void Compress([NotNull] string sourcePath, [NotNull] string destinationPath, string password, params string[] files);
		Task CompressAsync([NotNull] string sourcePath, [NotNull] string destinationPath, string password, CancellationToken token, params string[] files);
		void Compress([NotNull] string sourcePath, [NotNull] string destinationPath, string password, [NotNull] IEnumerable<string> files);
		Task CompressAsync([NotNull] string sourcePath, [NotNull] string destinationPath, string password, [NotNull] IEnumerable<string> files, CancellationToken token = default(CancellationToken));
		void Decompress([NotNull] Stream source, [NotNull] Stream target, string password);
		Task DecompressAsync([NotNull] Stream source, [NotNull] Stream target, string password, CancellationToken token = default(CancellationToken));
		void Decompress([NotNull] string sourcePath, [NotNull] string destinationPath, string password, params string[] files);
		void Decompress([NotNull] string sourcePath, [NotNull] string destinationPath, string password, [NotNull] IEnumerable<string> files);
		Task DecompressAsync([NotNull] string sourcePath, [NotNull] string destinationPath, string password, CancellationToken token, params string[] files);
		Task DecompressAsync([NotNull] string sourcePath, [NotNull] string destinationPath, string password, [NotNull] IEnumerable<string> files, CancellationToken token = default(CancellationToken));
	}
}