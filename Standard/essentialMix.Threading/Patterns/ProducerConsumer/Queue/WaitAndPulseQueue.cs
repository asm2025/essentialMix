using System;
using System.Collections.Generic;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue
{
	public sealed class WaitAndPulseQueue<T> : ProducerConsumerThreadQueue<T>, IProducerQueue<T>
	{
		private readonly object _lock = new object();
		private readonly Queue<T> _queue = new Queue<T>();
		private readonly Thread[] _workers;

		private AutoResetEvent _workEvent;
		private CountdownEvent _countdown;
		private bool _workStarted;

		public WaitAndPulseQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
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
			ObjectHelper.Dispose(ref _countdown);
		}

		public override int Count => _queue.Count + (_countdown?.CurrentCount ?? 1) - 1;

		public override bool IsBusy => Count > 0;

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
							_countdown.AddCount();
						}
					}
				}

				if (!_workEvent.WaitOne(TimeSpanHelper.HALF_SCHEDULE)) throw new TimeoutException();
				if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
				OnWorkStarted(EventArgs.Empty);
			}

			lock(_lock)
			{
				_queue.Enqueue(item);
				Monitor.Pulse(_lock);
			}
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			ThrowIfDisposed();
			
			if (_queue.Count == 0)
			{
				item = default(T);
				return false;
			}

			lock(_lock)
			{
				if (_queue.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _queue.Dequeue();
				Monitor.Pulse(_lock);
				return true;
			}
		}

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			ThrowIfDisposed();

			if (_queue.Count == 0)
			{
				item = default(T);
				return false;
			}

			lock(_lock)
			{
				ThrowIfDisposed();

				if (_queue.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _queue.Peek();
				return true;
			}
		}

		/// <inheritdoc />
		public void RemoveWhile(Predicate<T> predicate)
		{
			ThrowIfDisposed();
			if (_queue.Count == 0) return;

			lock(_lock)
			{
				ThrowIfDisposed();
				if (_queue.Count == 0) return;

				int n = 0;

				while (_queue.Count > 0)
				{
					T item = _queue.Peek();
					if (!predicate(item)) break;
					_queue.Dequeue();
					n++;
				}

				if (n == 0) return;
				Monitor.Pulse(_lock);
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
			_workEvent.Set();
			if (IsDisposed) return;

			try
			{
				T item;

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && _queue.Count == 0)
					{
						lock (_lock)
							Monitor.Wait(_lock, TimeSpanHelper.FAST_SCHEDULE);
					}

					if (IsDisposed || Token.IsCancellationRequested) return;
					if (CompleteMarked || _queue.Count == 0) continue;
					item = _queue.Dequeue();
					Run(item);
				}

				while (_queue.Count > 0 && !IsDisposed && !Token.IsCancellationRequested)
				{
					item = _queue.Dequeue();
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
			if (IsDisposed || _countdown == null) return;
			Monitor.Enter(_countdown);

			bool completed = false;

			try
			{
				if (IsDisposed || _countdown == null) return;
				_countdown.Signal();
				if (!CompleteMarked || _countdown.CurrentCount > 1) return;
			}
			finally
			{
				if (_countdown is { CurrentCount: < 2 })
				{
					_countdown.SignalAll();
					completed = true;
				}
				
				if (_countdown != null) 
					Monitor.Exit(_countdown);
				else
					completed = true;
			}

			if (!completed) return;
			OnWorkCompleted(EventArgs.Empty);
		}
	}
}