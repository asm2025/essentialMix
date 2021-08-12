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
	/// <summary>
	/// Not recommended because it's way too expensive unless you know what you're doing and need to
	/// have items run on their own threads.
	/// </summary>
	public class MutexQueue<TQueue, T> : NamedProducerConsumerThreadQueue<T>, IProducerQueue<TQueue, T>
		where TQueue : ICollection, IReadOnlyCollection<T>
	{
		private readonly QueueAdapter<TQueue, T> _queue;

		private Mutex _mutex;

		public MutexQueue([NotNull] TQueue queue, [NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			bool createdNew;
			string name = Name.ToNullIfEmpty()?.LeftMax(Win32.MAX_PATH);

			if (name == null)
			{
				_mutex = new Mutex(false, null, out createdNew);
			}
			else
			{
				try
				{
					_mutex = Mutex.OpenExisting(name);
					createdNew = false;
				}
				catch (WaitHandleCannotBeOpenedException)
				{
					_mutex = new Mutex(false, name, out createdNew);
				}
			}

			IsOwner = createdNew;
			_queue = new QueueAdapter<TQueue, T>(queue);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing || _mutex == null) return;
			_mutex.Close();
			ObjectHelper.Dispose(ref _mutex);
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
		public override bool CanResume => true;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;

			if (!WaitForWorkerStart())
			{
				bool invokeWorkStarted = false;

				lock(SyncRoot)
				{
					if (!WaitForWorkerStart())
					{
						InitializeWorkerStart();
						InitializeWorkersCountDown(1);
						InitializeBatchClear();
						InitializeTaskStart();
						InitializeTaskComplete();
						InitializeTasksCountDown();

						new Thread(Consume)
						{
							IsBackground = IsBackground,
							Priority = Priority
						}.Start();
					
						invokeWorkStarted = true;
						if (!WaitForWorkerStart()) throw new TimeoutException();
						if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
					}
				}

				if (invokeWorkStarted) OnWorkStarted(EventArgs.Empty);
			}

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

		protected override void CompleteInternal()
		{
			CompleteMarked = true;
		}

		protected override void ClearInternal()
		{
			lock(SyncRoot)
				_queue.Clear();
		}

		private void Consume()
		{
			if (IsDisposed || Token.IsCancellationRequested) return;
			SignalWorkerStart();
			
			try
			{
				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					if (IsPaused) continue;
					T item;

					lock(SyncRoot)
					{
						if (IsPaused || _queue.IsEmpty || !_queue.TryDequeue(out item)) continue;
					}

					if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
					AddTasksCountDown();
					new Thread(RunThread)
					{
						IsBackground = IsBackground,
						Priority = Priority
					}.Start(item);
					// WaitForTaskStart won't return false unless this thing is being destroyed
					if (!WaitForTaskStart()) return;
				}

				if (IsDisposed || Token.IsCancellationRequested) return;

				while (!IsDisposed && !Token.IsCancellationRequested && !_queue.IsEmpty)
				{
					if (IsPaused) continue;
					T item;

					lock(SyncRoot)
					{
						if (IsPaused || _queue.IsEmpty || !_queue.TryDequeue(out item)) continue;
					}

					if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
					AddTasksCountDown();
					new Thread(RunThread)
					{
						IsBackground = IsBackground,
						Priority = Priority
					}.Start(item);
					// WaitForTaskStart won't return false unless this thing is being destroyed
					if (!WaitForTaskStart()) return;
				}
			}
			catch (ObjectDisposedException) { }
			catch (OperationCanceledException) { }
			finally
			{
				SignalWorkersCountDown();
			}
		}

		private void RunThread(object rawValue)
		{
			bool entered = false;

			try
			{
				if (IsDisposed || Token.IsCancellationRequested) return;
				if (!_mutex.WaitOne(TimeSpanHelper.INFINITE, Token)) return;
				if (IsDisposed) return;
				entered = true;
				SignalTaskStart();
				if (Token.IsCancellationRequested) return;
				Run((T)rawValue);
			}
			finally
			{
				if (entered)
				{
					_mutex?.ReleaseMutex();
					SignalTaskComplete();
				}

				SignalTasksCountDown();
			}
		}
	}

	/// <inheritdoc cref="MutexQueue{TQueue,T}" />
	public sealed class MutexQueue<T> : MutexQueue<Queue<T>, T>, IProducerQueue<T>
	{
		/// <inheritdoc />
		public MutexQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(new Queue<T>(), options, token)
		{
		}
	}
}