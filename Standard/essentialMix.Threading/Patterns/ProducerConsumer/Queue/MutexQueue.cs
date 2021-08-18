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
	/// ONLY use this when you need a <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.mutex">Mutex</see> because it's the MOST
	/// expensive queue of them all unless you know what you're doing and need to have items to be run on their own threads.
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

					if (ScheduledCallback != null && !ScheduledCallback(item)) continue;
					AddTasksCountDown();
					new Thread(RunThread)
					{
						IsBackground = IsBackground,
						Priority = Priority
					}.Start(item);
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
					new Thread(RunThread)
					{
						IsBackground = IsBackground,
						Priority = Priority
					}.Start(item);
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
				if (Token.IsCancellationRequested) return;
				Run((T)rawValue);
			}
			finally
			{
				if (entered) _mutex?.ReleaseMutex();
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