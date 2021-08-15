using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading
{
	public abstract class EventWaitHandleBase : EventWaitHandle
	{
		private const int DISPOSAL_NOT_STARTED = 0;
		private const int DISPOSAL_STARTED = 1;
		private const int DISPOSAL_COMPLETE = 2;

		private int _disposeStage;

		/// <inheritdoc />
		protected EventWaitHandleBase(bool initialState, EventResetMode mode)
			: base(initialState, mode)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1063", Justification = "The enforced behavior of CA1063 is not thread-safe or full-featured enough for our purposes here.")]
		~EventWaitHandleBase()
		{
			Dispose(false);
		}

		/// <inheritdoc />
		protected override void Dispose(bool explicitDisposing)
		{
			if (!explicitDisposing)
			{
				base.Dispose(false);
				return;
			}

			if (Interlocked.CompareExchange(ref _disposeStage, DISPOSAL_STARTED, DISPOSAL_NOT_STARTED) != DISPOSAL_NOT_STARTED) return;
			base.Dispose(true);
			MarkAsDisposed();
		}

		public bool IsSet => WaitOne(0);

		[NotNull]
		public Task<bool> WaitOneAsync(CancellationToken token) { return WaitOneAsync(TimeSpanHelper.INFINITE, false, token); }

		[NotNull]
		public Task<bool> WaitOneAsync(TimeSpan timeout, CancellationToken token) { return WaitOneAsync(timeout.TotalIntMilliseconds(), false, token); }

		[NotNull]
		public Task<bool> WaitOneAsync(TimeSpan timeout, bool exitContext, CancellationToken token) { return WaitOneAsync(timeout.TotalIntMilliseconds(), exitContext, token); }

		[NotNull]
		public Task<bool> WaitOneAsync(int millisecondsTimeout, CancellationToken token) { return WaitOneAsync(millisecondsTimeout, false, token); }
		
		[NotNull]
		public Task<bool> WaitOneAsync(int millisecondsTimeout, bool exitContext, CancellationToken token)
		{
			ThrowIfDisposed();
			return token.IsCancellationRequested
						? Task.FromCanceled<bool>(token)
						: Task.Run(() => this.WaitOne(millisecondsTimeout, true, exitContext, token), token);
		}

		protected void ThrowIfDisposed()
		{
			if (Interlocked.CompareExchange(ref _disposeStage, DISPOSAL_COMPLETE, DISPOSAL_COMPLETE) != DISPOSAL_COMPLETE) return;
			throw new ObjectDisposedException(GetType().FullName);
		}

		[SuppressMessage("Microsoft.Usage", "CA1816", Justification = "This is a helper method for IDisposable.Dispose.")]
		private void MarkAsDisposed()
		{
			GC.SuppressFinalize(this);
			Interlocked.Exchange(ref _disposeStage, DISPOSAL_COMPLETE);
		}
	}
}
