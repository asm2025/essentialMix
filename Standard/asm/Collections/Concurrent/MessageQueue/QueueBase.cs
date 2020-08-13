using System;
using System.Collections;
using System.Collections.Concurrent;
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
	public abstract class QueueBase<T> : Disposable, IQueue<T>, ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IDisposable
	{
		private BlockingCollection<T> _queue;
		private ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim(false);
		private Thread _worker;
		private CancellationTokenSource _cts;
		private object _syncRoot;

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
			_queue = new BlockingCollection<T>();
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
				ObjectHelper.Dispose(ref _queue);
				ObjectHelper.Dispose(ref _manualResetEventSlim);
				ObjectHelper.Dispose(ref _cts);
			}
			base.Dispose(disposing);
		}

		public Action<T> Callback { get; }

		public object SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		bool ICollection.IsSynchronized => false;

		public virtual int Count => _queue.Count;

		public bool WaitForQueuedItems { get; set; } = true;

		public bool IsBackground { get; }

		public ThreadPriority Priority { get; }

		public CancellationToken Token { get; }

		public bool IsBusy => _manualResetEventSlim != null && !_manualResetEventSlim.IsSet;

		public bool CompleteMarked => _queue.IsAddingCompleted;

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return ((IEnumerable<T>)_queue).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_queue).GetEnumerator();
		}

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)_queue).CopyTo(array, index);
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
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
			_queue.Add(item, Token);
		}

		protected virtual void CompleteInternal()
		{
			_queue.CompleteAdding();
		}

		protected virtual void ClearInternal()
		{
			_queue.Clear();
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
			ObjectHelper.Dispose(ref _worker);
		}

		protected void Cancel() { _cts.CancelIfNotDisposed(); }

		private void Consume()
		{
			if (IsDisposed) return;

			_manualResetEventSlim.Reset();

			try
			{
				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && !_queue.IsCompleted)
				{
					while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryTake(out T item, TimeSpanHelper.MINIMUM_SCHEDULE, Token))
						Callback(item);
				}

				if (IsDisposed || Token.IsCancellationRequested) return;

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryTake(out T item, TimeSpanHelper.MINIMUM_SCHEDULE, Token))
					Callback(item);
			}
			catch (ObjectDisposedException) { }
			catch (OperationCanceledException) { }
			finally
			{
				_manualResetEventSlim.Set();
			}
		}
	}
}