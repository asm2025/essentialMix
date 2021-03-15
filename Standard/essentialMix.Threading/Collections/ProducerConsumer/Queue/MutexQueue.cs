using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Collections.ProducerConsumer.Queue
{
	public sealed class MutexQueue<T> : NamedProducerConsumerThreadQueue<T>
	{
		private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
		private readonly List<Thread> _running = new List<Thread>();

		private AutoResetEvent _workDoneEvent;
		private Mutex _mutex;
		private Thread _worker;

		public MutexQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
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
			_workDoneEvent = new AutoResetEvent(false);
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
			ObjectHelper.Dispose(ref _mutex);
			ObjectHelper.Dispose(ref _workDoneEvent);
		}

		public override int Count => _queue.Count + _running.Count;

		public override bool IsBusy => Count > 0;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
			_queue.Enqueue(item);
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
				if (millisecondsTimeout < TimeSpanHelper.ZERO)
					_workDoneEvent.WaitOne(Token);
				else if (!_workDoneEvent.WaitOne(millisecondsTimeout, Token))
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
			OnWorkStarted(EventArgs.Empty);
		
			try
			{
				T item;

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked && _queue.TryDequeue(out item))
				{
					T value = item;
					Thread thread = new Thread(() => Run(value))
					{
						IsBackground = IsBackground,
						Priority = Priority
					};
					thread.Start();
					AddRunning(thread);
				}

				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST_SCHEDULE);

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryDequeue(out item))
				{
					T value = item;
					Thread thread = new Thread(() => Run(value))
					{
						IsBackground = IsBackground,
						Priority = Priority
					};
					thread.Start();
					AddRunning(thread);
				}
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			finally
			{
				Thread[] threads = _running.Count > 0
										? _running.ToArray()
										: null;

				if (threads != null)
				{
					foreach (Thread thread in threads) 
						thread.Join();
				}

				OnWorkCompleted(EventArgs.Empty);
				_workDoneEvent.Set();
			}
		}

		protected override void Run(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return;

			bool entered = false;

			try
			{
				if (!_mutex.WaitOne(Token)) return;
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
				_running.Remove(Thread.CurrentThread);
				if (entered) _mutex.ReleaseMutex();
			}
		}

		private void AddRunning([NotNull] Thread thread)
		{
			if (!ObjectLockHelper.WaitFor(() => thread.IsAlive, _running, TimeSpanHelper.HALF_SCHEDULE)) throw new TimeoutException();
			if (!thread.IsAlive) return;
			_running.Add(thread);
		}
	}
}