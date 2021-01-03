using System;
using System.Collections.Concurrent;
using System.Threading;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Threading.Collections.ProducerConsumer.Queue
{
	public sealed class WaitAndPulseQueue<T> : ProducerConsumerThreadQueue<T>
	{
		private readonly object _lock = new object();
		private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
		private readonly Thread[] _workers;

		private CountdownEvent _countdown;
		private bool _workStarted;

		public WaitAndPulseQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_countdown = new CountdownEvent(Threads + 1);
			_workers = new Thread[Threads];
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing) ObjectHelper.Dispose(ref _countdown);
		}

		public override int Count => _queue.Count;

		public override bool IsBusy => _countdown != null && _countdown.CurrentCount > 1;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			if (!_workStarted)
			{
				lock(_workers)
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

						Thread.Sleep(TimeSpanHelper.MINIMUM_SCHEDULE);
						if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
						OnWorkStarted(EventArgs.Empty);
					}
				}
			}

			_queue.Enqueue(item);

			lock (_lock) 
				Monitor.Pulse(_lock);
		}

		protected override void CompleteInternal()
		{
			CompleteMarked = true;

			lock (_lock) 
				Monitor.PulseAll(_lock);
		}

		protected override void ClearInternal()
		{
			_queue.Clear();

			lock (_lock) 
				Monitor.PulseAll(_lock);
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
				T item;

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && _queue.IsEmpty)
					{
						lock (_lock)
							Monitor.Wait(_lock, TimeSpanHelper.FAST_SCHEDULE);
					}

					if (IsDisposed || Token.IsCancellationRequested) return;
					if (CompleteMarked || !_queue.TryDequeue(out item)) break;
					Run(item);
				}

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryDequeue(out item)) 
					Run(item);
			}
			finally
			{
				SignalAndCheck();
			}
		}

		protected override void Run(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			try
			{
				base.Run(item);
			}
			finally
			{
				lock(_lock)
				{
					Monitor.Pulse(_lock);
				}
			}
		}

		private void SignalAndCheck()
		{
			if (IsDisposed) return;
			Monitor.Enter(_countdown);

			try
			{
				_countdown.SignalOne();
				if (!CompleteMarked || _countdown.CurrentCount > 1) return;
				OnWorkCompleted(EventArgs.Empty);
			}
			finally
			{
				if (_countdown.CurrentCount < 2) _countdown.SignalAll();
				Monitor.Exit(_countdown);
			}
		}
	}
}