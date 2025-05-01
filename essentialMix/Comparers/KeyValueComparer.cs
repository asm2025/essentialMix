using System.Collections.Generic;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Comparers;

public class KeyValueComparer<TKey, TValue>(IGenericComparer<TKey> keyComparer, IGenericComparer<TValue> valueComparer)
	: IKeyValueComparer<TKey, TValue>
{
	public static IKeyValueComparer<TKey, TValue> Default { get; } = new KeyValueComparer<TKey, TValue>();

	public KeyValueComparer()
		: this(null, null) { }

	[NotNull]
	public IGenericComparer<TKey> KeyComparer { get; } = keyComparer ?? GenericComparer<TKey>.Default;

	[NotNull]
	public IGenericComparer<TValue> ValueComparer { get; } = valueComparer ?? GenericComparer<TValue>.Default;

	public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
	{
		return KeyComparer.Compare(x.Key, y.Key).IfEqual(0, ValueComparer.Compare(x.Value, y.Value));
	}

	public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) { return KeyComparer.Equals(x.Key, y.Key) && ValueComparer.Equals(x.Value, y.Value); }

	public int GetHashCode(KeyValuePair<TKey, TValue> obj) { return obj.GetHashCode(); }

	public int Compare(object x, object y) { return ReferenceComparer.Default.Compare(x, y); }

	public new bool Equals(object x, object y) { return ReferenceComparer.Default.Equals(x, y); }

	public int GetHashCode(object obj) { return ReferenceComparer.Default.GetHashCode(obj); }
}