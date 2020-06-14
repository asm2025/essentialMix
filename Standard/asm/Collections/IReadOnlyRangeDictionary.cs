using System;
using System.Collections.Generic;

namespace asm.Collections
{
	public interface IReadOnlyRangeDictionary<TKey, TValue> : IReadOnlyDictionary<(TKey Minimum, TKey Maximum), TValue>
		where TKey : IComparable
	{
		RangeDictionaryComparer<TKey> Comparer { get; }

		TValue this[TKey key] { get; }

		bool ContainsKey(TKey key);

		(TKey Minimum, TKey Maximum) GetKey(TKey key);

		bool TryGetKey(TKey lookup, out (TKey Minimum, TKey Maximum) key);

		bool TryGetValue(TKey key, out TValue value);
	}
}