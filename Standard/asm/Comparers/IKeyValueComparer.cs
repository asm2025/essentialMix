using System.Collections.Generic;

namespace asm.Comparers
{
	public interface IKeyValueComparer<TKey, TValue> : IGenericComparer<KeyValuePair<TKey, TValue>>
	{
		IGenericComparer<TKey> KeyComparer { get; }
		IGenericComparer<TValue> ValueComparer { get; }
	}
}