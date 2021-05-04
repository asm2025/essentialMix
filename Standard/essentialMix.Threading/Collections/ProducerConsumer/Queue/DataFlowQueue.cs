using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.Collections.ProducerConsumer.Queue
{
	public sealed class DataFlowQueue<T> : ProducerConsumerThreadQueue<T>
	{
		private ManualResetEventSlim _manualResetEventSlim;
		private BufferBlock<T> _queue;
		private ActionBlock<T> _processor;
		private IDisposable _link;

		private Thread _worker;

		public DataFlowQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			_manualResetEventSlim = new ManualResetEventSlim(false);

			// Define the mesh.
			_queue = new BufferBlock<T>(new DataflowBlockOptions
			{
				CancellationToken = Token
			});
			_processor = new ActionBlock<T>(Run,
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
			ObjectHelper.Dispose(ref _manualResetEventSlim);
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
			_queue.Completion.ContinueWith(_ => _processor.Complete());
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
			_manualResetEventSlim.Reset();
			OnWorkStarted(EventArgs.Empty);

			try
			{
				while (!IsDisposed && !Token.IsCancellationRequested && !CompleteMarked)
					Task.WhenAll(_queue.Completion, _processor.Completion).Execute();
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
	}
}