using System;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.Object
{
	/// <summary>
	/// Base class for asynchronous disposal. This prevents an object from being disposed while in use, by exposing
	/// a DeferDispose method, which defers disposal while an synchronous or async operation in active.
	/// <para>
	/// based on <see href="https://www.linqpad.net/RichClient/SampleLibraries.aspx">Joe Albahari's Samples for online presentation with SSW (April 2021)</see> for LinqPad
	/// </para>
	/// </summary>
	public class AsyncDisposable : Disposable
	{
		private CancellationTokenSource _cts;
		private CountdownEvent _countdown;

		/// <inheritdoc />
		protected AsyncDisposable() 
		{
			_countdown = new CountdownEvent(1);
			_cts = new CancellationTokenSource();
			Token = _cts.Token;
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;
			Wait();
			ObjectHelper.Dispose(ref _countdown);
			ObjectHelper.Dispose(ref _cts);
		}

		public bool IsDisposeRequested { get; private set; }
		public CancellationToken Token { get; }
		public bool IsMarkedForDisposal => _countdown.IsSet;

		public void Cancel()
		{
			ThrowIfDisposed();
			CancelInternal(false);
		}

		public void Cancel(bool enforce)
		{
			ThrowIfDisposed();
			CancelInternal(enforce);
		}

		public bool Wait()
		{
			ThrowIfDisposed();
			return WaitInternal(TimeSpanHelper.INFINITE);
		}

		public bool Wait(TimeSpan timeout)
		{
			ThrowIfDisposed();
			return WaitInternal(timeout.TotalIntMilliseconds());
		}

		public bool Wait(int millisecondsTimeout)
		{
			ThrowIfDisposed();
			return WaitInternal(millisecondsTimeout);
		}

		[NotNull] 
		public Task<bool> WaitAsync() { return WaitAsync(TimeSpanHelper.INFINITE); }

		[NotNull] 
		public Task<bool> WaitAsync(TimeSpan timeout) { return WaitAsync(timeout.TotalIntMilliseconds()); }

		[NotNull]
		public Task<bool> WaitAsync(int millisecondsTimeout)
		{
			ThrowIfDisposed();
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			return Task.Run(() => WaitInternal(millisecondsTimeout), Token);
		}

		protected virtual bool WaitInternal(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			
			try
			{
				if (millisecondsTimeout > TimeSpanHelper.INFINITE) return _countdown.Wait(millisecondsTimeout, Token);
				_countdown.Wait(Token);
				return !Token.IsCancellationRequested;
			}
			catch (OperationCanceledException)
			{
			}
			catch (TimeoutException)
			{
			}
			catch (AggregateException ag) when (ag.InnerException is OperationCanceledException || ag.InnerException is TimeoutException)
			{
			}

			return false;
		}

		protected virtual void CancelInternal(bool enforce)
		{
			if (!enforce) WaitInternal(TimeSpanHelper.INFINITE);
			_cts.CancelIfNotDisposed();
		}

		/// <summary>
		/// Suspends disposal until the token that it returns is disposed.
		/// If already disposed, throws an ObjectDisposedException.
		/// </summary>
		[NotNull]
		public IDisposable DeferDisposal()
		{
			if (!_countdown.TryAddCount()) throw new ObjectDisposedException (GetType().Name);
			return Create(() => _countdown?.Signal());
		}

		/// <summary>
		/// Suspends disposal until the token that it returns is disposed.
		/// If already disposed, returns null.
		/// </summary>
		public IDisposable TryDeferDisposal()
		{
			return !_countdown.TryAddCount()
						? null
						: Create(() => _countdown?.Signal());
		}

		/// <summary>
		/// Starts disposal. If disposal has been deferred, disposal will not start 
		/// until the deferral tokens have been released.
		/// </summary>
		[NotNull]
		public virtual Task DisposeAsync()
		{
			lock (this)
			{
				if (IsDisposeRequested) return WaitAsync();
				IsDisposeRequested = true;
				_countdown?.Signal();
			}

			return WaitAsync();
		}
	}
}