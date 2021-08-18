using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using essentialMix.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue
{
	public class DataFlowQueue<TQueue, T> : ProducerConsumerThreadQueue<T>, IProducerQueue<TQueue, T>
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly QueueAdapter<TQueue, T> _queue;

		private ActionBlock<T> _processor;

		public DataFlowQueue([NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new QueueAdapter<TQueue, T>(queue);
		}

		/// <inheritdoc />
		// ReSharper disable once InconsistentlySynchronizedField
		public TQueue Queue => _queue.Queue;
		
		/// <inheritdoc />
		// ReSharper disable once InconsistentlySynchronizedField
		public bool IsSynchronized => _queue.IsSynchronized;

		/// <inheritdoc />
		// ReSharper disable once InconsistentlySynchronizedField
		public object SyncRoot => _queue.SyncRoot;

		/// <inheritdoc />
		// ReSharper disable once InconsistentlySynchronizedField
		public override int Count => _queue.Count + Running;

		/// <inheritdoc />
		// ReSharper disable once InconsistentlySynchronizedField
		public override bool IsEmpty => _queue.Count == 0;

		/// <inheritdoc />
		public override bool CanPause => true;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || IsCompleted) return;

			if (!WaitForWorkerStart())
			{
				bool invokeWorkStarted = false;

				lock(SyncRoot)
				{
					if (!WaitForWorkerStart())
					{
						InitializeWorkerStart();
						InitializeWorkersCountDown(1);
						InitializeTasksCountDown();

						// configure the action block
						InitializeMesh();

						new Thread(Consume)
						{
							IsBackground = IsBackground,
							Priority = Priority
						}.Start();
					
						invokeWorkStarted = true;
						if (!WaitForWorkerStart()) throw new TimeoutException();
						if (IsDisposed || Token.IsCancellationRequested || IsCompleted) return;
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
				IsCompleted = true;
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

		private void InitializeMesh()
		{
			_processor = new ActionBlock<T>(item =>
			{
				if (ScheduledCallback != null && !ScheduledCallback(item)) return;
				AddTasksCountDown();
				Run(item);
			}, new ExecutionDataflowBlockOptions
			{
				// SingleProducerConstrained = false by default therefore, BufferBlock<T>.SendAsync is thread safe
				MaxDegreeOfParallelism = Threads,
				CancellationToken = Token
			});

			_processor.Completion
					.ConfigureAwait()
					.ContinueWith(_ => SignalWorkersCountDown());
		}

		private void Consume()
		{
			if (IsDisposed || Token.IsCancellationRequested) return;
			SignalWorkerStart();

			try
			{
				while (!IsDisposed && !Token.IsCancellationRequested && !IsCompleted)
				{
					if (IsPaused)
					{
						SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
						continue;
					}

					if (!WaitForQueue()) continue;
					if (IsDisposed || Token.IsCancellationRequested) return;
					T item;

					lock(SyncRoot)
					{
						if (IsPaused || _queue.IsEmpty || !_queue.TryDequeue(out item)) continue;
					}

					_processor.Post(item);
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

					_processor.Post(item);
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

		private bool WaitForQueue()
		{
			if (!_queue.IsEmpty) return true;
			SpinWait spinner = new SpinWait();

			while (!IsDisposed && !Token.IsCancellationRequested && !IsCompleted && _queue.IsEmpty)
			{
				if (IsPaused) return false;

				lock(SyncRoot)
				{
					if (IsPaused || IsDisposed || Token.IsCancellationRequested || !_queue.IsEmpty) continue;
					Monitor.Wait(SyncRoot, TimeSpanHelper.FAST);
				}

				spinner.SpinOnce();
			}

			return !IsDisposed && !Token.IsCancellationRequested && !IsCompleted && !_queue.IsEmpty;
		}
	}

	/// <inheritdoc cref="DataFlowQueue{TQueue,T}" />
	public sealed class DataFlowQueue<T> : DataFlowQueue<Queue<T>, T>, IProducerQueue<T>
	{
		public DataFlowQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(new Queue<T>(), options, token)
		{
		}
	}
}