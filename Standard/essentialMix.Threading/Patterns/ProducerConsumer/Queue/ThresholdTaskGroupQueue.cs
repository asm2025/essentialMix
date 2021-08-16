using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue
{
	public class ThresholdTaskGroupQueue<TQueue, T> : ProducerConsumerThresholdQueue<T>, IProducerQueue<TQueue, T>
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly QueueAdapter<TQueue, T> _queue;

		public ThresholdTaskGroupQueue([NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new QueueAdapter<TQueue, T>(queue);
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
			if (IsDisposed || Token.IsCancellationRequested || IsCompleted) return;
			
			if (!WaitForWorkerStart())
			{
				bool invokeWorkStarted = false;

				lock(SyncRoot)
				{
					if (IsDisposed || Token.IsCancellationRequested || IsCompleted) return;

					if (!WaitForWorkerStart())
					{
						InitializeWorkerStart();
						InitializeWorkersCountDown(1);
						InitializeTasksCountDown();

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

		private void Consume()
		{
			if (IsDisposed || Token.IsCancellationRequested) return;
			SignalWorkerStart();

			try
			{
				int count = 0;
				int threads = Threads;
				if (HasThreshold) threads++;
				T[] items = new T[Threads];
				Task[] tasks = new Task[threads];

				while (!IsDisposed && !Token.IsCancellationRequested && !IsCompleted)
				{
					if (IsPaused)
					{
						SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
						continue;
					}

					if (HasThreshold && count == 0)
					{
						AddTasksCountDown();
						tasks[tasks.Length - 1] = Task.Delay(Threshold, Token)
											.ContinueWith(_ => SignalTasksCountDown(), Token)
											.ConfigureAwait();
					}

					int offset = count;
					if (!ReadItems(items, ref count, true)) break;
					if (count < Threads) continue;
					SetupTasks(items, tasks, offset, count);
					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitAllSilently(Token)) return;
					Array.Clear(items, 0, count);
					tasks[tasks.Length - 1].Dispose();
					tasks[tasks.Length - 1] = null;
					Array.Clear(tasks, 0, count);
					count = 0;
				}

				if (IsDisposed || Token.IsCancellationRequested) return;

				while (!IsDisposed && !Token.IsCancellationRequested)
				{
					if (IsPaused)
					{
						SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
						continue;
					}

					if (HasThreshold && count == 0)
					{
						AddTasksCountDown();
						tasks[tasks.Length - 1] = Task.Delay(Threshold, Token)
													.ContinueWith(_ => SignalTasksCountDown(), Token)
													.ConfigureAwait();
					}

					int offset = count;
					if (!ReadItems(items, ref count, false) || count < items.Length) break;
					SetupTasks(items, tasks, offset, count);
					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitAllSilently(Token)) return;
					Array.Clear(items, 0, count);
					tasks[tasks.Length - 1].Dispose();
					tasks[tasks.Length - 1] = null;
					Array.Clear(tasks, 0, count);
					count = 0;
				}

				if (count < 1 || IsDisposed || Token.IsCancellationRequested) return;
				// it's not important for the last group to keep a threshold because they are the last group.
				if (tasks.Length != count) Array.Resize(ref tasks, count);
				SetupTasks(items, tasks, 0, count);
				if (IsPaused) SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || !IsPaused);
				if (IsDisposed || Token.IsCancellationRequested) return;
				tasks.WaitAllSilently(Token);
				Array.Clear(items, 0, count);
				tasks[tasks.Length - 1].Dispose();
				tasks[tasks.Length - 1] = null;
				Array.Clear(tasks, 0, count);
			}
			catch (ObjectDisposedException) { }
			catch (OperationCanceledException) { }
			finally
			{
				SignalWorkersCountDown();
			}
		}

		private bool ReadItems([NotNull] IList<T> items, ref int offset, bool waitOnQueue)
		{
			if (IsDisposed || Token.IsCancellationRequested) return false;

			int count = items.Count - offset;
			if (count < 1) return false;

			int read = 0;

			while (!IsDisposed && !Token.IsCancellationRequested && count > 0)
			{
				// ReSharper disable once InconsistentlySynchronizedField
				if (IsPaused || waitOnQueue && _queue.IsEmpty) continue;
				// ReSharper disable once InconsistentlySynchronizedField
				if (_queue.IsEmpty) break;

				lock(SyncRoot)
				{
					if (IsPaused || IsDisposed || Token.IsCancellationRequested || _queue.IsEmpty || !_queue.TryDequeue(out T item)) continue;
					items[read + offset] = item;
					count--;
					read++;
				}
			}

			offset += read;
			return !IsDisposed && !Token.IsCancellationRequested && read > 0;
		}
		
		private void SetupTasks(IReadOnlyList<T> items, IList<Task> tasks, int offset, int count)
		{
			for (int i = 0; !IsDisposed && !Token.IsCancellationRequested && i < count; i++)
			{
				T item = items[i];
				if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
				AddTasksCountDown();
				// this better be a real thread (which LongRunning is for), don't be tempted to use Task.Run.
				tasks[i + offset] = TaskHelper.Run(() => Run(item), TaskCreationOptions.LongRunning, Token)
										.ConfigureAwait();
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

	/// <inheritdoc cref="ThresholdTaskGroupQueue{TQueue,T}"/>
	public sealed class ThresholdTaskGroupQueue<T> : ThresholdTaskGroupQueue<Queue<T>, T>, IProducerQueue<T>
	{
		/// <inheritdoc />
		public ThresholdTaskGroupQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(new Queue<T>(), options, token)
		{
		}
	}
}