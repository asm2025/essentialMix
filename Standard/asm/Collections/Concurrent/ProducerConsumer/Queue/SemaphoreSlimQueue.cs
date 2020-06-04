using System;
using System.Collections.Generic;
using System.Threading;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer.Queue
{
	public sealed class SemaphoreSlimQueue<T> : ProducerConsumerThreadQueue<T>
	{
		private readonly object _lock = new object();
		private readonly Queue<T> _queue = new Queue<T>();

		private ManualResetEventSlim _manualResetEventSlim;
		private SemaphoreSlim _semaphore;
		private Thread _worker;
		
		private int _running;
		private int _workCompleted;

		public SemaphoreSlimQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_manualResetEventSlim = new ManualResetEventSlim(false);
			
			// the additional + 1 is for the Consume thread
			int threads = Threads + 1;
			_semaphore = new SemaphoreSlim(threads);
			(_worker = new Thread(Consume)
			{
				IsBackground = IsBackground,
				Priority = Priority
			}).Start();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;
			StopInternal(WaitOnDispose);
			ObjectHelper.Dispose(ref _semaphore);
			ObjectHelper.Dispose(ref _manualResetEventSlim);
		}

		public override int Count
		{
			get
			{
				lock (_lock)
				{
					return _queue.Count + _running;
				}
			}
		}

		public override bool IsBusy => Count > 0;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			lock (_lock)
			{
				_queue.Enqueue(item);
				Monitor.Pulse(_lock);
			}
		}

		protected override void CompleteInternal()
		{
			lock (_lock)
			{
				CompleteMarked = true;
				Monitor.PulseAll(_lock);
			}
		}

		protected override void ClearInternal()
		{
			lock(_lock)
			{
				_queue.Clear();
				Monitor.PulseAll(_lock);
			}
		}

		protected override bool WaitInternal(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!IsBusy) return true;

			try
			{
				if (millisecondsTimeout < TimeSpanHelper.MINIMUM)
					_manualResetEventSlim.Wait(Token);
				else if (!_manualResetEventSlim.Wait(millisecondsTimeout, Token))
					return false;

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

		protected override void StopInternal(bool enforce)
		{
			CompleteInternal();
			// Wait for the consumer's thread to finish.
			if (!enforce) WaitInternal(TimeSpanHelper.INFINITE);
			ClearInternal();
			ObjectHelper.Dispose(ref _worker);
		}

		private void Consume()
		{
			if (IsDisposed) return;

			bool entered = false;
			_manualResetEventSlim.Reset();
			OnWorkStarted(EventArgs.Empty);

			try
			{
				_semaphore.Wait(Token);
				entered = true;

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					T item = default(T);
					bool hasItem = false;

					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && !hasItem)
					{
						lock(_lock)
						{
							if (_queue.Count == 0)
							{
								Monitor.Wait(_lock, TimeSpanHelper.MINIMUM_SCHEDULE);
								Thread.Sleep(1);
							}

							if (IsDisposed || Token.IsCancellationRequested || _queue.Count == 0) return;
							item = _queue.Dequeue();
							hasItem = true;
						}
					}

					if (!hasItem) continue;
					if (IsDisposed || Token.IsCancellationRequested) return;

					new Thread(() => Run(item))
					{
						IsBackground = IsBackground,
						Priority = Priority
					}.Start();
				}

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.Count > 0)
				{
					T item;

					lock (_lock)
					{
						if (_queue.Count == 0) return;
						item = _queue.Dequeue();
					}

					if (IsDisposed || Token.IsCancellationRequested) return;

					new Thread(() => Run(item))
					{
						IsBackground = IsBackground,
						Priority = Priority
					}.Start();
				}
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			finally
			{
				if (entered) _semaphore.Release();
			}
		}

		protected override void Run(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			bool entered = false;

			try
			{
				_semaphore.Wait(Token);
				Interlocked.Increment(ref _running);
				entered = true;
				if (IsDisposed || Token.IsCancellationRequested) return;
				base.Run(item);
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			finally
			{
				if (entered)
				{
					_semaphore.Release();
					Interlocked.Decrement(ref _running);
				}

				SignalAndCheck();
			}
		}

		private void SignalAndCheck()
		{
			// don't change the order of this
			if (IsDisposed || !CompleteMarked || Count > 0 || Interlocked.CompareExchange(ref _workCompleted, 1, 0) > 0) return;
			Interlocked.Increment(ref _workCompleted);
			Thread.Sleep(50);
			OnWorkCompleted(EventArgs.Empty);
			_manualResetEventSlim.Set();
		}
	}
}