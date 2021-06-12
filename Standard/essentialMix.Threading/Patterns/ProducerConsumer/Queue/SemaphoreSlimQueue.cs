using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue
{
	public sealed class SemaphoreSlimQueue<T> : ProducerConsumerThreadQueue<T>, IProducerQueue<T>
	{
		private readonly ConcurrentQueue<T> _queue;
		private readonly ICollection _collection;

		private ManualResetEvent _allWorkDone;
		private SemaphoreSlim _semaphore;
		private Thread _worker;

		private volatile int _running;

		public SemaphoreSlimQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new ConcurrentQueue<T>();
			_collection = _queue;
			_allWorkDone = new ManualResetEvent(false);
			_semaphore = new SemaphoreSlim(Threads);
			(_worker = new Thread(Consume)
			{
				IsBackground = IsBackground,
				Priority = Priority
			}).Start();
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
		public object SyncRoot => _collection.SyncRoot;

		public override int Count
		{
			get
			{
				int threads = _running;
				return _queue.Count + threads;
			}
		}

		public override bool IsBusy => Count > 0;

		protected override void EnqueueInternal(T item)
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

		protected override void CompleteInternal()
		{
			CompleteMarked = true;
		}

		protected override void ClearInternal()
		{
			_queue.Clear();
		}

		protected override bool WaitInternal(int millisecondsTimeout)
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
			Cancel();
			ClearInternal();
			ObjectHelper.Dispose(ref _worker);
		}

		private void Consume()
		{
			if (IsDisposed) return;
			OnWorkStarted(EventArgs.Empty);

			Task[] tasks = new Task[Threads];
			Interlocked.Exchange(ref _running, 0);

			try
			{
				T item;

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					if (!_queue.TryDequeue(out item)) continue;
					T value = item;
					int index = Array.FindIndex(tasks,e => e == null || e.IsFinished());

					if (tasks[index] != null)
					{
						// reuse finished task slot
						ObjectHelper.Dispose(ref tasks[index]);
						tasks[index] = null;
						Interlocked.Decrement(ref _running);
					}

					tasks[index] = Task.Run(() => RunThread(value), Token).ConfigureAwait();
					Interlocked.Increment(ref _running);
					// don't wait until all slots are filled
					if (_running < Threads) continue;
					Task.WaitAny(tasks, Token);
				}

				if (Token.IsCancellationRequested) return;
				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST_SCHEDULE);

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryDequeue(out item))
				{
					T value = item;
					int index = Array.FindIndex(tasks,e => e == null || e.IsFinished());

					if (tasks[index] != null)
					{
						// reuse finished task slot
						ObjectHelper.Dispose(ref tasks[index]);
						tasks[index] = null;
						Interlocked.Decrement(ref _running);
					}

					tasks[index] = Task.Run(() => RunThread(value), Token).ConfigureAwait();
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
}