using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class ReadOnlyKeyedCollection<TKey, TValue> : IReadOnlyKeyedCollection<TKey, TValue>
	{
		public ReadOnlyKeyedCollection([NotNull] IKeyedCollection<TKey, TValue> collection)
		{
			Collection = collection ?? throw new ArgumentNullException(nameof(collection));
		}

		public TValue this[int index] => ((IReadOnlyList<TValue>)Collection)[index];

		public TValue this[TKey key] => ((IReadOnlyDictionary<TKey, TValue>)Collection)[key];

		public IEnumerable<TKey> Keys => ((IDictionary<TKey, TValue>)Collection).Keys;

		public IEnumerable<TValue> Values => ((IDictionary<TKey, TValue>)Collection).Values;

		public IEqualityComparer<TKey> Comparer => Collection.Comparer;

		protected IKeyedCollection<TKey, TValue> Collection { get; }

		public int Count => ((IReadOnlyCollection<TValue>)Collection).Count;

		int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => ((IReadOnlyCollection<KeyValuePair<TKey, TValue>>)Collection).Count;

		public bool ContainsKey([NotNull] TKey key) { return ((IDictionary<TKey, TValue>)Collection).ContainsKey(key); }

		public bool TryGetValue(TKey key, out TValue value) { return ((IDictionary<TKey, TValue>)Collection).TryGetValue(key, out value); }

		public int IndexOfKey(TKey key) { return Collection.IndexOfKey(key); }

		public bool Contains(TKey key) { return Collection.Contains(key); }

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<TKey, TValue>>)Collection).GetEnumerator();
		}

		IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() { return ((IEnumerable<TValue>)Collection).GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable)Collection).GetEnumerator(); }
	}
}