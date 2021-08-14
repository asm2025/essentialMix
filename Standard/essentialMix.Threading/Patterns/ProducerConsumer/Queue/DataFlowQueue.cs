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
		private readonly object _lock = new object();

		private BufferBlock<T> _queue;
		private ActionBlock<T> _processor;
		private IDisposable _link;

		public DataFlowQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!disposing) return;
			ObjectHelper.Dispose(ref _link);
		}

		/// <inheritdoc />
		public override int Count => _queue.Count + Running;

		/// <inheritdoc />
		public override bool IsEmpty => _queue.Count == 0;

		/// <inheritdoc />
		public override bool CanPause => false;

		protected override void EnqueueInternal(T item)
		{
			if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;

			bool invokeWorkStarted = false;

			if (!WaitForWorkerStart())
			{
				lock(_lock)
				{
					if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;

					if (!WaitForWorkerStart())
					{
						InitializeMesh();
						InitializeWorkerStart();
						InitializeWorkersCountDown(1);
						InitializeBatchClear();
						InitializeTaskStart();
						InitializeTaskComplete();
						InitializeTasksCountDown();

						new Thread(Consume)
						{
							IsBackground = IsBackground,
							Priority = Priority
						}.Start();

						invokeWorkStarted = true;
						if (!WaitForWorkerStart()) throw new TimeoutException();
						if (IsDisposed || Token.IsCancellationRequested || CompleteMarked) return;
					}
				}

				if (invokeWorkStarted) WorkStartedCallback?.Invoke(this);
			}

			_queue.Post(item);
			if (!invokeWorkStarted) return;
			WaitForTaskStart();
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

		private void InitializeMesh()
		{
			_queue = new BufferBlock<T>(new DataflowBlockOptions
			{
				CancellationToken = Token
			});
			_processor = new ActionBlock<T>(item =>
			{
				if (ScheduledCallback != null && !ScheduledCallback(item)) return;
				AddTasksCountDown();
				Run(item);
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
		}

		private void Consume()
		{
			if (IsDisposed || Token.IsCancellationRequested) return;
			SignalWorkerStart();
			Task.WhenAll(_queue.Completion.ConfigureAwait(), _processor.Completion.ConfigureAwait())
				.ConfigureAwait()
				.ContinueWith(_ => SignalWorkersCountDown())
				.Execute();
		}

		/// <inheritdoc />
		protected override void Run(T item)
		{
			try
			{
				SignalTaskStart();
				if (IsDisposed || Token.IsCancellationRequested) return;
				base.Run(item);
			}
			finally
			{
				SignalTaskComplete();
				SignalTasksCountDown();
			}
		}
	}
}