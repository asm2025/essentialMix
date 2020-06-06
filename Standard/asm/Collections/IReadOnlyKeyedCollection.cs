using System.Collections.Generic;

namespace asm.Collections
{
	public interface IReadOnlyKeyedCollection<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
	{
		IEqualityComparer<TKey> Comparer { get; }
		int IndexOfKey(TKey key);
		bool Contains(TKey key);
	}
}