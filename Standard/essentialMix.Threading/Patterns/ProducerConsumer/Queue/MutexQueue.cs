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

		private CountdownEvent _countdown;
		private ManualResetEvent _allWorkDone;
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
			_allWorkDone = new ManualResetEvent(false);
			
			new Thread(Consume)
			{
				IsBackground = IsBackground,
				Priority = Priority
			}.Start();
		}

		/// <inheritdoc />
		protected sealed override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;

			if (_mutex != null)
			{
				_mutex.Close();
				ObjectHelper.Dispose(ref _mutex);
			}
			
			ObjectHelper.Dispose(ref _countdown);
			ObjectHelper.Dispose(ref _allWorkDone);
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

		protected override void ClearInternal()
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
			ObjectHelper.Dispose(ref _countdown);
		}

		private void Consume()
		{
			if (IsDisposed) return;
			OnWorkStarted(EventArgs.Empty);
			
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

					ScheduledCallback?.Invoke(item);

					Thread thread = new Thread(RunThread)
					{
						IsBackground = IsBackground,
						Priority = Priority
					};

					if (_countdown == null) _countdown = new CountdownEvent(2);
					else _countdown.AddCount();
					thread.Start(item);
					if (_countdown.CurrentCount <= Threads) continue;
					_countdown.Signal();
					_countdown.Wait(Token);
					ObjectHelper.Dispose(ref _countdown);
				}

				if (IsDisposed || Token.IsCancellationRequested) return;
				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST);

				while (!IsDisposed && !Token.IsCancellationRequested)
				{
					if (IsPaused) continue;
					T item;

					lock(SyncRoot)
					{
						if (IsPaused) continue;
						if (_queue.IsEmpty || !_queue.TryDequeue(out item)) break;
					}

					ScheduledCallback?.Invoke(item);

					Thread thread = new Thread(RunThread)
					{
						IsBackground = IsBackground,
						Priority = Priority
					};

					if (_countdown == null) _countdown = new CountdownEvent(2);
					else _countdown.AddCount();
					thread.Start(item);
					if (_countdown.CurrentCount <= Threads) continue;
					_countdown.Signal();
					_countdown.Wait(Token);
					ObjectHelper.Dispose(ref _countdown);
				}

				if (_countdown is not { CurrentCount: > 1 }) return;
				_countdown.Signal();
				_countdown.Wait(Token);
			}
			finally
			{
				ObjectHelper.Dispose(ref _countdown);
				OnWorkCompleted(EventArgs.Empty);
				_allWorkDone?.Set();
			}
		}

		private void RunThread(object rawValue)
		{
			if (IsDisposed) return;

			if (Token.IsCancellationRequested)
			{
				_countdown?.Signal();
				return;
			}

			bool entered = false;

			try
			{
				if (!_mutex.WaitOne(TimeSpanHelper.INFINITE, Token)) return;
				entered = true;
				if (IsDisposed || Token.IsCancellationRequested) return;
				Run((T)rawValue);
			}
			finally
			{
				if (entered) _mutex?.ReleaseMutex();
				_countdown?.Signal();
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