using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace essentialMix.Compression;

public abstract class CompressorExBase : CompressorBase, ICompressorEx
{
	/// <inheritdoc />
	protected CompressorExBase()
	{
	}

	public bool IncludeEmptyDirectories { get; set; } = true;

	public bool PreserveDirectoryStructure { get; set; } = true;

	/// <inheritdoc />
	protected override void CompressInternal(Stream source, Stream target) { CompressInternal(source, target, null); }

	/// <inheritdoc />
	public void Compress(Stream source, Stream target, string password)
	{
		ThrowIfDisposed();
		CompressInternal(source, target, password);
	}

	protected abstract void CompressInternal([NotNull] Stream source, [NotNull] Stream target, string password);

	/// <inheritdoc />
	protected override Task CompressInternalAsync(Stream source, Stream target, CancellationToken token = default(CancellationToken)) { return CompressInternalAsync(source, target, null, token); }

	/// <inheritdoc />
	public Task CompressAsync(Stream source, Stream target, string password, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		return CompressInternalAsync(source, target, password, token);
	}

	protected abstract Task CompressInternalAsync([NotNull] Stream source, [NotNull] Stream target, string password, CancellationToken token = default(CancellationToken));

	/// <inheritdoc />
	protected override void CompressInternal(string sourcePath, string destinationPath) { CompressInternal(sourcePath, destinationPath, null, Array.Empty<string>()); }

	protected override Task CompressInternalAsync(string sourcePath, string destinationPath, CancellationToken token = default(CancellationToken)) { return CompressInternalAsync(sourcePath, destinationPath, null, Array.Empty<string>(), token); }

	/// <inheritdoc />
	public void Compress(string sourcePath, string destinationPath, string password, params string[] files)
	{
		ThrowIfDisposed();
		CompressInternal(sourcePath, destinationPath, password, files ?? Array.Empty<string>());
	}

	/// <inheritdoc />
	public void Compress(string sourcePath, string destinationPath, string password, IEnumerable<string> files)
	{
		ThrowIfDisposed();
		CompressInternal(sourcePath, destinationPath, password, files);
	}

	protected abstract void CompressInternal([NotNull] string sourcePath, [NotNull] string destinationPath, string password, [NotNull] IEnumerable<string> files);

	/// <inheritdoc />
	public Task CompressAsync(string sourcePath, string destinationPath, string password, CancellationToken token, params string[] files)
	{
		ThrowIfDisposed();
		return CompressInternalAsync(sourcePath, destinationPath, password, files ?? Array.Empty<string>(), token);
	}

	/// <inheritdoc />
	public Task CompressAsync(string sourcePath, string destinationPath, string password, IEnumerable<string> files, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		return CompressInternalAsync(sourcePath, destinationPath, password, files, token);
	}

	protected abstract Task CompressInternalAsync(string sourcePath, string destinationPath, string password, IEnumerable<string> files, CancellationToken token = default(CancellationToken));

	/// <inheritdoc />
	protected override void DecompressInternal(Stream source, Stream target) { DecompressInternal(source, target, null); }

	/// <inheritdoc />
	public void Decompress(Stream source, Stream target, string password)
	{
		ThrowIfDisposed();
		DecompressInternal(source, target, password);
	}

	protected abstract void DecompressInternal([NotNull] Stream source, [NotNull] Stream target, string password);

	/// <inheritdoc />
	protected override Task DecompressInternalAsync(Stream source, Stream target, CancellationToken token = default(CancellationToken)) { return DecompressInternalAsync(source, target, null, token); }

	/// <inheritdoc />
	public Task DecompressAsync(Stream source, Stream target, string password, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		return DecompressInternalAsync(source, target, password, token);
	}

	protected abstract Task DecompressInternalAsync([NotNull] Stream source, [NotNull] Stream target, string password, CancellationToken token = default(CancellationToken));

	/// <inheritdoc />
	protected override void DecompressInternal(string sourcePath, string destinationPath) { DecompressInternal(sourcePath, destinationPath, null, Array.Empty<string>()); }

	protected override Task DecompressInternalAsync(string sourcePath, string destinationPath, CancellationToken token = default(CancellationToken)) { return DecompressInternalAsync(sourcePath, destinationPath, null, Array.Empty<string>(), token); }

	/// <inheritdoc />
	public void Decompress(string sourcePath, string destinationPath, string password, params string[] files)
	{
		ThrowIfDisposed();
		DecompressInternal(sourcePath, destinationPath, password, files ?? Array.Empty<string>());
	}

	/// <inheritdoc />
	public void Decompress(string sourcePath, string destinationPath, string password, IEnumerable<string> files)
	{
		ThrowIfDisposed();
		DecompressInternal(sourcePath, destinationPath, password, files);
	}

	protected abstract void DecompressInternal([NotNull] string sourcePath, [NotNull] string destinationPath, string password, [NotNull] IEnumerable<string> files);

	/// <inheritdoc />
	public Task DecompressAsync(string sourcePath, string destinationPath, string password, CancellationToken token, params string[] files)
	{
		ThrowIfDisposed();
		return DecompressInternalAsync(sourcePath, destinationPath, password, files ?? Array.Empty<string>(), token);
	}

	/// <inheritdoc />
	public Task DecompressAsync(string sourcePath, string destinationPath, string password, IEnumerable<string> files, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		return DecompressInternalAsync(sourcePath, destinationPath, password, files, token);
	}

	protected abstract Task DecompressInternalAsync(string sourcePath, string destinationPath, string password, IEnumerable<string> files, CancellationToken token = default(CancellationToken));
}