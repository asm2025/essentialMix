using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Collections.ProducerConsumer.Queue
{
	public sealed class SemaphoreSlimQueue<T> : ProducerConsumerThreadQueue<T>, IProducerQueue<T>
	{
		private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

		private CountdownEvent _countdown;
		private SemaphoreSlim _semaphore;
		private Thread _worker;

		public SemaphoreSlimQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_countdown = new CountdownEvent(1);
			_semaphore = new SemaphoreSlim(Threads);
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
			ObjectHelper.Dispose(ref _semaphore);
			ObjectHelper.Dispose(ref _countdown);
		}

		public override int Count => _queue.Count + _countdown.CurrentCount - 1;

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
			ObjectHelper.Dispose(ref _worker);
		}

		private void Consume()
		{
			if (IsDisposed) return;
			OnWorkStarted(EventArgs.Empty);

			try
			{
				T item;

				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
				{
					if (!_queue.TryDequeue(out item)) continue;
					_countdown.AddCount();
					RunAsync(item);
				}

				TimeSpanHelper.WasteTime(TimeSpanHelper.FAST_SCHEDULE);

				while (!IsDisposed && !Token.IsCancellationRequested && _queue.TryDequeue(out item))
				{
					_countdown.AddCount();
					RunAsync(item);
				}
			}
			finally
			{
				SignalAndCheck();
			}
		}

		[NotNull]
		private Task RunAsync(T item)
		{
			if (IsDisposed) return Task.CompletedTask;

			if (Token.IsCancellationRequested)
			{
				_countdown?.SignalOne();
				return Task.CompletedTask;
			}

			return _semaphore.WaitAsync(Token)
							.ContinueWith(task =>
							{
								if (IsDisposed || Token.IsCancellationRequested || !task.IsCompleted)
								{
									_semaphore?.Release();
									_countdown?.SignalOne();
									return;
								}

								try
								{
									Run(item);
								}
								finally
								{
									_semaphore?.Release();
									SignalAndCheck();
								}
							}, Token);
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
				if (_countdown is { CurrentCount: < 2 })
				{
					_countdown.SignalAll();
					completed = true;
				}
				
				if (_countdown != null) 
					Monitor.Exit(_countdown);
				else
					completed = true;
			}

			if (!completed) return;
			OnWorkCompleted(EventArgs.Empty);
		}
	}
}