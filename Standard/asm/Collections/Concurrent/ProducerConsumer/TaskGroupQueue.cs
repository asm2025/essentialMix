using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public sealed class TaskGroupQueue : ProducerConsumerThreadQueue, IDisposable
	{
		private readonly object _lock = new object();
		private readonly Queue<TaskItem> _queue = new Queue<TaskItem>();
		private ManualResetEventSlim _manualResetEventSlim;
		private Thread _worker;

		public TaskGroupQueue(CancellationToken token = default(CancellationToken))
			: this(null, token)
		{
		}

		public TaskGroupQueue(ProducerConsumerQueueOptions options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
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
			if (!disposing)
				return;
			ObjectHelper.Dispose(ref _worker);
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

		protected override bool IsBusyInternal => CountInternal > 0;

		protected override void EnqueueInternal(TaskItem item)
		{
			if (IsDisposedOrDisposing || Token.IsCancellationRequested)
				return;

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
			lock (_lock)
			{
				_queue.Clear();
				Monitor.PulseAll(_lock);
			}
		}

		protected override bool WaitInternal(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE)
				throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!IsBusyInternal)
				return true;

			try
			{
				if (millisecondsTimeout < 0)
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
			if (!enforce)
				WaitInternal(TimeSpanHelper.INFINITE);
			ClearInternal();
		}

		private void Consume()
		{
			if (IsDisposedOrDisposing) return;
			_manualResetEventSlim.Reset();

			try
			{
				int n = 0;
				Task[] tasks = new Task[ThreadsInternal];

				while (!IsDisposedOrDisposing && !Token.IsCancellationRequested && !CompleteMarked)
				{
					n = 0;

					while (!IsDisposedOrDisposing && !Token.IsCancellationRequested && !CompleteMarked && n < tasks.Length)
					{
						lock (_lock)
						{
							if (_queue.Count == 0)
							{
								Monitor.Wait(_lock, TimeSpanHelper.MINIMUM_SCHEDULE);
								Thread.Sleep(1);
							}

							if (IsDisposedOrDisposing || Token.IsCancellationRequested || _queue.Count == 0)
								continue;

							while (_queue.Count > 0 && n < tasks.Length && !IsDisposedOrDisposing && !Token.IsCancellationRequested && !CompleteMarked)
							{
								TaskItem item = _queue.Dequeue();
								if (item == null)
									continue;
								tasks[n++] = Task.Run(() => Run(item), Token).ConfigureAwait();
							}
						}
					}

					if (n < tasks.Length)
						break;
					if (IsDisposedOrDisposing || Token.IsCancellationRequested || !tasks.WaitSilently(Token))
						return;
				}

				while (!IsDisposedOrDisposing && !Token.IsCancellationRequested && _queue.Count > 0)
				{
					lock (_lock)
					{
						while (_queue.Count > 0 && n < tasks.Length && !IsDisposedOrDisposing && !Token.IsCancellationRequested)
						{
							TaskItem item = _queue.Dequeue();
							if (item == null)
								continue;
							tasks[n++] = Task.Run(() => Run(item), Token).ConfigureAwait();
						}
					}

					if (n < tasks.Length)
						break;
					if (IsDisposedOrDisposing || Token.IsCancellationRequested || !tasks.WaitSilently(Token))
						return;
					n = 0;
				}

				if (n > 0 && !IsDisposedOrDisposing && !Token.IsCancellationRequested)
				{
					Array.Resize(ref tasks, ++n);
					tasks.WaitSilently(Token);
				}
			}
			finally
			{
				_manualResetEventSlim.Set();
			}
		}

		private void Run([NotNull] TaskItem item)
		{
			if (IsDisposedOrDisposing || item.Token.IsCancellationRequested)
				return;

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

			if (IsDisposedOrDisposing || item.Token.IsCancellationRequested)
				return;

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
	}
}