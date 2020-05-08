using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public sealed class DataFlowQueue : ProducerConsumerThreadQueue, IDisposable
	{
		private readonly object _lock = new object();
		private ManualResetEventSlim _manualResetEventSlim;
		private BufferBlock<TaskItem> _queue;
		private ActionBlock<TaskItem> _processor;
		private IDisposable _link;

		private Thread _worker;

		public DataFlowQueue(CancellationToken token = default(CancellationToken))
			: this(null, token)
		{
		}

		public DataFlowQueue(ProducerConsumerQueueOptions options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_manualResetEventSlim = new ManualResetEventSlim(false);

			// Define the mesh.
			_queue = new BufferBlock<TaskItem>(new DataflowBlockOptions
			{
				CancellationToken = Token
			});
			_processor = new ActionBlock<TaskItem>(Run,
				new ExecutionDataflowBlockOptions
				{
					MaxDegreeOfParallelism = Threads,
					CancellationToken = Token
				});
			_link = _queue.LinkTo(_processor, new DataflowLinkOptions
			{
				PropagateCompletion = false
			});

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
			ObjectHelper.Dispose(ref _link);
			ObjectHelper.Dispose(ref _processor);
			ObjectHelper.Dispose(ref _queue);
			ObjectHelper.Dispose(ref _worker);
			ObjectHelper.Dispose(ref _manualResetEventSlim);
		}

		protected override int CountInternal
		{
			get
			{
				lock(_lock)
				{
					return _queue.Count + _processor.InputCount;
				}
			}
		}

		protected override bool IsBusyInternal => CountInternal > 0;

		protected override void EnqueueInternal(TaskItem item)
		{
			if (IsDisposedOrDisposing || Token.IsCancellationRequested) return;

			lock (_lock)
			{
				item.Token = Token;
				_queue.SendAsync(item, Token);
				Monitor.Pulse(_lock);
			}
		}

		protected override void CompleteInternal()
		{
			lock(_lock)
			{
				CompleteMarked = true;
				_queue.Complete();
				_queue.Completion.ContinueWith(_ => _processor.Complete());
				Monitor.PulseAll(_lock);
			}
		}

		protected override void ClearInternal()
		{
			lock (_lock)
			{
				_queue.Complete();
				_processor.Complete();
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
					_manualResetEventSlim.Wait(Token);
				else if (!_manualResetEventSlim.Wait(millisecondsTimeout, Token))
					return false;

				return true;
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
				_queue.Complete();
				_processor.Complete();
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
					Task.WhenAll(_queue.Completion, _processor.Completion).ConfigureAwait().Wait(Token);
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
			finally
			{
				OnWorkCompleted(EventArgs.Empty);
				_manualResetEventSlim.Set();
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
	}
}