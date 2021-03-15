using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public abstract class KeyedCollectionBase<TKey, TValue> : System.Collections.ObjectModel.KeyedCollection<TKey, TValue>, IReadOnlyKeyedCollection<TKey, TValue>, IReadOnlyList<TValue>, IReadOnlyCollection<TValue>, IList<TValue>, IList
	{
		protected KeyedCollectionBase()
			: this(null, 0)
		{
		}

		protected KeyedCollectionBase(IEqualityComparer<TKey> comparer)
			: this(comparer, 0)
		{
		}

		/// <inheritdoc />
		protected KeyedCollectionBase(IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
			: base(comparer ?? EqualityComparer<TKey>.Default, dictionaryCreationThreshold)
		{
		}

		protected KeyedCollectionBase([NotNull] IEnumerable<TValue> collection)
			: this(collection, null)
		{
		}

		protected KeyedCollectionBase([NotNull] IEnumerable<TValue> collection, IEqualityComparer<TKey> comparer)
			: base(comparer ?? EqualityComparer<TKey>.Default)
		{
			if (collection == null) throw new ArgumentNullException(nameof(collection));
			
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

		public virtual void MoveItem(int index, int newIndex)
		{
			TValue value = Items[index];
			base.RemoveItem(index);
			base.InsertItem(newIndex, value);
		}

		public int IndexOfKey(TKey key)
		{
			if (Dictionary == null) return -1;

			int index = -1, i = -1;

			foreach (TKey k in Dictionary.Keys)
			{
				i++;
				if (!Comparer.Equals(key, k)) continue;
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
	}
}