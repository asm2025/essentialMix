using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public abstract class NamedProducerConsumerThreadQueue<T> : ProducerConsumerThreadQueue<T>, INamedProducerConsumerThreadQueue<T>
	{
		private bool _isOwner;

		protected NamedProducerConsumerThreadQueue([NotNull] ProducerConsumerQueueOptions<T> options, CancellationToken token = default(CancellationToken))
			: base(options, token)
		{
			if (options is not ProducerConsumerThreadNamedQueueOptions<T> namedQueueOptions) return;
			Name = namedQueueOptions.Name;
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