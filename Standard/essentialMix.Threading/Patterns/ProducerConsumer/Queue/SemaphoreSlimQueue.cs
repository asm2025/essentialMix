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
	public class SemaphoreSlimQueue<TQueue, T> : ProducerConsumerThreadQueue<T>, IProducerQueue<TQueue, T>
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly QueueAdapter<TQueue, T> _queue;

		private ManualResetEvent _allWorkDone;
		private SemaphoreSlim _semaphore;

		private volatile int _running;

		public SemaphoreSlimQueue([NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new QueueAdapter<TQueue, T>(queue);
			_allWorkDone = new ManualResetEvent(false);
			_semaphore = new SemaphoreSlim(Threads);
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
			ObjectHelper.Dispose(ref _semaphore);
			ObjectHelper.Dispose(ref _allWorkDone);
		}

		/// <inheritdoc />
		public TQueue Queue => _queue.Queue;
		
		/// <inheritdoc />
		public bool IsSynchronized => _queue.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _queue.SyncRoot;

		public sealed override int Count
		{
			get
			{
				int threads = _running;
				return _queue.Count + threads;
			}
		}

		public sealed override bool IsBusy => Count > 0;

		protected sealed override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
			_queue.Enqueue(item);
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			ThrowIfDisposed();
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

			while (_queue.TryPeek(out T item) && predicate(item))
			{
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
				if (millisecondsTimeout > TimeSpanHelper.INFINITE) return _allWorkDone.WaitOne(millisecondsTimeout, Token);
				_allWorkDone.WaitOne(Token);
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

			Task[] tasks = new Task[Threads];
			Interlocked.Exchange(ref _running, 0);

			try
			{
				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					if (!_queue.TryDequeue(out T item)) continue;
					int index = Array.FindIndex(tasks,e => e == null || e.IsFinished());

					if (tasks[index] != null)
					{
						// reuse finished task slot
						ObjectHelper.Dispose(ref tasks[index]);
						tasks[index] = null;
						Interlocked.Decrement(ref _running);
					}

					tasks[index] = Task.Run(() => RunThread(item), Token).ConfigureAwait();
					Interlocked.Increment(ref _running);
					// don't wait until all slots are filled
					if (_running < Threads) continue;
					Task.WaitAny(tasks, Token);
				}

				if (Token.IsCancellationRequested) return;
				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST);

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryDequeue(out T item))
				{
					int index = Array.FindIndex(tasks,e => e == null || e.IsFinished());

					if (tasks[index] != null)
					{
						// reuse finished task slot
						ObjectHelper.Dispose(ref tasks[index]);
						tasks[index] = null;
						Interlocked.Decrement(ref _running);
					}

					tasks[index] = Task.Run(() => RunThread(item), Token).ConfigureAwait();
					Interlocked.Increment(ref _running);
					// don't wait until all slots are filled
					if (_running < Threads) continue;
					Task.WaitAny(tasks, Token);
				}
	
				if (_running == 0) return;
				int firstNullIndex = Array.FindIndex(tasks,e => e == null);
				if (firstNullIndex > -1) Array.Resize(ref tasks, firstNullIndex + 1);
				Task.WaitAll(tasks, Token);
				Interlocked.Exchange(ref _running, 0);
			}
			finally
			{
				OnWorkCompleted(EventArgs.Empty);
				_allWorkDone.Set();
			}
		}

		private void RunThread(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			bool entered = false;

			try
			{
				if (!_semaphore.Wait(TimeSpanHelper.INFINITE, Token)) return;
				entered = true;
				if (IsDisposed || Token.IsCancellationRequested) return;
				Run(item);
			}
			finally
			{
				if (entered) _semaphore?.Release();
			}
		}
	}

	/// <inheritdoc cref="SemaphoreSlimQueue{TQueue,T}"/>
	public sealed class SemaphoreSlimQueue<T> : SemaphoreSlimQueue<Queue<T>, T>, IProducerQueue<T>
	{
		/// <inheritdoc />
		public SemaphoreSlimQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(new Queue<T>(), options, token)
		{
		}
	}
}