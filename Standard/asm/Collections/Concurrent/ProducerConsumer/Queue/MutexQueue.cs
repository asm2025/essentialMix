using System;
using System.Collections.Generic;
using System.Threading;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer.Queue
{
	public sealed class MutexQueue<T> : NamedProducerConsumerThreadQueue<T>
	{
		private readonly object _lock = new object();
		private readonly Queue<T> _queue = new Queue<T>();

		private ManualResetEventSlim _manualResetEventSlim;
		private Mutex _mutex;
		private Thread _worker;
		private int _running;
		private int _workCompleted;

		public MutexQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			bool createdNew;

			if (Name == null)
			{
				_mutex = new Mutex(false, Name, out createdNew);
			}
			else
			{
				try
				{
					_mutex = Mutex.OpenExisting(Name);
					createdNew = false;
				}
				catch
				{
					_mutex = new Mutex(false, Name, out createdNew);
				}
			}

			IsOwner = createdNew;
			_manualResetEventSlim = new ManualResetEventSlim(false);
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
			StopInternal(WaitOnDispose);
			ObjectHelper.Dispose(ref _mutex);
			ObjectHelper.Dispose(ref _manualResetEventSlim);
		}

		public override int Count
		{
			get
			{
				lock(_lock)
				{
					return _queue.Count + _running;
				}
			}
		}

		public override bool IsBusy => Count > 0;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			lock (_lock)
			{
				_queue.Enqueue(item);
				Monitor.Pulse(_lock);
			}
		}

		protected override void CompleteInternal()
		{
			lock (_lock)
			{
				CompleteMarked = true;
				Monitor.PulseAll(_lock);
			}
		}

		protected override void ClearInternal()
		{
			lock (_lock)
			{
				_queue.Clear();
				Monitor.PulseAll(_lock);
			}
		}

		protected override bool WaitInternal(int millisecondsTimeout)
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

		protected override void StopInternal(bool enforce)
		{
			CompleteInternal();
			// Wait for the consumer's thread to finish.
			if (!enforce) WaitInternal(TimeSpanHelper.INFINITE);
			ClearInternal();
			ObjectHelper.Dispose(ref _worker);
		}

		private void Consume()
		{
			if (IsDisposed) return;
	
			_manualResetEventSlim.Reset();
			OnWorkStarted(EventArgs.Empty);
		
			try
			{
				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					T item = default(T);
					bool hasItem = false;

					while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && !hasItem)
					{
						lock (_lock)
						{
							if (_queue.Count == 0)
							{
								Monitor.Wait(_lock, TimeSpanHelper.MINIMUM_SCHEDULE);
								Thread.Sleep(1);
							}

							if (IsDisposed || Token.IsCancellationRequested || _queue.Count == 0) continue;
							item = _queue.Dequeue();
							hasItem = true;
						}
					}

					if (IsDisposed || Token.IsCancellationRequested) return;
					if (!hasItem) continue;
					Run(item);
				}

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.Count > 0)
				{
					T item;

					lock (_lock)
					{
						if (_queue.Count == 0) return;
						item = _queue.Dequeue();
					}

					if (IsDisposed || Token.IsCancellationRequested) return;
					Run(item);
				}
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			finally
			{
				SignalAndCheck();
			}
		}

		protected override void Run(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			bool entered = false;

			try
			{
				if (!_mutex.WaitOne(Token)) return;
				Interlocked.Increment(ref _running);
				entered = true;
				if (IsDisposed || Token.IsCancellationRequested) return;
				base.Run(item);
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			finally
			{
				if (entered)
				{
					_mutex.ReleaseMutex();
					Interlocked.Decrement(ref _running);
				}

				SignalAndCheck();
			}
		}

		private void SignalAndCheck()
		{
			// don't change the order of this
			if (IsDisposed || !CompleteMarked || Count > 0 || Interlocked.CompareExchange(ref _workCompleted, 1, 0) > 0) return;
			Interlocked.Increment(ref _workCompleted);
			Thread.Sleep(50);
			OnWorkCompleted(EventArgs.Empty);
			_manualResetEventSlim.Set();
		}
	}
}