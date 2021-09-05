using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Exceptions;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.IO
{
	[DebuggerDisplay("Length = {Length}")]
	[Serializable]
	public class ConsumerStream : Stream
	{
		private LinkedList<byte[]> _linkedList;
		private int _readTimeout;
		private int _writeTimeout;
		private AutoResetEvent _dataAvailable;
		private long _length;

		private CancellationTokenSource _cts;
		private IDisposable _tokenRegistration;

		[NonSerialized]
		private object _syncRoot;
		[NonSerialized]
		private object _syncRead;

		private volatile int _isCompleted;

		/// <inheritdoc />
		public ConsumerStream(CancellationToken token = default(CancellationToken))
		{
			_linkedList = new LinkedList<byte[]>();
			_dataAvailable = new AutoResetEvent();
			InitializeToken(token);
		}

		/// <inheritdoc />
		public ConsumerStream([NotNull] IEnumerable<byte[]> collection, CancellationToken token = default(CancellationToken))
			: this(token)
		{
			foreach (byte[] item in collection)
			{
				if (item == null) throw new ArgumentHasNullValueException(nameof(collection));
				if (item.Length == 0) continue;
				_linkedList.AddLast(item);
			}

			if (_linkedList.Count > 0) _dataAvailable.Set();
		}

		~ConsumerStream()
		{
			Dispose(false);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stop();
				ObjectHelper.Dispose(ref _dataAvailable);
				ReleaseToken();
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

		/// <inheritdoc />
		public override bool CanRead => true;

		/// <inheritdoc />
		public override bool CanWrite => true;

		/// <inheritdoc />
		public override bool CanSeek => false;

		/// <inheritdoc />
		public override long Length => _length;

		/// <inheritdoc />
		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public object SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		protected object SyncRead
		{
			get
			{
				if (_syncRead == null) Interlocked.CompareExchange<object>(ref _syncRead, new object(), null);
				return _syncRead;
			}
		}

		public CancellationToken Token { get; private set; }

		public bool IsCompleted
		{
			get
			{
				// ensure we have the latest value
				Thread.MemoryBarrier();
				return _isCompleted != 0;
			}
			protected set => Interlocked.CompareExchange(ref _isCompleted, value
																				? 1
																				: 0, _isCompleted);
		}

		public bool IsEmpty => _linkedList.Count == 0;

		protected bool IsDisposed { get; private set; }

		/// <inheritdoc />
		public override int Read(byte[] buffer, int offset, int count)
		{
			return DoRead(ref buffer, offset, count, Token);
		}

		/// <inheritdoc />
		[NotNull]
		public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			IDisposable tokenReg = null;
			int read;

			try
			{
				if (cancellationToken.CanBeCanceled) tokenReg = cancellationToken.Register(Cancel);
				CancellationToken token = Token.CanBeCanceled
											? Token
											: cancellationToken;
				read = await Task.Run(() => DoRead(ref buffer, offset, count, token), token);
			}
			finally
			{
				ObjectHelper.Dispose(ref tokenReg);
			}

			return read;
		}

		/// <inheritdoc />
		public override int ReadByte()
		{
			byte[] buffer = new byte[1];
			return DoRead(ref buffer, 0, 1, Token);
		}

		public byte[] ReadSample()
		{
			byte[] buffer = null;
			DoRead(ref buffer, 0, 0, Token);
			return buffer;
		}

		[NotNull]
		public async Task<byte[]> ReadSampleAsync(CancellationToken cancellationToken)
		{
			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			IDisposable tokenReg = null;
			byte[] buffer = null;

			try
			{
				if (cancellationToken.CanBeCanceled) tokenReg = cancellationToken.Register(Cancel);
				CancellationToken token = Token.CanBeCanceled
											? Token
											: cancellationToken;
				await Task.Run(() => DoRead(ref buffer, 0, 0, token), token);
			}
			finally
			{
				ObjectHelper.Dispose(ref tokenReg);
			}

			return buffer;
		}

		protected int DoRead(ref byte[] buffer, int offset, int count, CancellationToken token)
		{
			ThrowIfDisposed();
			buffer?.Length.ValidateRange(offset, count);
			if (token.IsCancellationRequested || buffer != null && count == 0) return 0;

			if (token.CanBeCanceled)
			{
				bool lockTaken = false;

				if (!SpinWait.SpinUntil(() =>
				{
					lockTaken = Monitor.TryEnter(SyncRead, TimeSpanHelper.MINIMUM);
					return lockTaken || token.IsCancellationRequested;
				}, ReadTimeout))
				{
					throw new TimeoutException();
				}

				if (token.IsCancellationRequested)
				{
					if (lockTaken) Monitor.Exit(SyncRead);
					throw new OperationCanceledException();
				}
			}
			else
			{
				if (!Monitor.TryEnter(SyncRead, ReadTimeout)) 
					throw new TimeoutException();
			}

			int read = 0;
			if (buffer == null) count = int.MaxValue;

			try
			{
				while (!IsDisposed && !token.IsCancellationRequested && !IsCompleted && count > 0)
				{
					if (_linkedList.Count == 0 && !_dataAvailable.WaitOne(TimeSpanHelper.FAST, token)) continue;
					if (IsDisposed || token.IsCancellationRequested || IsCompleted || _linkedList.Count == 0) continue;

					lock(SyncRoot)
					{
						if (_linkedList.Count == 0) continue;

						int bytesToCopy;

						if (buffer == null)
						{
							buffer = _linkedList.First.Value;
							bytesToCopy = buffer.Length;
							_linkedList.RemoveFirst();
							count = 0;
						}
						else
						{
							byte[] bytes = _linkedList.First.Value;
							bytesToCopy = Math.Min(bytes.Length, count);
							Buffer.BlockCopy(bytes, 0, buffer, offset, bytesToCopy);
							_linkedList.RemoveFirst();
							count -= bytesToCopy;

							if (bytes.Length > count)
							{
								Array.Resize(ref bytes, bytes.Length - count);
								_linkedList.AddFirst(bytes);
							}
						}

						_length -= bytesToCopy;
						offset += bytesToCopy;
						read += bytesToCopy;
					}
				}
				
				if (IsDisposed || token.IsCancellationRequested) return read;

				while (!IsDisposed && !token.IsCancellationRequested && count > 0 && _linkedList.Count > 0)
				{
					lock(SyncRoot)
					{
						if (_linkedList.Count == 0) continue;

						int bytesToCopy;

						if (buffer == null)
						{
							buffer = _linkedList.First.Value;
							bytesToCopy = buffer.Length;
							_linkedList.RemoveFirst();
							count = 0;
						}
						else
						{
							byte[] bytes = _linkedList.First.Value;
							bytesToCopy = Math.Min(bytes.Length, count);
							Buffer.BlockCopy(bytes, 0, buffer, offset, bytesToCopy);
							_linkedList.RemoveFirst();
							count -= bytesToCopy;

							if (bytes.Length > count)
							{
								Array.Resize(ref bytes, bytes.Length - count);
								_linkedList.AddFirst(bytes);
							}
						}

						_length -= bytesToCopy;
						offset += bytesToCopy;
						read += bytesToCopy;
					}
				}
			}
			catch (ObjectDisposedException) { }
			catch (OperationCanceledException) { }
			finally
			{
				Monitor.Exit(SyncRead);
			}

			return read;
		}

		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count)
		{
			ThrowIfDisposed();
			buffer.Length.ValidateRange(offset, count);
			if (Token.IsCancellationRequested || IsCompleted || count == 0) return;

			lock(SyncRoot)
			{
				byte[] bytes = new byte[count];
				Buffer.BlockCopy(buffer, offset, bytes, 0, count);
				_linkedList.AddLast(bytes);
				_length += bytes.Length;
				_dataAvailable.Set();
			}
		}

		/// <inheritdoc />
		public override void Flush() { }

		/// <inheritdoc />
		public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }

		/// <inheritdoc />
		public override void SetLength(long value) { throw new NotSupportedException(); }

		public void InitializeToken(CancellationToken token)
		{
			ThrowIfDisposed();
			if (_cts != null) Interlocked.Exchange(ref _cts, null);
			ObjectHelper.Dispose(ref _tokenRegistration);
			Interlocked.CompareExchange(ref _cts, new CancellationTokenSource(), null);
			if (token.CanBeCanceled) _tokenRegistration = token.Register(() => _cts.CancelIfNotDisposed(), false);
			Token = _cts.Token;
		}

		protected void ReleaseToken()
		{
			if (_cts == null) return;
			Interlocked.Exchange(ref _cts, null);
			ObjectHelper.Dispose(ref _tokenRegistration);
			Token = CancellationToken.None;
		}

		protected void Cancel()
		{
			_cts.CancelIfNotDisposed();
		}

		public void Clear()
		{
			ThrowIfDisposed();

			lock(SyncRoot)
			{
				_linkedList.Clear();
				_dataAvailable.Set();
			}
		}

		public void Complete()
		{
			ThrowIfDisposed();
			IsCompleted = true;
		}

		public void Stop()
		{
			Complete();
			Cancel();
			Clear();
		}

		protected void ThrowIfDisposed()
		{
			if (!IsDisposed) return;
			throw new ObjectDisposedException(GetType().FullName);
		}
	}
}