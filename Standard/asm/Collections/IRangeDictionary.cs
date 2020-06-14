using System;

namespace asm.Collections
{
	public interface IRangeDictionary<TKey, TValue> : IReadOnlyRangeDictionary<TKey, TValue>
		where TKey : IComparable
	{
#pragma warning disable 109
		new TValue this[TKey key] { get; set; }
#pragma warning restore 109

		void Add(TKey minimum, TKey maximum, TValue value);

		void Remove(TKey minimum, TKey maximum);
	}
}