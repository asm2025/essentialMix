using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;

namespace essentialMix.Threading
{
	[HostProtection(Synchronization=true, ExternalThreading=true)]
	[ComVisible(true)]	
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

		public bool WaitOne(CancellationToken token) { return WaitOne(TimeSpanHelper.INFINITE, false, token); }
		public bool WaitOne(TimeSpan timeout, CancellationToken token) { return WaitOne(timeout.TotalIntMilliseconds(), false, token); }
		public bool WaitOne(TimeSpan timeout, bool exitContext, CancellationToken token) { return WaitOne(timeout.TotalIntMilliseconds(), exitContext, token); }
		public bool WaitOne(int millisecondsTimeout, CancellationToken token) { return WaitOne(millisecondsTimeout, false, token); }
		public bool WaitOne(int millisecondsTimeout, bool exitContext, CancellationToken token)
		{
			ThrowIfDisposed();
			return !token.IsCancellationRequested && this.WaitOne(millisecondsTimeout, true, exitContext, token);
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
