using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public sealed class TaskQueue : ProducerConsumerQueue, IDisposable
	{
		private readonly object _lock = new object();
		private readonly object _threadsLock = new object();
		private readonly Queue<TaskItem> _queue = new Queue<TaskItem>();
		private readonly Task[] _workers;
		private CountdownEvent _countdown;

		public TaskQueue(CancellationToken token = default(CancellationToken))
			: this(null, token)
		{
		}

		public TaskQueue(ProducerConsumerQueueOptions options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_countdown = new CountdownEvent(Threads + 1);
			_workers = new Task[Threads];
			OnWorkStarted(EventArgs.Empty);
		
			for (int i = 0; i < _workers.Length; i++)
				_workers[i] = Task.Run(Consume, Token).ConfigureAwait();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;

			for (int i = 0; i < _workers.Length; i++)
				ObjectHelper.Dispose(ref _workers[i]);

			ObjectHelper.Dispose(ref _countdown);
		}

		protected override int CountInternal
		{
			get
			{
				lock(_lock)
				{
					return _queue.Count;
				}
			}
		}

		protected override bool IsBusyInternal => _countdown.CurrentCount > 1;

		protected override void EnqueueInternal(TaskItem item)
		{
			if (IsDisposedOrDisposing || Token.IsCancellationRequested) return;

			lock (_lock)
			{
				item.Token = Token;
				_queue.Enqueue(item);
				Monitor.Pulse(_lock);
			}
		}

		protected override void CompleteInternal()
		{
			lock(_lock)
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
				while (!IsDisposedOrDisposing && !Token.IsCancellationRequested && !CompleteMarked)
				{
					TaskItem item = null;

					while (!IsDisposedOrDisposing && !Token.IsCancellationRequested && !CompleteMarked)
					{
						lock (_lock)
						{
							if (_queue.Count == 0)
							{
								Monitor.Wait(_lock, TimeSpanHelper.MINIMUM_SCHEDULE);
								Thread.Sleep(1);
							}

							if (IsDisposedOrDisposing || Token.IsCancellationRequested || _queue.Count == 0) continue;
							item = _queue.Dequeue();
						}

						if (item != null) break;
					}

					if (item == null) continue;
					if (IsDisposedOrDisposing || Token.IsCancellationRequested) return;
					Run(item);
				}

				while (!IsDisposedOrDisposing && !Token.IsCancellationRequested && _queue.Count > 0)
				{
					TaskItem item;

					lock (_lock)
					{
						if (_queue.Count == 0) return;
						item = _queue.Dequeue();
					}

					if (item == null) continue;
					if (IsDisposedOrDisposing || Token.IsCancellationRequested) return;
					Run(item);
				}
			}
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

			lock (_threadsLock)
			{
				_countdown.SignalOne();
				if (!(_countdown.CurrentCount <= 1 && CompleteMarked)) return;
			}

			OnWorkCompleted(EventArgs.Empty);
			_countdown.SignalAll();
		}
	}
}