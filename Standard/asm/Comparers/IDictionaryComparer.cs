using System.Collections.Generic;

namespace asm.Comparers
{
	public interface IDictionaryComparer<TKey, TValue> : IGenericComparer<IDictionary<TKey, TValue>>
	{
		IKeyValueComparer<TKey, TValue> Comparer { get; }
	}
}