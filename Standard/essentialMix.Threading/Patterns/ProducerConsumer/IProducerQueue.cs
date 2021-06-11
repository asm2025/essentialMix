using System;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public interface IProducerQueue<T>
	{
		bool TryDequeue(out T item);
		bool TryPeek(out T item);
		void RemoveWhile([NotNull] Predicate<T> predicate);
	}
}
