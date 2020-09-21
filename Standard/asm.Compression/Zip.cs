using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using asm.Exceptions;
using asm.Exceptions.Compression;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Compression
{
	//https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.zipfile.createfromdirectory?view=netframework-4.7.2
	public sealed class Zip : CompressorBase
	{
		/// <inheritdoc />
		public Zip() 
		{
		}

		/// <inheritdoc />
		protected override void CompressInternal(Stream source, Stream target)
		{
			if (source.CanSeek) source.Position = 0;

			string entryName = source.GetFileName() ?? "_";
			ZipArchive archive = null;
			Stream entryStream = null;

			try
			{
				archive = new ZipArchive(target, CompressionMode.ToSystemCompressionMode(), true);
				ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.ToSystemCompressionLevel());
				entry.LastWriteTime = DateTimeOffset.Now;
				entryStream = entry.Open();
				source.CopyTo(entryStream);
				entryStream.Flush();
				target.Flush();
			}
			finally
			{
				ObjectHelper.Dispose(ref entryStream);
				ObjectHelper.Dispose(ref archive);
			}
		}

		/// <inheritdoc />
		protected override async Task CompressInternalAsync(Stream source, Stream target, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			if (source.CanSeek) source.Position = 0;

			string entryName = source.GetFileName() ?? "_";
			ZipArchive archive = null;
			Stream entryStream = null;

			try
			{
				archive = new ZipArchive(target, CompressionMode.ToSystemCompressionMode(), true);
				ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.ToSystemCompressionLevel());
				entry.LastWriteTime = DateTimeOffset.Now;
				entryStream = entry.Open();
				await source.CopyToAsync(entryStream, Constants.BUFFER_16KB, token);
				await entryStream.FlushAsync(token);
				await target.FlushAsync(token);
			}
			finally
			{
				ObjectHelper.Dispose(ref entryStream);
				ObjectHelper.Dispose(ref archive);
			}
		}

		protected override void CompressInternal(string sourcePath, string destinationPath)
		{
			sourcePath = PathHelper.Trim(sourcePath) ?? throw new ArgumentNullException(nameof(sourcePath));
			destinationPath = PathHelper.Trim(destinationPath) ?? throw new ArgumentNullException(nameof(destinationPath));
			if (!DirectoryHelper.Ensure(destinationPath)) throw new DirectoryNotFoundException();

			string fileName;
			string newPath;

			if (File.Exists(sourcePath))
			{
				// compressing one file
				fileName = Path.GetFileNameWithoutExtension(sourcePath);
				newPath = Path.Combine(destinationPath, fileName + ".zip");
				ZipArchive zip = null;

				try
				{
					zip = ZipFile.Open(newPath, CompressionMode.ToSystemCompressionMode(), Encoding.UTF8);
					zip.CreateEntryFromFile(sourcePath, fileName, CompressionLevel.ToSystemCompressionLevel());
				}
				finally
				{
					ObjectHelper.Dispose(ref zip);
				}

				return;
			}

			if (!Directory.Exists(sourcePath)) throw new NotFoundException("File or directory not found.");
			fileName = Path.GetDirectoryName(sourcePath);
			newPath = Path.Combine(destinationPath, fileName + ".zip");
			ZipFile.CreateFromDirectory(sourcePath, newPath, CompressionLevel.ToSystemCompressionLevel(), PreserveDirectoryRoot, Encoding.UTF8);
		}

		/// <inheritdoc />
		[NotNull]
		protected override Task CompressInternalAsync(string sourcePath, string destinationPath, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			sourcePath = PathHelper.Trim(sourcePath) ?? throw new ArgumentNullException(nameof(sourcePath));
			destinationPath = PathHelper.Trim(destinationPath) ?? throw new ArgumentNullException(nameof(destinationPath));
			if (!DirectoryHelper.Ensure(destinationPath)) throw new DirectoryNotFoundException();
			token.ThrowIfCancellationRequested();

			string fileName;
			string newPath;

			if (File.Exists(sourcePath))
			{
				// compressing one file
				fileName = Path.GetFileNameWithoutExtension(sourcePath);
				newPath = Path.Combine(destinationPath, fileName + ".zip");
				ZipArchive zip = null;

				try
				{
					zip = ZipFile.Open(newPath, CompressionMode.ToSystemCompressionMode(), Encoding.UTF8);
					token.ThrowIfCancellationRequested();
					zip.CreateEntryFromFile(sourcePath, fileName, CompressionLevel.ToSystemCompressionLevel());
					token.ThrowIfCancellationRequested();
				}
				finally
				{
					ObjectHelper.Dispose(ref zip);
				}

				return Task.CompletedTask;
			}

			if (!Directory.Exists(sourcePath)) throw new NotFoundException("File or directory not found.");
			fileName = Path.GetDirectoryName(sourcePath);
			newPath = Path.Combine(destinationPath, fileName + ".zip");
			token.ThrowIfCancellationRequested();
			ZipFile.CreateFromDirectory(sourcePath, newPath, CompressionLevel.ToSystemCompressionLevel(), PreserveDirectoryRoot, Encoding.UTF8);
			token.ThrowIfCancellationRequested();
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		protected override void DecompressInternal(Stream source, Stream target)
		{
			if (source.CanSeek) source.Position = 0;

			ZipArchive archive = null;
			Stream entryStream = null;

			try
			{
				archive = new ZipArchive(target, ZipArchiveMode.Read, true);

				if (archive.Entries.Count > 0)
				{
					if (archive.Entries.Count > 1) throw new ExtractionFailedException("Source contains more than one file entry.");
					ZipArchiveEntry entry = archive.Entries[0];
					entryStream = entry.Open();
					entryStream.CopyTo(target);
					target.Flush();
				}
			}
			finally
			{
				ObjectHelper.Dispose(ref entryStream);
				ObjectHelper.Dispose(ref archive);
			}
		}

		/// <inheritdoc />
		protected override async Task DecompressInternalAsync(Stream source, Stream target, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			if (source.CanSeek) source.Position = 0;

			ZipArchive archive = null;
			Stream entryStream = null;

			try
			{
				archive = new ZipArchive(target, ZipArchiveMode.Read, true);

				if (archive.Entries.Count > 0)
				{
					if (archive.Entries.Count > 1) throw new ExtractionFailedException("Source contains more than one file entry.");
					ZipArchiveEntry entry = archive.Entries[0];
					entryStream = entry.Open();
					await entryStream.CopyToAsync(target, Constants.BUFFER_16KB, token);
					await target.FlushAsync(token);
				}
			}
			finally
			{
				ObjectHelper.Dispose(ref entryStream);
				ObjectHelper.Dispose(ref archive);
			}
		}

		protected override void DecompressInternal(string sourcePath, string destinationPath)
		{
			sourcePath = PathHelper.Trim(sourcePath) ?? throw new ArgumentNullException(nameof(sourcePath));
			destinationPath = PathHelper.Trim(destinationPath) ?? throw new ArgumentNullException(nameof(destinationPath));
			if (!DirectoryHelper.Ensure(destinationPath)) throw new DirectoryNotFoundException();
			ZipFile.ExtractToDirectory(sourcePath, destinationPath, Encoding.UTF8);
		}

		/// <inheritdoc />
		[NotNull]
		protected override Task DecompressInternalAsync(string sourcePath, string destinationPath, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			sourcePath = PathHelper.Trim(sourcePath) ?? throw new ArgumentNullException(nameof(sourcePath));
			destinationPath = PathHelper.Trim(destinationPath) ?? throw new ArgumentNullException(nameof(destinationPath));
			if (!DirectoryHelper.Ensure(destinationPath)) throw new DirectoryNotFoundException();
			token.ThrowIfCancellationRequested();
			ZipFile.ExtractToDirectory(sourcePath, destinationPath, Encoding.UTF8);
			token.ThrowIfCancellationRequested();
			return Task.CompletedTask;
		}
	}
}