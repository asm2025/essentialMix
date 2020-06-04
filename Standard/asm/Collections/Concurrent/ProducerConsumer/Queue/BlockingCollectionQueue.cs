using System;
using System.Collections.Concurrent;
using System.Threading;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer.Queue
{
	public sealed class BlockingCollectionQueue<T> : ProducerConsumerThreadQueue<T>
	{
		private readonly object _lock = new object();
		private readonly object _threadsLock = new object();
		private readonly Thread[] _workers;

		private BlockingCollection<T> _queue;
		private CountdownEvent _countdown;
		private bool _workStarted;

		public BlockingCollectionQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new BlockingCollection<T>();
			_countdown = new CountdownEvent(Threads + 1);
			_workers = new Thread[Threads];
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;
			StopInternal(WaitOnDispose);
			ObjectHelper.Dispose(ref _queue);
			ObjectHelper.Dispose(ref _countdown);
		}

		public override int Count => _queue.Count;

		public override bool IsBusy => _countdown.CurrentCount > 1;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			if (!_workStarted)
			{
				lock(_threadsLock)
				{
					if (!_workStarted)
					{
						_workStarted = true;

						for (int i = 0; i < _workers.Length; i++)
						{
							(_workers[i] = new Thread(Consume)
									{
										IsBackground = IsBackground,
										Priority = Priority
									}).Start();
						}
					}
				}

				OnWorkStarted(EventArgs.Empty);
				Thread.Sleep(0);
			}

			lock (_lock)
			{
				_queue.Add(item, Token);
				Monitor.Pulse(_lock);
			}
		}

		protected override void CompleteInternal()
		{
			lock (_lock)
			{
				CompleteMarked = true;
				_queue.CompleteAdding();
				Monitor.PulseAll(_lock);
			}
		}

		protected override void ClearInternal()
		{
			lock (_lock)
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

		protected override void StopInternal(bool enforce)
		{
			CompleteInternal();
			// Wait for the consumer's thread to finish.
			if (!enforce) WaitInternal(TimeSpanHelper.INFINITE);
			ClearInternal();
			if (!_workStarted) return;

			for (int i = 0; i < _workers.Length; i++) 
				ObjectHelper.Dispose(ref _workers[i]);
		}

		private void Consume()
		{
			if (IsDisposed) return;

			try
			{
				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && !_queue.IsCompleted)
				{
					while (_queue.TryTake(out T item, TimeSpanHelper.MINIMUM_SCHEDULE, Token) && !IsDisposed && !Token.IsCancellationRequested)
					{
						Run(item);
						if (IsDisposed || Token.IsCancellationRequested) break;
					}
				}

				if (IsDisposed || Token.IsCancellationRequested) return;

				while (_queue.TryTake(out T item, TimeSpanHelper.MINIMUM_SCHEDULE, Token) && !IsDisposed && !Token.IsCancellationRequested)
				{
					Run(item);
					if (IsDisposed || Token.IsCancellationRequested) break;
				}
			}
			catch (ObjectDisposedException) { }
			catch (OperationCanceledException) { }
			finally
			{
				SignalAndCheck();
			}
		}

		private void SignalAndCheck()
		{
			if (IsDisposed) return;

			lock (_threadsLock)
			{
				_countdown.SignalOne();
				if (!CompleteMarked || _countdown.CurrentCount > 1) return;
			}

			_countdown.SignalAll();
			OnWorkCompleted(EventArgs.Empty);
		}
	}
}