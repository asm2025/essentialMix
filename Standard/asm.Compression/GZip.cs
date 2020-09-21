using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using SystemCompressionMode = System.IO.Compression.CompressionMode;

namespace asm.Compression
{
	public sealed class GZip : CompressorCore
	{
		/// <inheritdoc />
		public GZip()
		{
		}

		/// <inheritdoc />
		protected override void CompressInternal(Stream source, Stream target)
		{
			if (source.CanSeek) source.Position = 0;

			GZipStream gZipStream = null;

			try
			{
				gZipStream = new GZipStream(target, CompressionLevel.ToSystemCompressionLevel(), true);
				source.CopyTo(gZipStream);
				gZipStream.Flush();
				target.Flush();
			}
			finally
			{
				ObjectHelper.Dispose(ref gZipStream);
			}
		}

		/// <inheritdoc />
		protected override async Task CompressInternalAsync(Stream source, Stream target, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			if (source.CanSeek) source.Position = 0;

			GZipStream gZipStream = null;

			try
			{
				gZipStream = new GZipStream(target, CompressionLevel.ToSystemCompressionLevel(), true);
				await source.CopyToAsync(gZipStream, Constants.BUFFER_16KB, token);
				await gZipStream.FlushAsync(token);
				await target.FlushAsync(token);
			}
			finally
			{
				ObjectHelper.Dispose(ref gZipStream);
			}
		}

		/// <inheritdoc />
		protected override void CompressInternal(string sourcePath, string destinationPath)
		{
			sourcePath = PathHelper.Trim(sourcePath) ?? throw new ArgumentNullException(nameof(sourcePath));
			destinationPath = PathHelper.Trim(destinationPath) ?? throw new ArgumentNullException(nameof(destinationPath));
			// GZip supports files only
			if (!File.Exists(sourcePath)) throw new FileNotFoundException();
			if (!DirectoryHelper.Ensure(destinationPath)) throw new DirectoryNotFoundException();
			
			// keep the file extension for later extraction
			string fileName = Path.GetFileName(sourcePath);
			string newPath = Path.Combine(destinationPath, fileName + ".gz");
			FileStream source = null, target = null;

			try
			{
				source = File.OpenRead(sourcePath);
				target = File.Create(newPath);
				CompressInternal(source, target);
			}
			finally
			{
				ObjectHelper.Dispose(ref target);
				ObjectHelper.Dispose(ref source);
			}
		}

		/// <inheritdoc />
		protected override async Task CompressInternalAsync(string sourcePath, string destinationPath, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			sourcePath = PathHelper.Trim(sourcePath) ?? throw new ArgumentNullException(nameof(sourcePath));
			destinationPath = PathHelper.Trim(destinationPath) ?? throw new ArgumentNullException(nameof(destinationPath));
			// GZip supports files only
			if (!File.Exists(sourcePath)) throw new FileNotFoundException();
			if (!DirectoryHelper.Ensure(destinationPath)) throw new DirectoryNotFoundException();
			token.ThrowIfCancellationRequested();
			
			// keep the file extension for later extraction
			string fileName = Path.GetFileName(sourcePath);
			string newPath = Path.Combine(destinationPath, fileName + ".gz");
			FileStream source = null, target = null;

			try
			{
				source = File.OpenRead(sourcePath);
				target = File.Create(newPath);
				await CompressInternalAsync(source, target, token);
			}
			finally
			{
				ObjectHelper.Dispose(ref target);
				ObjectHelper.Dispose(ref source);
			}
		}

		/// <inheritdoc />
		protected override void DecompressInternal(Stream source, Stream target)
		{
			if (source.CanSeek) source.Position = 0;

			GZipStream gZipStream = null;

			try
			{
				gZipStream = new GZipStream(target, SystemCompressionMode.Decompress, true);
				source.CopyTo(gZipStream);
				gZipStream.Flush();
				target.Flush();
			}
			finally
			{
				ObjectHelper.Dispose(ref gZipStream);
			}
		}

		/// <inheritdoc />
		protected override async Task DecompressInternalAsync(Stream source, Stream target, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			if (source.CanSeek) source.Position = 0;

			GZipStream gZipStream = null;

			try
			{
				gZipStream = new GZipStream(target, SystemCompressionMode.Decompress, true);
				await source.CopyToAsync(gZipStream, Constants.BUFFER_16KB, token);
				await gZipStream.FlushAsync(token);
				await target.FlushAsync(token);
			}
			finally
			{
				ObjectHelper.Dispose(ref gZipStream);
			}
		}

		/// <inheritdoc />
		protected override void DecompressInternal(string sourcePath, string destinationPath)
		{
			sourcePath = PathHelper.Trim(sourcePath) ?? throw new ArgumentNullException(nameof(sourcePath));
			destinationPath = PathHelper.Trim(destinationPath) ?? throw new ArgumentNullException(nameof(destinationPath));
			if (!DirectoryHelper.Ensure(destinationPath)) throw new DirectoryNotFoundException();

			// If file extension was kept from compression step, after removing the
			// gz extension the original extension should be there in place.
			string fileName = Path.GetFileNameWithoutExtension(sourcePath);
			string newPath = Path.Combine(destinationPath, fileName);
			FileStream source = null, target = null;

			try
			{
				source = File.OpenRead(sourcePath);
				target = File.Create(newPath);
				Decompress(source, target);
			}
			finally
			{
				ObjectHelper.Dispose(ref target);
				ObjectHelper.Dispose(ref source);
			}
		}

		/// <inheritdoc />
		protected override async Task DecompressInternalAsync(string sourcePath, string destinationPath, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			sourcePath = PathHelper.Trim(sourcePath) ?? throw new ArgumentNullException(nameof(sourcePath));
			destinationPath = PathHelper.Trim(destinationPath) ?? throw new ArgumentNullException(nameof(destinationPath));
			if (!DirectoryHelper.Ensure(destinationPath)) throw new DirectoryNotFoundException();
			token.ThrowIfCancellationRequested();

			// If file extension was kept from compression step, after removing the
			// gz extension the original extension should be there in place.
			string fileName = Path.GetFileNameWithoutExtension(sourcePath);
			string newPath = Path.Combine(destinationPath, fileName);
			FileStream source = null, target = null;

			try
			{
				source = File.OpenRead(sourcePath);
				target = File.Create(newPath);
				await DecompressInternalAsync(source, target, token);
			}
			finally
			{
				ObjectHelper.Dispose(ref target);
				ObjectHelper.Dispose(ref source);
			}
		}
	}
}