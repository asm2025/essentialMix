using System.Collections.Generic;

namespace essentialMix.Collections
{
	public interface IReadOnlyKeyedCollection<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
	{
		IEqualityComparer<TKey> Comparer { get; }
		int IndexOfKey(TKey key);
		bool Contains(TKey key);
	}
}