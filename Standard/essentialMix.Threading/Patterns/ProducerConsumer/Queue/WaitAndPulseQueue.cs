using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using essentialMix.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue
{
	public class WaitAndPulseQueue<TQueue, T> : ProducerConsumerThreadQueue<T>, IProducerQueue<TQueue, T>
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly QueueAdapter<TQueue, T> _queue;
		private readonly Thread[] _workers;

		private AutoResetEvent _workStartedEvent;
		private CountdownEvent _countdown;
		private bool _workStarted;

		public WaitAndPulseQueue([NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new QueueAdapter<TQueue, T>(queue);
			_workStartedEvent = new AutoResetEvent(false);
			_countdown = new CountdownEvent(1);
			_workers = new Thread[Threads];
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;
			ObjectHelper.Dispose(ref _workStartedEvent);
			ObjectHelper.Dispose(ref _countdown);
		}

		/// <inheritdoc />
		public TQueue Queue => _queue.Queue;
		
		/// <inheritdoc />
		public bool IsSynchronized => _queue.IsSynchronized;
		
		/// <inheritdoc />
		public object SyncRoot => _queue.SyncRoot;

		public sealed override int Count => _queue.Count + (_countdown?.CurrentCount ?? 1) - 1;

		public sealed override bool IsBusy => Count > 0;

		[NotNull]
		protected object GetLock()
		{
			return IsSynchronized
						? SyncRoot
						: _queue;
		}

		protected sealed override void EnqueueInternal(T item)
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
							_countdown.AddCount();
							(_workers[i] = new Thread(Consume)
									{
										IsBackground = IsBackground,
										Priority = Priority
									}).Start();
						}
					}
				}

				if (!_workStartedEvent.WaitOne(TimeSpanHelper.HALF)) throw new TimeoutException();
				if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
				OnWorkStarted(EventArgs.Empty);
			}

			object lck = GetLock();

			lock(lck)
			{
				_queue.Enqueue(item);
				Monitor.Pulse(lck);
			}
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			ThrowIfDisposed();
			object lck = GetLock();

			lock(lck)
			{
				if (!_queue.TryDequeue(out item)) return false;
				Monitor.Pulse(lck);
				return true;
			}
		}

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			ThrowIfDisposed();
			object lck = GetLock();

			lock(lck)
			{
				if (!_queue.TryPeek(out item)) return false;
				Monitor.Pulse(lck);
				return true;
			}
		}

		/// <inheritdoc />
		public void RemoveWhile(Predicate<T> predicate)
		{
			ThrowIfDisposed();
			if (_queue.Count == 0) return;
			object lck = GetLock();

			lock(lck)
			{
				ThrowIfDisposed();
				if (_queue.Count == 0) return;

				int n = 0;

				while (_queue.Count > 0)
				{
					if (!_queue.TryPeek(out T item)) continue;
					if (!predicate(item)) break;
					_queue.TryDequeue(out _);
					n++;
				}

				if (n == 0) return;
				Monitor.Pulse(lck);
			}
		}

		protected sealed override void CompleteInternal()
		{
			CompleteMarked = true;
			object lck = GetLock();
			lock(lck) 
				Monitor.PulseAll(lck);
		}

		protected sealed override void ClearInternal()
		{
			object lck = GetLock();

			lock(lck)
			{
				_queue.Clear();
				Monitor.PulseAll(lck);
			}
		}

		protected sealed override bool WaitInternal(int millisecondsTimeout)
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
				// ignored
			}
			catch (TimeoutException)
			{
				// ignored
			}

			return false;
		}

		protected sealed override void StopInternal(bool enforce)
		{
			CompleteInternal();
			// Wait for the consumer's thread to finish.
			if (!enforce) WaitInternal(TimeSpanHelper.INFINITE);
			ClearInternal();
		}

		private void Consume()
		{
			_workStartedEvent.Set();
			if (IsDisposed) return;
			object lck = GetLock();

			try
			{
				T item;

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && _queue.IsEmpty)
					{
						lock(lck)
							Monitor.Wait(lck, TimeSpanHelper.FAST);
					}

					if (IsDisposed || Token.IsCancellationRequested) return;
					if (CompleteMarked) break;

					lock(lck)
					{
						if (IsDisposed || Token.IsCancellationRequested) return;
						if (CompleteMarked) break;
						if (!_queue.TryDequeue(out item)) continue;
					}
					Run(item);
				}

				while (!IsDisposed && !Token.IsCancellationRequested)
				{
					lock(lck)
					{
						if (IsDisposed || Token.IsCancellationRequested) return;
						if (!_queue.TryDequeue(out item)) break;
					}
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

	/// <inheritdoc cref="WaitAndPulseQueue{TQueue,T}" />
	public sealed class WaitAndPulseQueue<T> : WaitAndPulseQueue<Queue<T>, T>, IProducerQueue<T>
	{
		public WaitAndPulseQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(new Queue<T>(), options, token)
		{
		}
	}
}