using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using asm.Helpers;
using asm.Patterns.Object;
using JetBrains.Annotations;

namespace asm.Compression
{
	public abstract class CompressorCore : Disposable, ICompressorCore
	{
		/// <inheritdoc />
		protected CompressorCore() 
		{
		}

		/// <inheritdoc />
		public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Ultra;
		
		/// <inheritdoc />
		public CompressionMode CompressionMode { get; set; } = CompressionMode.Create;

		/// <inheritdoc />
		[NotNull]
		public byte[] Compress(byte[] value)
		{
			ThrowIfDisposed();
			return CompressInternal(value, 0, -1);
		}

		/// <inheritdoc />
		[NotNull]
		public byte[] Compress(byte[] value, int startIndex)
		{
			ThrowIfDisposed();
			return CompressInternal(value, startIndex, -1);
		}

		/// <inheritdoc />
		[NotNull]
		public byte[] Compress(byte[] value, int startIndex, int count)
		{
			ThrowIfDisposed();
			return CompressInternal(value, startIndex, count);
		}

		[NotNull]
		protected virtual byte[] CompressInternal([NotNull] byte[] value, int startIndex, int count)
		{
			value = ArrayHelper.ValidateAndGetRange(value, ref startIndex, ref count);
			if (value.Length == 0 || count == 0) return Array.Empty<byte>();

			MemoryStream source = null, target = null;

			try
			{
				source = new MemoryStream(value);
				target = new MemoryStream();
				Compress(source, target);
				return target.ToArray();
			}
			finally
			{
				ObjectHelper.Dispose(ref source);
				ObjectHelper.Dispose(ref target);
			}
		}

		/// <inheritdoc />
		public void Compress(Stream source, Stream target)
		{
			ThrowIfDisposed();
			CompressInternal(source, target);
		}

		protected abstract void CompressInternal([NotNull] Stream source, [NotNull] Stream target);

		/// <inheritdoc />
		public Task CompressAsync(Stream source, Stream target, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			return CompressInternalAsync(source, target, token);
		}

		protected abstract Task CompressInternalAsync([NotNull] Stream source, [NotNull] Stream target, CancellationToken token = default(CancellationToken));

		/// <inheritdoc />
		public void Compress(string sourcePath, string destinationPath)
		{
			ThrowIfDisposed();
			CompressInternal(sourcePath, destinationPath);
		}

		protected abstract void CompressInternal([NotNull] string sourcePath, [NotNull] string destinationPath);

		/// <inheritdoc />
		public Task CompressAsync(string sourcePath, string destinationPath, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			return CompressInternalAsync(sourcePath, destinationPath, token);
		}

		protected abstract Task CompressInternalAsync([NotNull] string sourcePath, [NotNull] string destinationPath, CancellationToken token = default(CancellationToken));

		/// <inheritdoc />
		[NotNull]
		public byte[] Decompress(byte[] value)
		{
			ThrowIfDisposed();
			return DecompressInternal(value, 0, -1);
		}

		/// <inheritdoc />
		[NotNull]
		public byte[] Decompress(byte[] value, int startIndex)
		{
			ThrowIfDisposed();
			return DecompressInternal(value, startIndex, -1);
		}

		/// <inheritdoc />
		[NotNull]
		public byte[] Decompress(byte[] value, int startIndex, int count)
		{
			ThrowIfDisposed();
			return DecompressInternal(value, startIndex, count);
		}

		[NotNull]
		protected virtual byte[] DecompressInternal([NotNull] byte[] value, int startIndex, int count)
		{
			value = ArrayHelper.ValidateAndGetRange(value, ref startIndex, ref count);
			if (value.Length == 0 || count == 0) return Array.Empty<byte>();

			MemoryStream source = null, target = null;

			try
			{
				source = new MemoryStream(value);
				target = new MemoryStream();
				Decompress(source, target);
				return target.ToArray();
			}
			finally
			{
				ObjectHelper.Dispose(ref source);
				ObjectHelper.Dispose(ref target);
			}
		}

		/// <inheritdoc />
		public void Decompress(Stream source, Stream target)
		{
			ThrowIfDisposed();
			DecompressInternal(source, target);
		}

		protected abstract void DecompressInternal([NotNull] Stream source, [NotNull] Stream target);

		/// <inheritdoc />
		public Task DecompressAsync(Stream source, Stream target, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			return DecompressInternalAsync(source, target, token);
		}

		protected abstract Task DecompressInternalAsync([NotNull] Stream source, [NotNull] Stream target, CancellationToken token = default(CancellationToken));

		/// <inheritdoc />
		public void Decompress(string sourcePath, string destinationPath)
		{
			ThrowIfDisposed();
			DecompressInternal(sourcePath, destinationPath);
		}

		protected abstract void DecompressInternal([NotNull] string sourcePath, [NotNull] string destinationPath);

		/// <inheritdoc />
		public Task DecompressAsync(string sourcePath, string destinationPath, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			return DecompressInternalAsync(sourcePath, destinationPath, token);
		}

		protected abstract Task DecompressInternalAsync([NotNull] string sourcePath, [NotNull] string destinationPath, CancellationToken token = default(CancellationToken));
	}
}