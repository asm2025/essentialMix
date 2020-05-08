using System;
using System.Collections.Generic;
using System.Threading;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public sealed class SemaphoreSlimQueue : ProducerConsumerThreadQueue, IDisposable
	{
		private readonly object _lock = new object();
		private readonly Queue<TaskItem> _queue = new Queue<TaskItem>();
		private ManualResetEventSlim _manualResetEventSlim;
		private SemaphoreSlim _semaphore;
		private Thread _worker;

		public SemaphoreSlimQueue(CancellationToken token = default(CancellationToken))
			: this(null, token)
		{
		}

		public SemaphoreSlimQueue(ProducerConsumerQueueOptions options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_manualResetEventSlim = new ManualResetEventSlim(false);
			_semaphore = new SemaphoreSlim(Threads, Threads);
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
			ObjectHelper.Dispose(ref _worker);
			ObjectHelper.Dispose(ref _semaphore);
			ObjectHelper.Dispose(ref _manualResetEventSlim);
		}

		protected override int CountInternal
		{
			get
			{
				lock (_lock)
				{
					return _queue.Count;
				}
			}
		}

		protected override bool IsBusyInternal => CountInternal > 0 || _semaphore.CurrentCount < ThreadsInternal;

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
			lock (_lock)
			{
				CompleteMarked = true;
				Monitor.PulseAll(_lock);
			}
		}

		protected override void ClearInternal()
		{
			lock(_lock)
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
				if (millisecondsTimeout < 0 )
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
			_manualResetEventSlim.Reset();
			OnWorkStarted(EventArgs.Empty);

			try
			{
				while (!IsDisposedOrDisposing && !Token.IsCancellationRequested && !CompleteMarked)
				{
					TaskItem item = null;

					while (!IsDisposedOrDisposing && !Token.IsCancellationRequested && !CompleteMarked)
					{
						lock(_lock)
						{
							if (_queue.Count == 0)
							{
								Monitor.Wait(_lock, TimeSpanHelper.MINIMUM_SCHEDULE);
								Thread.Sleep(1);
							}

							if (IsDisposedOrDisposing || Token.IsCancellationRequested || _queue.Count == 0) return;
							item = _queue.Dequeue();
						}
	
						if (item != null) break;
					}

					if (item == null) continue;
					_semaphore.Wait(Token);
					if (IsDisposedOrDisposing || Token.IsCancellationRequested) return;

					new Thread(() => Run(item))
					{
						IsBackground = IsBackground,
						Priority = Priority
					}.Start();
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
					_semaphore.Wait(Token);
					if (IsDisposedOrDisposing || Token.IsCancellationRequested) return;

					new Thread(() => Run(item))
					{
						IsBackground = IsBackground,
						Priority = Priority
					}.Start();
				}
			}
			finally
			{
				OnWorkCompleted(EventArgs.Empty);
				_manualResetEventSlim.Set();
			}
		}

		private void Run([NotNull] TaskItem item)
		{
			try
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
			finally
			{
				_semaphore.Release();
			}
		}
	}
}