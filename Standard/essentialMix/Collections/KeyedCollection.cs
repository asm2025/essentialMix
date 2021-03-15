using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public class KeyedCollection<TKey, TValue> : System.Collections.ObjectModel.KeyedCollection<TKey, TValue>, IReadOnlyKeyedCollection<TKey, TValue>, IReadOnlyList<TValue>, IReadOnlyCollection<TValue>, IList<TValue>, IList
	{
		[NotNull]
		protected readonly Func<TValue, TKey> GetKey;

		public KeyedCollection([NotNull] Func<TValue, TKey> getKey)
			: this(getKey, null, 0)
		{
		}

		public KeyedCollection([NotNull] Func<TValue, TKey> getKey, IEqualityComparer<TKey> comparer)
			: this(getKey, comparer, 0)
		{
		}

		/// <inheritdoc />
		public KeyedCollection([NotNull] Func<TValue, TKey> getKey, IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
			: base(comparer ?? EqualityComparer<TKey>.Default, dictionaryCreationThreshold)
		{
			GetKey = getKey;
		}

		public KeyedCollection([NotNull] Func<TValue, TKey> getKey, [NotNull] IEnumerable<TValue> collection)
			: this(getKey, collection, null)
		{
		}

		public KeyedCollection([NotNull] Func<TValue, TKey> getKey, [NotNull] IEnumerable<TValue> collection, IEqualityComparer<TKey> comparer)
			: base(comparer ?? EqualityComparer<TKey>.Default)
		{
			GetKey = getKey;
		
			foreach (TValue value in collection) 
				Add(value);
		}

		public bool IsFixedSize => false;

		public bool IsReadOnly => false;

		public IEnumerable<TKey> Keys => Dictionary?.Keys ?? Array.Empty<TKey>();

		public IEnumerable<TValue> Values => Items;

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return Dictionary?.GetEnumerator() ?? Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();
		}

		void IList.Remove([NotNull] object key) { Remove((TKey)key); }

		public bool ContainsKey([NotNull] TKey key) { return Contains(key); }

		bool IList.Contains(object key) { return key is TKey k && Contains(k); }

		public void MoveItem(int index, int newIndex)
		{
			TValue value = Items[index];
			base.RemoveItem(index);
			base.InsertItem(newIndex, value);
		}

		public int IndexOfKey(TKey key)
		{
			int index = -1;
			if (Items.Count < 1) return index;

			for (int i = 0; i < Items.Count; i++)
			{
				if (!Comparer.Equals(key, GetKeyForItem(Items[i]))) continue;
				index = i;
				break;
			}

			return index;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			if (Dictionary != null) return Dictionary.TryGetValue(key, out value);
			value = default(TValue);
			return false;
		}

		/// <inheritdoc />
		protected sealed override TKey GetKeyForItem(TValue item) { return GetKey(item); }
	}
}