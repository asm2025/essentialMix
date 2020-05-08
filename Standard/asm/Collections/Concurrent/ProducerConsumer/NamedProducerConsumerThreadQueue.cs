using System;
using System.Threading;

namespace asm.Collections.Concurrent.ProducerConsumer
{
	public abstract class NamedProducerConsumerThreadQueue : ProducerConsumerThreadQueue, INamedProducerConsumerThreadQueue, IDisposable
	{
		private bool _isOwner;
		protected NamedProducerConsumerThreadQueue(CancellationToken token = default(CancellationToken))
			: this(null, token)
		{
		}

		protected NamedProducerConsumerThreadQueue(ProducerConsumerQueueOptions options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			if (options is ProducerConsumerThreadNamedQueueOptions namedQueueOptions)
			{
				Name = namedQueueOptions.Name;
			}
		}

		public string Name { get; }

		public bool IsOwner
		{
			get => _isOwner;
			protected set
			{
				ThrowIfDisposed();
				_isOwner = value;
			}
		}
	}
}