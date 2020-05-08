using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public abstract class KeyedCollection<TKey, TValue> : System.Collections.ObjectModel.KeyedCollection<TKey, TValue>, IKeyedCollection<TKey, TValue>
	{
		[NonSerialized]
		private SerializationInfo _siInfo;

		protected KeyedCollection()
		{
		}

		protected KeyedCollection(IEqualityComparer<TKey> comparer)
			: this(comparer, 0)
		{
		}

		/// <inheritdoc />
		protected KeyedCollection(IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
			: base(comparer ?? EqualityComparer<TKey>.Default, dictionaryCreationThreshold)
		{
		}

		protected KeyedCollection([NotNull] IEnumerable<TValue> collection)
			: this(collection, null)
		{
		}

		protected KeyedCollection([NotNull] IEnumerable<TValue> collection, IEqualityComparer<TKey> comparer)
			: base(comparer ?? EqualityComparer<TKey>.Default)
		{
			if (collection == null) throw new ArgumentNullException(nameof(collection));
			
			foreach (TValue value in collection) 
				base.Add(value);
		}

		protected KeyedCollection(SerializationInfo info, StreamingContext context) { _siInfo = info; }

		protected override void InsertItem(int index, TValue item)
		{
			OnInserting(index, item);
			base.InsertItem(index, item);
			OnInserted(index, item);
		}

		protected override void SetItem(int index, TValue item)
		{
			OnUpdating(index, item);
			base.SetItem(index, item);
			OnUpdated(index, item);
		}

		protected override void RemoveItem(int index)
		{
			TValue value = Items[index];
			OnRemoving(index, value);
			base.RemoveItem(index);
			OnRemoved(index, value);
		}

		protected override void ClearItems()
		{
			if (Count == 0) return;
			OnClearing();
			base.ClearItems();
			OnCleared();
		}

		public new virtual TValue this[TKey key]
		{
			get => base[key];
			set
			{
				int index = IndexOfKey(key);
				if (index > -1) SetItem(index, value);
				else Add(value);
			}
		}

		object IDictionary.this[object key]
		{
			get => this[(TKey)key];
			set => this[(TKey)key] = (TValue)value;
		}

		public ICollection<TKey> Keys => Dictionary?.Keys ?? Array.Empty<TKey>();

		public ICollection<TValue> Values => Dictionary?.Values ?? Array.Empty<TValue>();

		public bool IsFixedSize => false;

		public bool IsReadOnly => false;

		ICollection IDictionary.Keys => (ICollection)Keys;

		ICollection IDictionary.Values => (ICollection)Values;

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

		public new virtual void Add(TValue item)
		{
			base.Add(item);
		}

		public new virtual void Insert(int index, TValue item)
		{
			base.Insert(index, item);
		}

		public virtual void MoveItem(int index, int newIndex)
		{
			TValue value = Items[index];
			OnMoving(index, newIndex, value);
			base.RemoveItem(index);
			base.InsertItem(newIndex, value);
			OnMoved(index, newIndex, value);
		}

		public new virtual bool Remove(TKey key)
		{
			if (!base.Remove(key)) return false;
			return true;
		}

		public new virtual bool Remove(TValue item) { return base.Remove(item); }

		public new virtual void RemoveAt(int index)
		{
			base.RemoveAt(index);
		}

		public new virtual void Clear()
		{
			base.Clear();
		}

		[SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		[SecurityCritical]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null) throw new ArgumentNullException(nameof(info));
			info.AddValue("Comparer", Comparer, typeof(IEqualityComparer<TKey>));
			info.AddValue("Count", Count);
			if (Count == 0) return;
			TValue[] array = new TValue[Count];
			Items.CopyTo(array, 0);
			info.AddValue("Values", array, typeof(TValue[]));
		}

		public virtual void OnDeserialization(object sender)
		{
			if (_siInfo == null) return;

			IEqualityComparer<TKey> comparer = (IEqualityComparer<TKey>)_siInfo.GetValue("Comparer", typeof(IEqualityComparer<TKey>));
			if (comparer != null) this.SetFieldValue("comparer", comparer);

			int count = _siInfo.GetInt32("Count");

			if (count > 0)
			{
				TValue[] array = (TValue[])_siInfo.GetValue("Values", typeof(TValue[]));

				if (array != null)
				{
					foreach (TValue value in array)
					{
						Dictionary.Add(GetKeyForItem(value), value);
						Items.Add(value);
					}
				}
			}

			_siInfo = null;
		}

		public virtual int IndexOfKey(TKey key)
		{
			int index = -1;

			if (Items.Count > 0)
			{
				for (int i = 0; i < Items.Count; i++)
				{
					if (!Comparer.Equals(key, GetKeyForItem(Items[i]))) continue;
					index = i;
					break;
				}
			}

			return index;
		}

		public virtual bool TryGetValue(TKey key, out TValue value)
		{
			if (Dictionary == null)
			{
				value = default(TValue);
				return false;
			}

			return Dictionary.TryGetValue(key, out value);
		}

		public void Add(TKey key, [NotNull] TValue value)
		{
			TKey k = GetKeyForItem(value);
			if (!Comparer.Equals(key, k)) throw new ArgumentException("Key mismatch.");
			Add(value);
		}

		protected virtual void OnInserting(int index, TValue item) { }
		protected virtual void OnInserted(int index, TValue item) { }
		protected virtual void OnUpdating(int index, TValue item) { }
		protected virtual void OnUpdated(int index, TValue item) { }
		protected virtual void OnRemoving(int index, TValue item) { }
		protected virtual void OnRemoved(int index, TValue item) { }
		protected virtual void OnMoving(int index, int newIndex, TValue item) { }
		protected virtual void OnMoved(int index, int newIndex, TValue item) { }
		protected virtual void OnClearing() { }
		protected virtual void OnCleared() { }

		bool IDictionary<TKey, TValue>.ContainsKey(TKey key) { return Contains(key); }

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) { Add(item.Key, item.Value); }

		bool IDictionary.Contains(object key) { return Contains((TKey)key); }

		void IDictionary.Add(object key, [NotNull] object value) { Add((TKey)key, (TValue)value); }

		IDictionaryEnumerator IDictionary.GetEnumerator() { return ((IDictionary)this).Enumerate(); }

		void IDictionary.Remove(object key) { Remove((TKey)key); }

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) { return Dictionary?.Contains(item) == true; }

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { Dictionary?.CopyTo(array, arrayIndex); }

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) { return Dictionary?.Remove(item) == true; }

		void IList.Remove([NotNull] object key) { Remove((TKey)key); }
		bool IReadOnlyDictionary<TKey, TValue>.ContainsKey([NotNull] TKey key) { return Contains(key); }

		bool IList.Contains(object key) { return key is TKey k && Contains(k); }

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return Dictionary?.GetEnumerator() ?? Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();
		}
	}
}