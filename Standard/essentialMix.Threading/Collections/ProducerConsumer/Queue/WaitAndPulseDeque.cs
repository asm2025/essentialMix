using System;
using System.Threading;
using essentialMix.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Collections.ProducerConsumer.Queue
{
	public sealed class WaitAndPulseDeque<T> : ProducerConsumerThreadQueue<T>, IProducerDeque<T>
	{
		private readonly object _lock = new object();
		private readonly Deque<T> _deque;
		private readonly Thread[] _workers;

		private AutoResetEvent _workEvent;
		private CountdownEvent _countdown;
		private bool _workStarted;

		public WaitAndPulseDeque([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_deque = new Deque<T>();
			_workEvent = new AutoResetEvent(false);
			_countdown = new CountdownEvent(Threads + 1);
			_workers = new Thread[Threads];
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;
			ObjectHelper.Dispose(ref _workEvent);
			ObjectHelper.Dispose(ref _countdown);
		}

		public override int Count => _deque.Count;

		public override bool IsBusy => _countdown is { CurrentCount: > 1 };

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;
			StartWorkers();

			lock(_lock)
			{
				_deque.Enqueue(item);
				Monitor.Pulse(_lock);
			}
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			ThrowIfDisposed();

			if (_deque.Count == 0)
			{
				item = default(T);
				return false;
			}

			lock(_lock)
			{
				ThrowIfDisposed();

				if (_deque.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _deque.Dequeue();
				return true;
			}
		}

		public void Push(T item)
		{
			ThrowIfDisposed();
			if (CompleteMarked) throw new InvalidOperationException("Completion marked.");
			if (Token.IsCancellationRequested) return;
			StartWorkers();

			lock(_lock)
			{
				_deque.Push(item);
				Monitor.Pulse(_lock);
			}

			if (SleepAfterEnqueue > 0) Thread.Sleep(SleepAfterEnqueue);
		}

		/// <inheritdoc />
		public bool TryPop(out T item)
		{
			ThrowIfDisposed();

			if (_deque.Count == 0)
			{
				item = default(T);
				return false;
			}

			lock(_lock)
			{
				ThrowIfDisposed();

				if (_deque.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _deque.Pop();
				return true;
			}
		}

		/// <inheritdoc />
		public bool TryPeekHead(out T item)
		{
			ThrowIfDisposed();

			if (_deque.Count == 0)
			{
				item = default(T);
				return false;
			}

			lock(_lock)
			{
				ThrowIfDisposed();

				if (_deque.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _deque.PeekHead();
				return true;
			}
		}

		/// <inheritdoc />
		public bool TryPeekTail(out T item)
		{
			ThrowIfDisposed();

			if (_deque.Count == 0)
			{
				item = default(T);
				return false;
			}

			lock(_lock)
			{
				ThrowIfDisposed();

				if (_deque.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _deque.PeekTail();
				return true;
			}
		}

		protected override void CompleteInternal()
		{
			CompleteMarked = true;

			lock (_lock) 
				Monitor.PulseAll(_lock);
		}

		protected override void ClearInternal()
		{
			lock(_lock)
			{
				_deque.Clear();
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

		private void StartWorkers()
		{
			if (_workStarted) return;

			lock(_workers)
			{
				if (_workStarted) return;
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

			if (!_workEvent.WaitOne(TimeSpanHelper.HALF_SCHEDULE)) throw new TimeoutException();
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
			OnWorkStarted(EventArgs.Empty);
		}

		private void Consume()
		{
			_workEvent.Set();
			if (IsDisposed) return;

			try
			{
				T item;

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && _deque.Count == 0)
					{
						lock (_lock)
							Monitor.Wait(_lock, TimeSpanHelper.FAST_SCHEDULE);
					}

					if (IsDisposed || Token.IsCancellationRequested) return;
					if (CompleteMarked) break;
					if (_deque.Count == 0) continue;
					item = _deque.Dequeue();
					Run(item);
				}

				while (!IsDisposed && !Token.IsCancellationRequested)
				{
					if (_deque.Count == 0) break;
					item = _deque.Dequeue();
					Run(item);
				}
			}
			finally
			{
				SignalAndCheck();
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