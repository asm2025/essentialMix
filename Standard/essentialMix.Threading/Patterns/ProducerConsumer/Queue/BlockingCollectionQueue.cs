using System;
using System.Collections.Concurrent;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue
{
	public sealed class BlockingCollectionQueue<T> : ProducerConsumerThreadQueue<T>
	{
		private readonly Thread[] _workers;

		private BlockingCollection<T> _queue;
		private AutoResetEvent _workEvent;
		private CountdownEvent _countdown;
		private bool _workStarted;

		public BlockingCollectionQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new BlockingCollection<T>();
			_workEvent = new AutoResetEvent(false);
			_countdown = new CountdownEvent(1);
			_workers = new Thread[Threads];
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;
			ObjectHelper.Dispose(ref _workEvent);
			ObjectHelper.Dispose(ref _queue);
			ObjectHelper.Dispose(ref _countdown);
		}

		public override int Count => _queue.Count + (_countdown?.CurrentCount ?? 1) - 1;

		public override bool IsBusy => Count > 0;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;

			if (!_workStarted)
			{
				lock(_workers)
				{
					if (!_workStarted)
					{
						_workStarted = true;

						for (int i = 0; i < _workers.Length; i++)
						{
							_countdown.AddCount();
							(_workers[i] = new Thread(Consume)
									{
										IsBackground = IsBackground,
										Priority = Priority
									}).Start();
						}
					}
				}

				if (!_workEvent.WaitOne(TimeSpanHelper.HALF_SCHEDULE)) throw new TimeoutException();
				if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
				OnWorkStarted(EventArgs.Empty);
			}

			_queue.Add(item, Token);
		}

		protected override void CompleteInternal()
		{
			CompleteMarked = true;
			_queue.CompleteAdding();
		}

		protected override void ClearInternal()
		{
			_queue.Clear();
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
			Cancel();
			ClearInternal();
			if (!_workStarted) return;

			for (int i = 0; i < _workers.Length; i++) 
				ObjectHelper.Dispose(ref _workers[i]);
		}

		private void Consume()
		{
			_workEvent.Set();
			if (IsDisposed) return;

			try
			{
				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && !_queue.IsCompleted)
				{
					while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryTake(out T item, TimeSpanHelper.FAST_SCHEDULE, Token))
						Run(item);
				}

				if (IsDisposed || Token.IsCancellationRequested) return;

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryTake(out T item, TimeSpanHelper.FAST_SCHEDULE, Token))
					Run(item);
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
			if (IsDisposed || _countdown == null) return;
			Monitor.Enter(_countdown);

			bool completed;

			try
			{
				if (IsDisposed || _countdown == null) return;
				_countdown.Signal();
			}
			finally
			{
				completed = _countdown is null or { CurrentCount: < 2 };
			}

			if (completed)
			{
				OnWorkCompleted(EventArgs.Empty);
				_countdown.SignalAll();
			}

			if (_countdown != null) Monitor.Exit(_countdown);
		}
	}
}