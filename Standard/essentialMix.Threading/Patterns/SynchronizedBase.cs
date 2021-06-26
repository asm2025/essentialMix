using System;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns
{
	/// <summary>
	/// Encapsulate a synchronized multi-threaded object.
	/// </summary>
	/// <example>
	/// Typical use case:
	/// <code>
	/// void MySynchronizedMethod()
	/// {
	/// 	if (IsBusy) return;
	/// 	
	/// 	bool isSynchronized = IsSynchronized;
	/// 	
	/// 	if (isSynchronized)
	/// 	{
	/// 		if (Token.CanBeCanceled)
	/// 		{
	/// 			bool lockTaken = false;
	/// 	
	/// 			while (!lockTaken && !Token.IsCancellationRequested) 
	/// 				lockTaken = Monitor.TryEnter(SyncRoot, TimeSpanHelper.MINIMUM_SCHEDULE);
	/// 	
	/// 			Token.ThrowIfCancellationRequested();
	/// 			if (!lockTaken) return;
	/// 		}
	/// 		else
	/// 		{
	/// 			Monitor.Enter(SyncRoot);
	/// 		}
	/// 	}
	/// 
	/// 	try
	/// 	{
	///			...
	/// 	}
	/// 	finally
	/// 	{
	/// 		Might be used
	/// 		if (!HasPendingWait() [&& other conditions]) ...
	/// 		MUST be called
	/// 		if (isSynchronized) Monitor.Exit(SyncRoot);
	/// 	}
	/// }
	/// 
	/// void MySynchronizedThreadMethod()
	/// {
	/// 	if (Token.CanBeCanceled)
	/// 	{
	/// 		bool lockTaken = false;
	/// 	
	/// 		while (!lockTaken && !Token.IsCancellationRequested) 
	/// 			lockTaken = Monitor.TryEnter(SyncRoot, TimeSpanHelper.MINIMUM_SCHEDULE);
	/// 	
	/// 		Token.ThrowIfCancellationRequested();
	/// 		if (!lockTaken) return;
	/// 	}
	/// 	else
	/// 	{
	/// 		Monitor.Enter(SyncRoot);
	/// 	}
	///
	///		if (IsBusy || !WaitForWorkerToFinish())
	///		{
	///			...
	///			Monitor.Exit(SyncRoot);
	///			return;
	///		}
	///
	///		ReleaseCancellationTokenSource();
	///		ReleaseFinishEvent();
	///		IsBusy = true;
	///		
	/// 	try
	/// 	{
	///			InitializeCancellationTokenSource();
	///			InitializeFinishEvent();
	///
	///			// copy to local variable
	///			CancellationToken token = Token;
	///			...
	///			token.ThrowIfCancellationRequested();
	///			if (!IsBusy) throw new OperationCanceledException();
	///			...
	/// 	}
	///		catch (OperationCanceledException)
	///		{
	///			// ignored
	///		}
	///		catch (TimeoutException)
	///		{
	///			// ignored
	///		}
	/// 	finally
	/// 	{
	///			...
	///			*MUST be called*
	/// 		IsBusy = false;
	///			SignalWorkerFinished();
	/// 		Monitor.Exit(SyncRoot);
	/// 	}
	/// }
	/// 
	/// void MyCancelMethod()
	/// {
	/// 	Cancel();
	/// 	if (!WaitForWorkerToFinish()) return;
	/// 	ReleaseCancellationTokenSource();
	///		...
	/// }
	/// </code>
	/// </example>
	public abstract class SynchronizedBase : Disposable
	{
		private object _syncRoot;
		private ManualResetEventSlim _workerStarted;
		private ManualResetEventSlim _workerDone;
		private CancellationTokenSource _ctx;

		private volatile int _isBusy;

		/// <inheritdoc />
		protected SynchronizedBase()
		{
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ReleaseStartEvent();
				ReleaseFinishEvent();
				ReleaseCancellationTokenSource();
			}

			base.Dispose(disposing);
		}

		public object SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		public bool IsBusy
		{
			get
			{
				int isBusy = _isBusy;
				return isBusy != 0;
			}
			protected set => Interlocked.CompareExchange(ref _isBusy, value ? 1 : 0, _isBusy);
		}
	
		protected CancellationToken Token { get; private set; }

		protected abstract bool IsSynchronized { get; }

		[NotNull]
		protected ManualResetEventSlim InitializeStartEvent()
		{
			if (_workerStarted == null) Interlocked.CompareExchange(ref _workerStarted, new ManualResetEventSlim(), null);
			return _workerStarted;
		}

		protected void ReleaseStartEvent()
		{
			if (_workerStarted == null) return;
			Interlocked.Exchange(ref _workerStarted, null);
		}

		protected bool WaitForWorkerToStart()
		{
			if (_workerStarted is not { IsSet: false }) return true;

			try
			{
				return _workerStarted.Wait(TimeSpanHelper.INFINITE, Token);
			}
			catch (OperationCanceledException)
			{
				return true;
			}
			catch (TimeoutException)
			{
				return false;
			}
		}

		protected void SignalWorkerStarted()
		{
			_workerStarted?.Set();
		}

		[NotNull]
		protected ManualResetEventSlim InitializeFinishEvent()
		{
			if (_workerDone == null) Interlocked.CompareExchange(ref _workerDone, new ManualResetEventSlim(), null);
			return _workerDone;
		}

		protected void ReleaseFinishEvent()
		{
			if (_workerDone == null) return;
			Interlocked.Exchange(ref _workerDone, null);
		}

		protected bool WaitForWorkerToFinish()
		{
			if (_workerDone is not { IsSet: false }) return true;

			try
			{
				return _workerDone.Wait(TimeSpanHelper.INFINITE, Token);
			}
			catch (OperationCanceledException)
			{
				return true;
			}
			catch (TimeoutException)
			{
				return false;
			}
		}

		protected void SignalWorkerFinished()
		{
			_workerDone?.Set();
		}

		protected bool HasPendingWait()
		{
			return IsBusy || _ctx != null || _workerStarted != null || _workerDone != null;
		}

		[NotNull]
		protected CancellationTokenSource InitializeCancellationTokenSource()
		{
			if (_ctx != null) return _ctx;
			Interlocked.CompareExchange(ref _ctx, new CancellationTokenSource(), null);
			Token = _ctx.Token;
			return _ctx;
		}

		protected void ReleaseCancellationTokenSource()
		{
			if (_ctx == null) return;
			Interlocked.Exchange(ref _ctx, null);
			Token = CancellationToken.None;
		}

		protected virtual void Cancel()
		{
			_ctx.CancelIfNotDisposed();
		}
	}
}
