using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.IO;

public class BlockingStream : MemoryStream
{
	private AutoResetEvent _dataAvailable = new AutoResetEvent();
	private int _readTimeout;
	private int _writeTimeout;
	private object _syncRoot;

	/// <inheritdoc />
	public BlockingStream() 
	{
	}

	/// <inheritdoc />
	public BlockingStream(int capacity)
		: base(capacity)
	{
	}

	/// <inheritdoc />
	public BlockingStream([NotNull] byte[] buffer)
		: base(buffer)
	{
	}

	/// <inheritdoc />
	public BlockingStream([NotNull] byte[] buffer, bool writable)
		: base(buffer, writable)
	{
	}

	/// <inheritdoc />
	public BlockingStream([NotNull] byte[] buffer, int index, int count)
		: base(buffer, index, count)
	{
	}

	/// <inheritdoc />
	public BlockingStream([NotNull] byte[] buffer, int index, int count, bool writable)
		: base(buffer, index, count, writable)
	{
	}

	/// <inheritdoc />
	public BlockingStream([NotNull] byte[] buffer, int index, int count, bool writable, bool publiclyVisible)
		: base(buffer, index, count, writable, publiclyVisible)
	{
	}

	~BlockingStream()
	{
		Dispose(false);
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			ObjectHelper.Dispose(ref _dataAvailable);
			IsDisposed = true;
		}
		base.Dispose(disposing);
	}

	/// <inheritdoc />
	public override bool CanTimeout => true;

	/// <inheritdoc />
	public override int ReadTimeout
	{
		get => _readTimeout;
		set
		{
			if (value < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(value));
			_readTimeout = value;
		}
	}

	/// <inheritdoc />
	public override int WriteTimeout
	{
		get => _writeTimeout;
		set
		{
			if (value < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(value));
			_writeTimeout = value;
		}
	}

	protected bool IsDisposed { get; private set; }

	protected object SyncRoot
	{
		get
		{
			if (_syncRoot == null) Interlocked.CompareExchange(ref _syncRoot, new object(), null);
			return _syncRoot;
		}
	}

	/// <inheritdoc />
	public override int Read(byte[] buffer, int offset, int count)
	{
		ThrowIfDisposed();

		if (Length == 0)
		{
			_dataAvailable.WaitOne();
			ThrowIfDisposed();
		}

		lock(SyncRoot)
			return base.Read(buffer, offset, count);
	}

	/// <inheritdoc />
	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		ThrowIfDisposed();
		cancellationToken.ThrowIfCancellationRequested();

		if (Length == 0)
		{
			await _dataAvailable.WaitOneAsync(cancellationToken);
			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();
		}

		return await base.ReadAsync(buffer, offset, count, cancellationToken);
	}

	/// <inheritdoc />
	public override void Write(byte[] buffer, int offset, int count)
	{
		ThrowIfDisposed();

		lock(SyncRoot)
		{
			long position = Position;
			base.Write(buffer, offset, count);
			Position = position;
			_dataAvailable.Set();
		}
	}

	protected void ThrowIfDisposed()
	{
		if (!IsDisposed) return;
		throw new ObjectDisposedException(GetType().FullName);
	}
}