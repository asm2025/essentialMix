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
		public TQueue Queue => _queue.Queue;
		
		/// <inheritdoc />
		public bool IsSynchronized => _queue.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _queue.SyncRoot;

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
			return _queue.TryPeek(out item);
		}

		/// <inheritdoc />
		public void RemoveWhile(Predicate<T> predicate)
		{
			ThrowIfDisposed();

			lock(SyncRoot)
			{
				while (_queue.TryPeek(out T item) && predicate(item)) 
					_queue.TryDequeue(out _);
			}
		}

		protected sealed override void CompleteInternal()
		{
			CompleteMarked = true;
		}

		protected sealed override void ClearInternal()
		{
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
			if (IsDisposed) return;
			OnWorkStarted(EventArgs.Empty);

			try
			{
				int count = 0;
				T[] items = new T[Threads];
				Task[] tasks = new Task[Threads];

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					if (!ReadItems(items, ref count) || count < items.Length) break;

					for (int i = 0; !IsDisposed && !Token.IsCancellationRequested && i < items.Length; i++)
					{
						T item = items[i];
						_countdown.AddCount();
						tasks[i] = RunAsync(item);
					}

					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitSilently(Token)) return;
					count = 0;
					Array.Clear(tasks, 0, tasks.Length);
				}

				if (count == items.Length) count = 0;
				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST);

				while (!IsDisposed && !Token.IsCancellationRequested)
				{
					if (!ReadItems(items, ref count) || count < items.Length) break;

					for (int i = 0; !IsDisposed && !Token.IsCancellationRequested && i < items.Length; i++)
					{
						T item = items[i];
						_countdown.AddCount();
						tasks[i] = RunAsync(item);
					}

					if (IsDisposed || Token.IsCancellationRequested || !tasks.WaitSilently(Token)) return;

					for (int i = 0; i < tasks.Length; i++) 
						ObjectHelper.Dispose(ref tasks[i]);

					count = 0;
					Array.Clear(tasks, 0, tasks.Length);
				}

				if (count < 1 || IsDisposed || Token.IsCancellationRequested) return;
				Array.Resize(ref tasks, ++count);

				for (int i = 0; !IsDisposed && !Token.IsCancellationRequested && i < tasks.Length; i++)
				{
					T item = items[i];
					_countdown.AddCount();
					tasks[i] = RunAsync(item);
				}

				if (!tasks.WaitSilently(Token)) return;
				
				for (int i = 0; i < tasks.Length; i++)
					ObjectHelper.Dispose(ref tasks[i]);
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

			while (count > 0 && !_queue.IsEmpty)
			{
				lock(SyncRoot)
				{
					if (_queue.IsEmpty || !_queue.TryDequeue(out T item)) continue;
					items[offset++] = item;
					count--;
					read++;
				}
			}

			return !IsDisposed && !Token.IsCancellationRequested && read > 0;
		}

		[NotNull]
		private Task RunAsync(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return Task.CompletedTask;
			return Task.Run(() =>
			{
				try
				{
					Run(item);
				}
				finally
				{
					_countdown?.SignalOne();
				}
			}, Token);
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