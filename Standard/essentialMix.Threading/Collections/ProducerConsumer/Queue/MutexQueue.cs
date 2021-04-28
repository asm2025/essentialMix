using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Collections.ProducerConsumer.Queue
{
	public sealed class MutexQueue<T> : NamedProducerConsumerThreadQueue<T>, IProducerQueue<T>
	{
		private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

		private AutoResetEvent _workDoneEvent;
		private Mutex _mutex;
		private Thread _worker;
		private int _running;

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

			if (_mutex != null)
			{
				_mutex.Close();
				ObjectHelper.Dispose(ref _mutex);
			}

			ObjectHelper.Dispose(ref _workDoneEvent);
		}

		public override int Count => _queue.Count + _running;

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
					Task.Run(() => RunAsync(value), Token);
				}

				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST_SCHEDULE);

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryDequeue(out item))
				{
					T value = item;
					Task.Run(() => RunAsync(value), Token);
				}

				SpinWait.SpinUntil(() => IsDisposed || Token.IsCancellationRequested || _running == 0);
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			finally
			{
				OnWorkCompleted(EventArgs.Empty);
				_workDoneEvent.Set();
			}
		}

		[NotNull]
		private Task RunAsync(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested) return Task.CompletedTask;

			bool entered = false;

			try
			{
				if (!_mutex.WaitOne(Token)) return Task.CompletedTask;
				entered = true;
				Interlocked.Increment(ref _running);
				if (IsDisposed || Token.IsCancellationRequested) return Task.CompletedTask;
				Run(item);
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
			}

			return Task.CompletedTask;
		}
	}
}