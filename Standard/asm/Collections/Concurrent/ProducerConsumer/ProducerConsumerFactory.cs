using System;
using System.Threading;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public static class ProducerConsumerFactory
	{
		[NotNull] public static IProducerConsumer Create(ThreadQueueMode mode, CancellationToken token = default(CancellationToken)) { return Create(mode, new ProducerConsumerQueueOptions(), token); }

		[NotNull]
		public static IProducerConsumer Create(ThreadQueueMode mode, ProducerConsumerQueueOptions options, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();

			switch (mode)
			{
				case ThreadQueueMode.task:
					return new TaskQueue(options, token);
				case ThreadQueueMode.dataflow:
					return new DataFlowQueue(options, token);
				case ThreadQueueMode.waitpulse:
					return new WaitAndPulseQueue(options, token);
				case ThreadQueueMode.blockingCollection:
					return new BlockingCollectionQueue(options, token);
				case ThreadQueueMode.taskGroup:
					return new TaskGroupQueue(options, token);
				case ThreadQueueMode.semaphoreslim:
					return new SemaphoreSlimQueue(options, token);
				case ThreadQueueMode.semaphore:
					return new SemaphoreQueue(options, token);
				case ThreadQueueMode.thresholdTaskGroup:
					return new ThresholdTaskGroupQueue(options, token);
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}
	}
}