using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Threading.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer.Queue
{
	public sealed class DataFlowQueue<T> : ProducerConsumerThreadQueue<T>
	{
		private readonly BufferBlock<T> _queue;
		private readonly ActionBlock<T> _processor;
		private IDisposable _link;
		private ManualResetEventSlim _workCompletedEventSlim;

		public DataFlowQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_workCompletedEventSlim = new ManualResetEventSlim(false);

			// Define the mesh.
			_queue = new BufferBlock<T>(new DataflowBlockOptions
			{
				CancellationToken = Token
			});
			_processor = new ActionBlock<T>(e =>
			{
				ScheduledCallback?.Invoke(e);
				Run(e);
			}, new ExecutionDataflowBlockOptions
			{
				MaxDegreeOfParallelism = Threads,
				CancellationToken = Token
			});
			_link = _queue.LinkTo(_processor, new DataflowLinkOptions
			{
				PropagateCompletion = true
			});
			TaskHelper.Run(Consume, TaskCreationOptions.LongRunning).ConfigureAwait();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;
			ObjectHelper.Dispose(ref _link);
			ObjectHelper.Dispose(ref _workCompletedEventSlim);
		}

		public override int Count => _queue.Count + _processor.InputCount;

		public override bool IsBusy => Count > 0;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
			_queue.SendAsync(item, Token);
		}

		protected override void CompleteInternal()
		{
			CompleteMarked = true;
			_queue.Complete();
		}

		protected override void ClearInternal()
		{
			_queue.Complete();
			_processor.Complete();
		}

		protected override bool WaitInternal(int millisecondsTimeout)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!IsBusy) return true;
			
			try
			{
				if (millisecondsTimeout < TimeSpanHelper.ZERO)
					_workCompletedEventSlim.Wait(Token);
				else if (!_workCompletedEventSlim.Wait(millisecondsTimeout, Token))
					return false;

				return true;
			}
			catch (OperationCanceledException)
			{
				// ignored
			}
			catch (TimeoutException)
			{
				// ignored
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

		[NotNull]
		private Task Consume()
		{
			if (IsDisposed) return Task.CompletedTask;
			_workCompletedEventSlim.Reset();
			OnWorkStarted(EventArgs.Empty);
			return Task.WhenAll(_queue.Completion, _processor.Completion)
						.ContinueWith(_ =>
						{
							OnWorkCompleted(EventArgs.Empty);
							_workCompletedEventSlim.Set();
						});
		}
	}
}