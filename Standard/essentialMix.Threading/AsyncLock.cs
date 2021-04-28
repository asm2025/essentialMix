using System;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Threading
{
	/// <summary>
	/// Based on Joe Albahari's AsyncLock
	/// </summary>
	public class AsyncLock : Disposable
	{
		private SemaphoreSlim _semaphore;

		public AsyncLock()
			: this(1)
		{
		}

		public AsyncLock(int capacity)
		{
			if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
			_semaphore = new SemaphoreSlim(capacity);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _semaphore);
			base.Dispose(disposing);
		}

		[NotNull]
		public Task<IDisposable> EnterAsync() { return EnterAsync(TimeSpanHelper.INFINITE, CancellationToken.None); }
		[NotNull]
		public Task<IDisposable> EnterAsync(CancellationToken token) { return EnterAsync(TimeSpanHelper.INFINITE, token); }
		[NotNull]
		public Task<IDisposable> EnterAsync(TimeSpan timeout) { return EnterAsync(timeout.TotalIntMilliseconds(), CancellationToken.None); }
		[NotNull]
		public Task<IDisposable> EnterAsync(TimeSpan timeout, CancellationToken token) { return EnterAsync(timeout.TotalIntMilliseconds(), token); }
		[NotNull]
		public Task<IDisposable> EnterAsync(int millisecondsTimeout) { return EnterAsync(millisecondsTimeout, CancellationToken.None); }
		public async Task<IDisposable> EnterAsync(int millisecondsTimeout, CancellationToken token)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			token.ThrowIfCancellationRequested();

			try
			{
				if (millisecondsTimeout < TimeSpanHelper.ZERO) await _semaphore.WaitAsync(token).ConfigureAwait();
				else if (!await _semaphore.WaitAsync(millisecondsTimeout, token).ConfigureAwait()) throw new TimeoutException();
				return Create(() => _semaphore?.Release());
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}
	}
}
