using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public interface IQueue<T>
	{
		void Enqueue([NotNull] T item);
		void Dequeue();
	}
}