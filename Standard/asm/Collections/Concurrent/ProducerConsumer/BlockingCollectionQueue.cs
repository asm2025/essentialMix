using System;
using System.Collections.Concurrent;
using System.Threading;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public sealed class BlockingCollectionQueue : ProducerConsumerThreadQueue, IDisposable
	{
		private readonly object _lock = new object();
		private readonly object _threadsLock = new object();
		private readonly Thread[] _workers;
		private BlockingCollection<TaskItem> _queue;
		private CountdownEvent _countdown;

		public BlockingCollectionQueue(CancellationToken token = default(CancellationToken))
			: this(null, token)
		{
		}

		public BlockingCollectionQueue(ProducerConsumerQueueOptions options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_queue = new BlockingCollection<TaskItem>();
			_countdown = new CountdownEvent(Threads + 1);
			_workers = new Thread[Threads];
			OnWorkStarted(EventArgs.Empty);

			for (int i = 0; i < _workers.Length; i++)
			{
				(_workers[i] = new Thread(Consume)
				{
					IsBackground = IsBackground,
					Priority = Priority
				}).Start();
			}
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;

			for (int i = 0; i < _workers.Length; i++)
				ObjectHelper.Dispose(ref _workers[i]);

			ObjectHelper.Dispose(ref _queue);
			ObjectHelper.Dispose(ref _countdown);
		}

		protected override int CountInternal => _queue.Count;

		protected override bool IsBusyInternal => _countdown.CurrentCount > 1;

		protected override void EnqueueInternal(TaskItem item)
		{
			if (IsDisposedOrDisposing || Token.IsCancellationRequested) return;

			lock (_lock)
			{
				item.Token = Token;
				_queue.Add(item, Token);
				Monitor.Pulse(_lock);
			}
		}

		protected override void CompleteInternal()
		{
			lock (_lock)
			{
				CompleteMarked = true;
				_queue.CompleteAdding();
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
			if (!IsBusyInternal) return true;

			try
			{
				if (millisecondsTimeout < 0)
				{
					_countdown.Wait(Token);
					return true;
				}

				return _countdown.Wait(millisecondsTimeout, Token);
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
			lock (_lock)
			{
				Cancel();
				Monitor.PulseAll(_lock);
			}

			// Wait for the consumer's thread to finish.
			if (!enforce) WaitInternal(TimeSpanHelper.INFINITE);
			ClearInternal();
		}

		private void Consume()
		{
			if (IsDisposedOrDisposing) return;

			try
			{
				while (!IsDisposedOrDisposing && !Token.IsCancellationRequested && !CompleteMarked && !_queue.IsCompleted)
				{
					while (_queue.TryTake(out TaskItem item, TimeSpanHelper.MINIMUM_SCHEDULE, Token) && !IsDisposedOrDisposing && !Token.IsCancellationRequested)
					{
						if (item == null) continue;
						Run(item);
						if (IsDisposedOrDisposing || Token.IsCancellationRequested) break;
					}
				}

				if (IsDisposedOrDisposing || Token.IsCancellationRequested) return;

				while (_queue.TryTake(out TaskItem item, TimeSpanHelper.MINIMUM_SCHEDULE, Token) && !IsDisposedOrDisposing && !Token.IsCancellationRequested)
				{
					if (item == null) continue;
					Run(item);
					if (IsDisposedOrDisposing || Token.IsCancellationRequested) break;
				}
			}
			catch (ObjectDisposedException) { }
			catch (OperationCanceledException) { }
			finally
			{
				SignalAndCheck();
			}
		}

		private void Run([NotNull] TaskItem item)
		{
			if (IsDisposedOrDisposing || item.Token.IsCancellationRequested) return;

			TaskItemResult result = new TaskItemResult(item);

			try
			{
				result.Result = item.OnExecute(item);
			}
			catch (Exception e)
			{
				item.Exception = e;
				result.Result = TaskQueueResult.Error;
				return;
			}
			finally
			{
				item.OnCleanUp?.Invoke(item);
			}

			if (IsDisposedOrDisposing || item.Token.IsCancellationRequested) return;

			switch (item.Exception)
			{
				case TimeoutException _:
					item.OnResult?.Invoke(item, TaskQueueResult.Timeout);
					break;
				case AggregateException agto when agto.InnerException is TimeoutException:
					item.OnResult?.Invoke(item, TaskQueueResult.Timeout);
					break;
				case OperationCanceledException _:
					item.OnResult?.Invoke(item, TaskQueueResult.Canceled);
					break;
				case AggregateException agto when agto.InnerException is OperationCanceledException:
					item.OnResult?.Invoke(item, TaskQueueResult.Canceled);
					break;
				default:
					item.OnResult?.Invoke(item, item.Exception == null ? result.Result : TaskQueueResult.Error);
					break;
			}
		}

		private void SignalAndCheck()
		{
			if (IsDisposedOrDisposing) return;

			bool done;

			lock (_threadsLock)
			{
				_countdown.SignalOne();
				done = _countdown.CurrentCount == 1 && CompleteMarked;
			}

			if (!done) return;
			OnWorkCompleted(EventArgs.Empty);
			_countdown.SignalAll();
		}
	}
}