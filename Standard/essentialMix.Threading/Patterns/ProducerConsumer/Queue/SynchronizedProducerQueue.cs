using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue
{
	[DebuggerDisplay("Count = {Count}")]
	public class SynchronizedProducerQueue<TQueue, T> : Disposable, IProducerConsumer<T>, IProducerQueue<TQueue, T>
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly IProducerQueue<TQueue, T> _producerQueue;

		private IProducerConsumer<T> _queue;

		/// <inheritdoc />
		public SynchronizedProducerQueue(ThreadQueueMode mode, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
		{
			_queue = ProducerConsumerQueue.Create(mode, options, token);
			_producerQueue = _queue as IProducerQueue<TQueue, T> ?? throw new NotSupportedException();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _queue);
			base.Dispose(disposing);
		}

		/// <inheritdoc />
		public TQueue Queue => _producerQueue.Queue;
		
		/// <inheritdoc />
		public bool IsSynchronized => _producerQueue.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _producerQueue.SyncRoot;

		/// <inheritdoc />
		public bool IsPaused => _queue.IsPaused;

		/// <inheritdoc />
		public bool IsCompleted => _queue.IsCompleted;

		/// <inheritdoc />
		public int Count => _queue.Count;

		/// <inheritdoc />
		public bool IsEmpty => _queue.IsEmpty;

		/// <inheritdoc />
		public int Running => _queue.Running;

		/// <inheritdoc />
		public bool IsBusy => _queue.IsBusy;

		/// <inheritdoc />
		public bool CanPause => _queue.CanPause;

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
		public void InitializeToken(CancellationToken token)
		{
			_queue.InitializeToken(token);
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
		public void Pause() { _queue.Pause(); }

		/// <inheritdoc />
		public void Resume() { _queue.Resume(); }

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
		/// <summary>
		/// Use lock or <see cref="Monitor" /> enter methods with <see cref="SyncRoot" /> to synchronize the queue
		/// </summary>
		public bool TryDequeue(out T item)
		{
			lock(SyncRoot) 
				return _producerQueue.TryDequeue(out item);
		}

		/// <inheritdoc />
		/// <summary>
		/// Use lock or <see cref="Monitor" /> enter methods with <see cref="SyncRoot" /> to synchronize the queue
		/// </summary>
		public bool TryPeek(out T item)
		{
			lock(SyncRoot) 
				return _producerQueue.TryPeek(out item);
		}

		/// <inheritdoc />
		/// <summary>
		/// Use lock or <see cref="Monitor" /> enter methods with <see cref="SyncRoot" /> to synchronize the queue
		/// </summary>
		public void RemoveWhile(Predicate<T> predicate)
		{
			lock(SyncRoot) 
				_producerQueue.RemoveWhile(predicate);
		}
	}

	public sealed class SynchronizedProducerQueue<T> : SynchronizedProducerQueue<Queue<T>, T>, IProducerQueue<T>
	{
		/// <inheritdoc />
		public SynchronizedProducerQueue(ThreadQueueMode mode, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(mode, options, token)
		{
		}
	}
}