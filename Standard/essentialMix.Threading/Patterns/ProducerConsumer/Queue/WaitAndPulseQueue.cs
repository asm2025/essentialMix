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
	public sealed class WaitAndPulseQueue<T> : ProducerConsumerThreadQueue<T>, IProducerQueue<T>
	{
		private readonly Queue<T> _queue;
		private readonly ICollection _collection;
		private readonly Thread[] _workers;

		private AutoResetEvent _workEvent;
		private CountdownEvent _countdown;
		private bool _workStarted;

		public WaitAndPulseQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new Queue<T>();
			_collection = _queue;
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

		public object SyncRoot => _collection.SyncRoot;

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

			lock(SyncRoot)
			{
				_queue.Enqueue(item);
				Monitor.Pulse(SyncRoot);
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

			lock(SyncRoot)
			{
				if (_queue.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _queue.Dequeue();
				Monitor.Pulse(SyncRoot);
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

			lock(SyncRoot)
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

			lock(SyncRoot)
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
				Monitor.Pulse(SyncRoot);
			}
		}

		protected override void CompleteInternal()
		{
			CompleteMarked = true;

			lock (SyncRoot) 
				Monitor.PulseAll(SyncRoot);
		}

		protected override void ClearInternal()
		{
			lock(SyncRoot)
			{
				_queue.Clear();
				Monitor.PulseAll(SyncRoot);
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
						lock (SyncRoot)
							Monitor.Wait(SyncRoot, TimeSpanHelper.FAST_SCHEDULE);
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
	
	public sealed class WaitAndPulseQueue<TQueue, T> : ProducerConsumerThreadQueue<T>, IProducerQueue<T>
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly struct QueueAdapter : ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
		{
			private readonly Action<T> _enqueue;
			private readonly Func<T> _dequeue;
			private readonly Action _clear;
			private readonly Func<T> _peek;

			public QueueAdapter([NotNull] TQueue tQueue)
			{
				switch (tQueue)
				{
					case Queue<T> queue:
						_enqueue = queue.Enqueue;
						_dequeue = queue.Dequeue;
						_peek = queue.Peek;
						_clear = queue.Clear;
						break;
					case Stack<T> stack:
						_enqueue = stack.Push;
						_dequeue = stack.Pop;
						_peek = stack.Peek;
						_clear = stack.Clear;
						break;
					case Deque<T> deque:
						_enqueue = deque.Enqueue;
						_dequeue = deque.Dequeue;
						_peek = deque.PeekHead;
						_clear = deque.Clear;
						break;
					case CircularBuffer<T> circularBuffer:
						_enqueue = circularBuffer.Add;
						_dequeue = circularBuffer.Dequeue;
						_peek = circularBuffer.PeekHead;
						_clear = circularBuffer.Clear;
						break;
					case IList<T> list:
						_enqueue = list.Add;
						_dequeue = () =>
						{
							if (list.Count == 0) throw new Exception("Collection is empty.");
							T item = list[0];
							list.RemoveAt(0);
							return item;
						};
						_peek = () =>
						{
							if (list.Count == 0) throw new Exception("Collection is empty.");
							return list[0];
						};
						_clear = list.Clear;
						break;
					default:
						throw new NotSupportedException();
				}
				
				Queue = tQueue;
			}

			public TQueue Queue { get; }

			/// <inheritdoc />
			public bool IsSynchronized => Queue.IsSynchronized;

			/// <inheritdoc />
			public object SyncRoot => Queue.SyncRoot;

			/// <inheritdoc cref="ICollection.Count" />
			public int Count => ((ICollection)Queue).Count;

			/// <inheritdoc />
			public IEnumerator<T> GetEnumerator() { return Queue.GetEnumerator(); }

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

			/// <inheritdoc />
			public void CopyTo(Array array, int index) { Queue.CopyTo(array, index); }

			public void Enqueue(T item) { _enqueue(item); }

			public T Dequeue() { return _dequeue(); }

			public T Peek() { return _peek(); }

			public void Clear() { _clear(); }
		}

		private readonly QueueAdapter _queue;
		private readonly Thread[] _workers;

		private AutoResetEvent _workEvent;
		private CountdownEvent _countdown;
		private bool _workStarted;

		public WaitAndPulseQueue([NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new QueueAdapter(queue);
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

		[NotNull]
		public TQueue Queue => _queue.Queue;
		
		/// <inheritdoc />
		public object SyncRoot => _queue.SyncRoot;

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

			lock(SyncRoot)
			{
				_queue.Enqueue(item);
				Monitor.Pulse(SyncRoot);
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

			lock(SyncRoot)
			{
				if (_queue.Count == 0)
				{
					item = default(T);
					return false;
				}

				item = _queue.Dequeue();
				Monitor.Pulse(SyncRoot);
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

			lock(SyncRoot)
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

			lock(SyncRoot)
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
				Monitor.Pulse(SyncRoot);
			}
		}

		protected override void CompleteInternal()
		{
			CompleteMarked = true;

			lock (SyncRoot) 
				Monitor.PulseAll(SyncRoot);
		}

		protected override void ClearInternal()
		{
			lock(SyncRoot)
			{
				_queue.Clear();
				Monitor.PulseAll(SyncRoot);
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
						lock (SyncRoot)
							Monitor.Wait(SyncRoot, TimeSpanHelper.FAST_SCHEDULE);
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