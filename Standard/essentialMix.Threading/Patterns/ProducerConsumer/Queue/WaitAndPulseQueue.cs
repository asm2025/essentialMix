using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using essentialMix.Collections;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue
{
	public class WaitAndPulseQueue<TQueue, T> : ProducerConsumerThreadQueue<T>, IProducerQueue<TQueue, T>
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly QueueAdapter<TQueue, T> _queue;
		private readonly Thread[] _workers;

		public WaitAndPulseQueue([NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new QueueAdapter<TQueue, T>(queue);
			_workers = new Thread[Threads];
		}

		/// <inheritdoc />
		public TQueue Queue => _queue.Queue;
		
		/// <inheritdoc />
		public bool IsSynchronized => _queue.IsSynchronized;
		
		/// <inheritdoc />
		public object SyncRoot => _queue.SyncRoot;

		/// <inheritdoc />
		public override int Count => _queue.Count + Running;

		/// <inheritdoc />
		public override bool IsEmpty => _queue.Count == 0;
		
		/// <inheritdoc />
		public override bool CanPause => true;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;

			if (!WaitForWorkerStart())
			{
				bool invokeWorkStarted = false;

				lock(_workers)
				{
					if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;

					if (!WaitForWorkerStart())
					{
						InitializeWorkerStart();
						InitializeWorkersCountDown();
						InitializeTasksCountDown();

						for (int i = 0; i < _workers.Length; i++)
						{
							(_workers[i] = new Thread(Consume)
									{
										IsBackground = IsBackground,
										Priority = Priority
									}).Start();
						}
					
						invokeWorkStarted = true;
						if (!WaitForWorkerStart()) throw new TimeoutException();
						if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
					}
				}

				if (invokeWorkStarted) WorkStartedCallback?.Invoke(this);
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

			lock(SyncRoot)
			{
				if (!_queue.TryDequeue(out item)) return false;
				Monitor.Pulse(SyncRoot);
			}

			return true;
		}

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			ThrowIfDisposed();
			
			lock(SyncRoot)
				return _queue.TryPeek(out item);
		}

		/// <inheritdoc />
		public void RemoveWhile(Predicate<T> predicate)
		{
			ThrowIfDisposed();

			lock(SyncRoot)
			{
				if (_queue.IsEmpty) return;

				int n = _queue.Count;

				while (!_queue.IsEmpty && _queue.TryPeek(out T item) && predicate(item)) 
					_queue.Dequeue();

				if (n == _queue.Count) return;
				Monitor.Pulse(SyncRoot);
			}
		}

		protected override void CompleteInternal()
		{
			lock(SyncRoot)
			{
				CompleteMarked = true;
				Monitor.PulseAll(SyncRoot);
			}
		}

		protected override void ClearInternal()
		{
			lock(SyncRoot)
			{
				_queue.Clear();
				Monitor.PulseAll(SyncRoot);
			}
		}

		private void Consume()
		{
			if (IsDisposed || Token.IsCancellationRequested) return;
			SignalWorkerStart();

			try
			{
				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					if (IsPaused)
					{
						SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
						continue;
					}

					SpinWait spinner = new SpinWait();

					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && _queue.IsEmpty)
					{
						if (IsPaused)
						{
							SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
							continue;
						}

						lock(SyncRoot)
						{
							if (IsPaused || IsDisposed || Token.IsCancellationRequested || !_queue.IsEmpty) continue;
							Monitor.Wait(SyncRoot, TimeSpanHelper.FAST);
						}

						spinner.SpinOnce();
					}

					if (IsDisposed || Token.IsCancellationRequested) return;
					T item;

					lock(SyncRoot)
					{
						if (IsPaused || _queue.IsEmpty || !_queue.TryDequeue(out item)) continue;
					}

					if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
					AddTasksCountDown();
					Run(item);
				}

				if (IsDisposed || Token.IsCancellationRequested) return;

				while (!IsDisposed && !Token.IsCancellationRequested && !_queue.IsEmpty)
				{
					if (IsPaused)
					{
						SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
						continue;
					}

					T item;

					lock(SyncRoot)
					{
						if (IsPaused || _queue.IsEmpty || !_queue.TryDequeue(out item)) continue;
					}

					if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
					AddTasksCountDown();
					Run(item);
				}
			}
			catch (ObjectDisposedException) { }
			catch (OperationCanceledException) { }
			finally
			{
				SignalWorkersCountDown();
			}
		}

		/// <inheritdoc />
		protected override void Run(T item)
		{
			try
			{
				if (IsDisposed || Token.IsCancellationRequested) return;
				base.Run(item);
			}
			finally
			{
				SignalTasksCountDown();
			}
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