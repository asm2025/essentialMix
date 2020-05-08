using System;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Object;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public abstract class ProducerConsumerQueue : Disposable, IProducerConsumer, IDisposable
	{
		private CancellationTokenSource _cts;

		protected ProducerConsumerQueue(CancellationToken token = default(CancellationToken))
			: this(null, token)
		{
		}

		protected ProducerConsumerQueue(ProducerConsumerQueueOptions options, CancellationToken token = default(CancellationToken))
		{
			if (options == null) options = new ProducerConsumerQueueOptions();
			ThreadsInternal = options.Threads;
			_cts = new CancellationTokenSource();
			if (token.CanBeCanceled) token.Register(() => _cts.CancelIfNotDisposed());
			Token = _cts.Token;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopInternal(!WaitForQueuedItemsOnDispose);
				ObjectHelper.Dispose(ref _cts);
			}

			base.Dispose(disposing);
		}

		public event EventHandler WorkStarted;
		public event EventHandler WorkCompleted;

		public int Count => CountInternal;

		public bool WaitForQueuedItemsOnDispose { get; set; } = true;

		public int Threads => ThreadsInternal;

		public CancellationToken Token { get; }

		public bool IsBusy => IsBusyInternal;

		public bool CompleteMarked { get; protected set; }

		public int SleepAfterEnqueue { get; set; } = TimeSpanHelper.INFINITE;

		protected abstract int CountInternal { get; }

		protected abstract bool IsBusyInternal { get; }

		protected int ThreadsInternal { get; }

		public void Stop()
		{
			ThrowIfDisposed();
			StopInternal(!WaitForQueuedItemsOnDispose);
		}

		public void Stop(bool enforce)
		{
			ThrowIfDisposed();
			StopInternal(enforce);
		}

		public void Enqueue(TaskItem item)
		{
			ThrowIfDisposed();
			if (CompleteMarked) throw new InvalidOperationException("Completion marked.");
			EnqueueInternal(item);
			if (SleepAfterEnqueue > TimeSpanHelper.INFINITE) Thread.Sleep(SleepAfterEnqueue);
		}

		public void Complete()
		{
			ThrowIfDisposed();
			if (CompleteMarked) return;
			CompleteInternal();
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

		[NotNull] public Task<bool> WaitAsync() { return WaitAsync(TimeSpanHelper.INFINITE); }

		[NotNull] public Task<bool> WaitAsync(TimeSpan timeout) { return WaitAsync(timeout.TotalIntMilliseconds()); }

		[NotNull]
		public Task<bool> WaitAsync(int millisecondsTimeout)
		{
			ThrowIfDisposed();
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			return Task.Run(() => WaitInternal(millisecondsTimeout), Token);
		}

		public void Clear()
		{
			ThrowIfDisposed();
			ClearInternal();
		}

		protected abstract void EnqueueInternal(TaskItem item);

		protected abstract void CompleteInternal();

		protected abstract void ClearInternal();

		protected abstract bool WaitInternal(int millisecondsTimeout);

		protected abstract void StopInternal(bool enforce);

		protected virtual void OnWorkStarted(EventArgs args) { WorkStarted?.Invoke(this, args); }

		protected virtual void OnWorkCompleted(EventArgs args) { WorkCompleted?.Invoke(this, args); }

		protected void Cancel()
		{
			_cts.CancelIfNotDisposed();
		}
	}
}