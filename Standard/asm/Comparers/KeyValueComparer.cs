using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Comparers
{
	public class KeyValueComparer<TKey, TValue> : IKeyValueComparer<TKey, TValue>
	{
		public static IKeyValueComparer<TKey, TValue> Default { get; } = new KeyValueComparer<TKey, TValue>();

		public KeyValueComparer()
			: this(null, null) { }

		public KeyValueComparer(IGenericComparer<TKey> keyComparer, IGenericComparer<TValue> valueComparer)
		{
			KeyComparer = keyComparer ?? GenericComparer<TKey>.Default;
			ValueComparer = valueComparer ?? GenericComparer<TValue>.Default;
		}

		[NotNull]
		public IGenericComparer<TKey> KeyComparer { get; }

		[NotNull]
		public IGenericComparer<TValue> ValueComparer { get; }

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
}