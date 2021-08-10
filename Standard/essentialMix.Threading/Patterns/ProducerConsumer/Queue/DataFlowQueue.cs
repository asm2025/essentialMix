using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using essentialMix.Extensions;
using essentialMix.Helpers;
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
				if (IsPaused)
				{
					CancellationToken tkn = Token;
					SpinWait.SpinUntil(() => IsDisposed || tkn.IsCancellationRequested || !IsPaused);
					if (IsDisposed || tkn.IsCancellationRequested) return;
				}
				ScheduledCallback?.Invoke(e);
				Run(e);
			}, new ExecutionDataflowBlockOptions
			{
				// SingleProducerConstrained = false by default therefore, BufferBlock<T>.SendAsync is thread safe
				MaxDegreeOfParallelism = Threads,
				CancellationToken = Token
			});
			_link = _queue.LinkTo(_processor, new DataflowLinkOptions
			{
				PropagateCompletion = true
			});
			new Thread(Consume)
			{
				IsBackground = IsBackground,
				Priority = Priority
			}.Start();
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
			_queue.Post(item);
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

		private void Consume()
		{
			if (IsDisposed) return;
			_workCompletedEventSlim.Reset();
			OnWorkStarted(EventArgs.Empty);
			Task.WhenAll(_queue.Completion.ConfigureAwait(), _processor.Completion.ConfigureAwait())
						.ContinueWith(_ =>
						{
							OnWorkCompleted(EventArgs.Empty);
							_workCompletedEventSlim.Set();
						})
						.ConfigureAwait()
						.GetAwaiter()
						.GetResult();
		}
	}
}