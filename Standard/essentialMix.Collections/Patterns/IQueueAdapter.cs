using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections.Patterns
{
	public interface IQueueAdapter<TCollection, T> : IQueue<T>
		where TCollection : ICollection, IReadOnlyCollection<T>
	{
		[NotNull]
		TCollection Collection { get; }
	}
}