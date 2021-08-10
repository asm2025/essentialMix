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
	public class TaskGroupQueue<TQueue, T> : ProducerConsumerThreadQueue<T>, IProducerQueue<TQueue, T>
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly QueueAdapter<TQueue, T> _queue;

		private CountdownEvent _countdown;

		public TaskGroupQueue([NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new QueueAdapter<TQueue, T>(queue);
			_countdown = new CountdownEvent(1);
			new Thread(Consume)
			{
				IsBackground = IsBackground,
				Priority = Priority
			}.Start();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;
			ObjectHelper.Dispose(ref _countdown);
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

		// ReSharper disable once InconsistentlySynchronizedField
		public sealed override int Count => _queue.Count + (_countdown?.CurrentCount ?? 1) - 1;

		public sealed override bool IsBusy => Count > 0;

		protected sealed override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;

			lock(SyncRoot) 
				_queue.Enqueue(item);
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			ThrowIfDisposed();

			lock(SyncRoot)
				return _queue.TryDequeue(out item);
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
				while (!_queue.IsEmpty && _queue.TryPeek(out T item) && predicate(item)) 
					_queue.Dequeue();
			}
		}

		protected sealed override void CompleteInternal()
		{
			CompleteMarked = true;
		}

		protected sealed override void ClearInternal()
		{
			lock(SyncRoot)
				_queue.Clear();
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
			Cancel();
			ClearInternal();
		}

		private void Consume()
		{
			if (IsDisposed || Token.IsCancellationRequested) return;
			OnWorkStarted(EventArgs.Empty);

			try
			{
				int offset;
				int count = 0;
				T[] items = new T[Threads];
				Task[] tasks = new Task[Threads];

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					if (IsPaused) continue;
					offset = count;
					if (!ReadItems(items, ref count)) break;
					if (count < Threads) continue;

					for (int i = offset; !IsDisposed && !Token.IsCancellationRequested && i < offset + count; i++)
					{
						// copy to local variables
						int index = i;
						T item = items[index];
						Task[] tasksRef = tasks;
						ScheduledCallback?.Invoke(item);
						_countdown.AddCount();
						tasks[i] = Task.Run(() =>
										{
											try
											{
												Run(item);
											}
											finally
											{
												tasksRef[index].Dispose();
												tasksRef[index] = null;
												_countdown?.SignalOne();
											}
										}, Token)
										.ConfigureAwait();
					}

					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitAllSilently(Token)) return;
					count = 0;
				}

				if (IsDisposed || Token.IsCancellationRequested) return;
				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST);

				while (!IsDisposed && !Token.IsCancellationRequested)
				{
					if (IsPaused) continue;
					offset = count;
					if (!ReadItems(items, ref count) || count < items.Length) break;

					for (int i = offset; !IsDisposed && !Token.IsCancellationRequested && i < offset + count; i++)
					{
						// copy to local variables
						int index = i;
						T item = items[index];
						Task[] tasksRef = tasks;
						ScheduledCallback?.Invoke(item);
						_countdown.AddCount();
						tasks[i] = Task.Run(() =>
										{
											try
											{
												Run(item);
											}
											finally
											{
												tasksRef[index].Dispose();
												tasksRef[index] = null;
												_countdown?.SignalOne();
											}
										}, Token)
										.ConfigureAwait();
					}

					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitAllSilently(Token)) return;
					count = 0;
				}

				if (count < 1 || IsDisposed || Token.IsCancellationRequested) return;
				offset = count;
				Array.Resize(ref tasks, ++count);

				for (int i = offset; !IsDisposed && !Token.IsCancellationRequested && i < offset + count; i++)
				{
					// copy to local variables
					int index = i;
					T item = items[index];
					Task[] tasksRef = tasks;
					ScheduledCallback?.Invoke(item);
					_countdown.AddCount();
					tasks[i] = Task.Run(() =>
									{
										try
										{
											Run(item);
										}
										finally
										{
											tasksRef[index].Dispose();
											tasksRef[index] = null;
											_countdown?.SignalOne();
										}
									}, Token)
									.ConfigureAwait();
				}

				if (IsDisposed || Token.IsCancellationRequested) return;
				tasks.WaitAllSilently(Token);
			}
			finally
			{
				OnWorkCompleted(EventArgs.Empty);
				_countdown.SignalAll();
			}
		}

		private bool ReadItems([NotNull] IList<T> items, ref int offset)
		{
			if (IsDisposed || Token.IsCancellationRequested) return false;

			int count = items.Count - offset;
			if (count < 1) return false;

			int read = 0;

			while (!IsDisposed && !Token.IsCancellationRequested && count > 0)
			{
				// ReSharper disable once InconsistentlySynchronizedField
				if (IsPaused || _queue.IsEmpty) continue;

				lock(SyncRoot)
				{
					if (IsPaused || _queue.IsEmpty || !_queue.TryDequeue(out T item)) continue;
					items[offset++] = item;
					count--;
					read++;
				}
			}

			return !IsDisposed && !Token.IsCancellationRequested && read > 0;
		}
	}

	/// <inheritdoc cref="TaskGroupQueue{TQueue,T}"/>
	public sealed class TaskGroupQueue<T> : TaskGroupQueue<Queue<T>, T>, IProducerQueue<T>
	{
		/// <inheritdoc />
		public TaskGroupQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(new Queue<T>(), options, token)
		{
		}
	}
}