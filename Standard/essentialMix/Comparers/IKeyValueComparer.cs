using System.Collections.Generic;

namespace essentialMix.Comparers
{
	public interface IKeyValueComparer<TKey, TValue> : IGenericComparer<KeyValuePair<TKey, TValue>>
	{
		IGenericComparer<TKey> KeyComparer { get; }
		IGenericComparer<TValue> ValueComparer { get; }
	}
}