using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Collections.ProducerConsumer.Queue
{
	public sealed class TaskQueue<T> : ProducerConsumerQueue<T>, IProducerQueue<T>
	{
		private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
		private readonly Task[] _workers;
		
		private AutoResetEvent _workEvent;
		private CountdownEvent _countdown;
		private bool _workStarted;

		public TaskQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_workEvent = new AutoResetEvent(false);
			_countdown = new CountdownEvent(1);
			_workers = new Task[Threads];
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;
			ObjectHelper.Dispose(ref _workEvent);
			ObjectHelper.Dispose(ref _countdown);
		}

		public override int Count => _queue.Count + _countdown.CurrentCount - 1;

		public override bool IsBusy => Count > 0;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;

			if (!_workStarted)
			{
				lock(_workers)
				{
					if (!_workStarted)
					{
						_workStarted = true;

						for (int i = 0; i < _workers.Length; i++)
						{
							_countdown.AddCount();
							_workers[i] = Task.Run(Consume, Token).ConfigureAwait();
						}
					}
				}

				if (!_workEvent.WaitOne(TimeSpanHelper.HALF_SCHEDULE)) throw new TimeoutException();
				if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
				OnWorkStarted(EventArgs.Empty);
			}

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
				if (millisecondsTimeout > TimeSpanHelper.INFINITE) return _countdown.Wait(millisecondsTimeout, Token);
				_countdown.Wait(Token);
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
			Cancel();
			ClearInternal();
		}

		private void Consume()
		{
			_workEvent.Set();
			if (IsDisposed || Token.IsCancellationRequested) return;

			try
			{
				T item;

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					if (!_queue.TryDequeue(out item)) continue;
					Run(item);
				}

				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST_SCHEDULE);

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryDequeue(out item)) 
					Run(item);
			}
			finally
			{
				SignalAndCheck();
			}
		}

		private void SignalAndCheck()
		{
			if (IsDisposed || _countdown == null) return;
			Monitor.Enter(_countdown);

			bool completed = false;

			try
			{
				if (IsDisposed || _countdown == null) return;
				_countdown.Signal();
				if (!CompleteMarked || _countdown.CurrentCount > 1) return;
			}
			finally
			{
				if (_countdown is { CurrentCount: < 2 }) completed = true;

				if (_countdown != null) 
					Monitor.Exit(_countdown);
				else
					completed = true;
			}

			if (!completed) return;
			OnWorkCompleted(EventArgs.Empty);
			_countdown.SignalAll();
		}
	}
}