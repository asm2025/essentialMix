using System;
using System.Collections.Generic;
using System.Threading;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer.Queue
{
	public sealed class WaitAndPulseQueue<T> : ProducerConsumerThreadQueue<T>
	{
		private readonly object _lock = new object();
		private readonly object _threadsLock = new object();
		private readonly Queue<T> _queue = new Queue<T>();
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
			if (!disposing) return;
			StopInternal(WaitOnDispose);
			ObjectHelper.Dispose(ref _countdown);
		}

		public override int Count
		{
			get
			{
				lock (_lock)
				{
					return _queue.Count;
				}
			}
		}

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
				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					T item = default(T);
					bool hasItem = false;

					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && !hasItem)
					{
						lock (_lock)
						{
							if (_queue.Count == 0)
							{
								Monitor.Wait(_lock, TimeSpanHelper.MINIMUM_SCHEDULE);
								Thread.Sleep(1);
							}

							if (IsDisposed || Token.IsCancellationRequested || _queue.Count == 0) continue;
							item = _queue.Dequeue();
							hasItem = true;
						}
					}

					if (IsDisposed || Token.IsCancellationRequested) return;
					if (!hasItem) continue;
					Run(item);
				}

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.Count > 0)
				{
					T item;

					lock (_lock)
					{
						if (_queue.Count == 0) return;
						item = _queue.Dequeue();
					}

					if (item == null) continue;
					if (IsDisposed || Token.IsCancellationRequested) return;
					Run(item);
				}
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