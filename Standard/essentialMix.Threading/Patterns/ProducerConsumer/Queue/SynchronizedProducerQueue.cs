using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue
{
	[DebuggerDisplay("Count = {Count}")]
	public sealed class SynchronizedProducerQueue<T> : Disposable, IProducerConsumer<T>, IProducerQueue<T>
	{
		private readonly IProducerQueue<T> _producerQueue;

		private IProducerConsumer<T> _queue;

		/// <inheritdoc />
		public SynchronizedProducerQueue(ThreadQueueMode mode, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		{
			_queue = ProducerConsumerQueue.Create(mode, options, token);
			_producerQueue = _queue as IProducerQueue<T> ?? throw new NotSupportedException();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _queue);
			base.Dispose(disposing);
		}

		/// <inheritdoc />
		public object SyncRoot => _producerQueue.SyncRoot;

		/// <inheritdoc />
		public bool CompleteMarked => _queue.CompleteMarked;

		/// <inheritdoc />
		public int Count => _queue.Count;

		/// <inheritdoc />
		public bool IsBusy => _queue.IsBusy;

		/// <inheritdoc />
		public int Threads => _queue.Threads;

		/// <inheritdoc />
		public CancellationToken Token => _queue.Token;

		/// <inheritdoc />
		public bool WaitOnDispose
		{
			get => _queue.WaitOnDispose;
			set => _queue.WaitOnDispose = value;
		}

		/// <inheritdoc />
		public int SleepAfterEnqueue
		{
			get => _queue.SleepAfterEnqueue;
			set => _queue.SleepAfterEnqueue = value;
		}

		/// <inheritdoc />
		public event EventHandler WorkStarted
		{
			add => _queue.WorkStarted += value;
			remove => _queue.WorkStarted -= value;
		}

		/// <inheritdoc />
		public event EventHandler WorkCompleted
		{
			add => _queue.WorkCompleted += value;
			remove => _queue.WorkCompleted -= value;
		}

		/// <inheritdoc />
		public void Enqueue(T item)
		{
			lock(SyncRoot) 
				_queue.Enqueue(item);
		}

		/// <inheritdoc />
		public void Complete()
		{
			lock(SyncRoot) 
				_queue.Complete();
		}

		/// <inheritdoc />
		public void Clear()
		{
			lock(SyncRoot) 
				_queue.Clear();
		}

		/// <inheritdoc />
		public void Stop() { _queue.Stop(); }

		/// <inheritdoc />
		public void Stop(bool enforce) { _queue.Stop(enforce); }

		/// <inheritdoc />
		public bool Wait() { return _queue.Wait(); }

		/// <inheritdoc />
		public bool Wait(TimeSpan timeout) { return _queue.Wait(timeout); }

		/// <inheritdoc />
		public bool Wait(int millisecondsTimeout) { return _queue.Wait(millisecondsTimeout); }

		/// <inheritdoc />
		public Task<bool> WaitAsync() { return _queue.WaitAsync(); }

		/// <inheritdoc />
		public Task<bool> WaitAsync(TimeSpan timeout) { return _queue.WaitAsync(timeout); }

		/// <inheritdoc />
		public Task<bool> WaitAsync(int millisecondsTimeout) { return _queue.WaitAsync(millisecondsTimeout); }

		/// <inheritdoc />
		/// <summary>
		/// Use lock or <see cref="Monitor" /> enter methods with <see cref="SyncRoot" /> to synchronize the queue
		/// </summary>
		public bool TryDequeue(out T item)
		{
			return _producerQueue.TryDequeue(out item);
		}

		/// <inheritdoc />
		/// <summary>
		/// Use lock or <see cref="Monitor" /> enter methods with <see cref="SyncRoot" /> to synchronize the queue
		/// </summary>
		public bool TryPeek(out T item)
		{
			return _producerQueue.TryPeek(out item);
		}

		/// <inheritdoc />
		/// <summary>
		/// Use lock or <see cref="Monitor" /> enter methods with <see cref="SyncRoot" /> to synchronize the queue
		/// </summary>
		public void RemoveWhile(Predicate<T> predicate)
		{
			_producerQueue.RemoveWhile(predicate);
		}
	}
}