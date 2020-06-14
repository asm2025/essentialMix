using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Object;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.MessageQueue
{
	/*
	 * This is based on the insightful book of Joseph Albahari, C# 6 in a Nutshell
	 * http://www.albahari.com/threading/
	 */
	public abstract class QueueBase<T> : Disposable, IQueue<T>, ICollection
	{
		private readonly System.Collections.Generic.Queue<T> _queue = new System.Collections.Generic.Queue<T>();

		private ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim(false);
		private Thread _worker;
		private CancellationTokenSource _cts;

		protected QueueBase([NotNull] Action<T> callback, CancellationToken token = default(CancellationToken))
			: this(callback, false, ThreadPriority.Normal, token)
		{
		}

		protected QueueBase([NotNull] Action<T> callback, ThreadPriority priority, CancellationToken token = default(CancellationToken))
			: this(callback, false, priority, token)
		{
		}

		protected QueueBase([NotNull] Action<T> callback, bool isBackground, ThreadPriority priority, CancellationToken token = default(CancellationToken))
		{
			Callback = callback;
			IsBackground = isBackground;
			Priority = priority;
			_cts = new CancellationTokenSource();
			if (token.CanBeCanceled) token.Register(() => _cts.CancelIfNotDisposed());
			Token = _cts.Token;
			(_worker = new Thread(Consume)
			{
				IsBackground = IsBackground,
				Priority = Priority
			}).Start();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopInternal(WaitForQueuedItems);
				ObjectHelper.Dispose(ref _worker);
				ObjectHelper.Dispose(ref _manualResetEventSlim);
				ObjectHelper.Dispose(ref _cts);
			}

			base.Dispose(disposing);
		}

		public Action<T> Callback { get; }

		public object SyncRoot { get; } = new object();

		public bool IsSynchronized => true;

		public virtual int Count
		{
			get
			{
				lock (SyncRoot)
				{
					return _queue.Count;
				}
			}
		}

		public bool WaitForQueuedItems { get; set; } = true;

		public bool IsBackground { get; }

		public ThreadPriority Priority { get; }

		public CancellationToken Token { get; }

		public bool IsBusy { get; protected set; }

		public bool CompleteMarked { get; protected set; }

		public IEnumerator<T> GetEnumerator()
		{
			ThrowIfDisposed();

			System.Collections.Generic.Queue<T>.Enumerator enumerator;
			
			lock (SyncRoot)
			{
				enumerator = _queue.GetEnumerator();
			}

			return enumerator;
		}

		public void CopyTo([NotNull] T[] array, int index)
		{
			ThrowIfDisposed();

			lock (SyncRoot)
			{
				_queue.CopyTo(array, index);
			}
		}

		public void Stop()
		{
			ThrowIfDisposed();
			StopInternal(WaitForQueuedItems);
		}

		public void Stop(bool waitForQueue)
		{
			ThrowIfDisposed();
			StopInternal(waitForQueue);
		}

		public void Enqueue(T item)
		{
			ThrowIfDisposed();
			if (CompleteMarked) throw new InvalidOperationException("Completion marked.");
			EnqueueInternal(item);
		}

		public void Complete()
		{
			ThrowIfDisposed();
			if (CompleteMarked) return;
			CompleteInternal();
		}

		public bool Wait()
		{
			ThrowIfDisposed();
			return WaitInternal(TimeSpanHelper.INFINITE);
		}

		public bool Wait(TimeSpan timeout)
		{
			ThrowIfDisposed();
			return WaitInternal(timeout.TotalIntMilliseconds());
		}

		public bool Wait(int millisecondsTimeout)
		{
			ThrowIfDisposed();
			return WaitInternal(millisecondsTimeout);
		}

		[NotNull]
		public Task<bool> WaitAsync() { return WaitAsync(TimeSpanHelper.INFINITE); }

		[NotNull]
		public Task<bool> WaitAsync(TimeSpan timeout) { return WaitAsync(timeout.TotalIntMilliseconds()); }

		[NotNull]
		public Task<bool> WaitAsync(int millisecondsTimeout)
		{
			ThrowIfDisposed();
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			return Task.Run(() => WaitInternal(millisecondsTimeout), Token);
		}

		public void Clear()
		{
			ThrowIfDisposed();
			ClearInternal();
		}

		protected virtual void EnqueueInternal(T item)
		{
			lock (SyncRoot)
			{
				_queue.Enqueue(item);
				Monitor.Pulse(SyncRoot);
			}
		}

		protected virtual void CompleteInternal()
		{
			lock (SyncRoot)
			{
				CompleteMarked = true;
				Monitor.PulseAll(SyncRoot);
			}
		}

		protected virtual void ClearInternal()
		{
			lock (SyncRoot)
			{
				_queue.Clear();
				Monitor.PulseAll(SyncRoot);
			}
		}

		protected virtual bool WaitInternal(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!IsBusy) return true;

			try
			{
				if (millisecondsTimeout < TimeSpanHelper.MINIMUM)
					_manualResetEventSlim.Wait(Token);
				else if (!_manualResetEventSlim.Wait(millisecondsTimeout, Token))
					return false;

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

		protected virtual void StopInternal(bool waitForQueue)
		{
			CompleteInternal();
			// Wait for the consumer's thread to finish.
			if (waitForQueue) WaitInternal(TimeSpanHelper.INFINITE);
			ClearInternal();
		}

		protected void Cancel() { _cts.CancelIfNotDisposed(); }

		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));

			if (array is T[] tArray)
			{
				CopyTo(tArray, index);
				return;
			}

			/*
			 * Catch the obvious case assignment will fail.
			 * We can find all possible problems by doing the check though.
			 * For example, if the element type of the Array is derived from T,
			 * we can't figure out if we can successfully copy the element beforehand.
			 */
			array.Length.ValidateRange(index, Count);
			Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
			Type sourceType = typeof(IItem);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[] objects)) throw new ArgumentException("Invalid array type", nameof(array));
			if (Count == 0) return;

			try
			{
				foreach (T item in this)
				{
					objects[index++] = item;
				}
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Invalid array type", nameof(array));
			}
		}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		private void Consume()
		{
			if (IsDisposed || IsBusy) return;
			IsBusy = true;

			try
			{
				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					T item = default(T);

					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
					{
						lock (SyncRoot)
						{
							if (_queue.Count == 0)
							{
								Monitor.Wait(SyncRoot, TimeSpanHelper.MINIMUM_SCHEDULE);
								Thread.Sleep(1);
							}

							if (IsDisposed || Token.IsCancellationRequested || _queue.Count == 0) continue;
							item = _queue.Dequeue();
						}

						if (item != null) break;
					}
					
					if (IsDisposed || Token.IsCancellationRequested) return;
					Callback(item);
				}

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.Count > 0)
				{
					T item;
					
					lock (SyncRoot)
					{
						if (_queue.Count == 0) return;
						item = _queue.Dequeue();
					}

					if (IsDisposed || Token.IsCancellationRequested) return;
					Callback(item);
				}
			}
			finally
			{
				IsBusy = false;
				_manualResetEventSlim.Set();
			}
		}
	}
}