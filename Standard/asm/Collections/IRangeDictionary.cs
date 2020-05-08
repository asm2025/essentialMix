using System;
using System.Collections.Generic;

namespace asm.Collections
{
	public interface IRangeDictionary<TKey, TValue> : IReadOnlyRangeDictionary<TKey, TValue>,
		IDictionary<(TKey Minimum, TKey Maximum), TValue>,
		ICollection<KeyValuePair<(TKey Minimum, TKey Maximum), TValue>>
		where TKey : IComparable
	{
		new TValue this[TKey key] { get; set; }

		void Add(TKey minimum, TKey maximum, TValue value);

		void Remove(TKey minimum, TKey maximum);
	}
}