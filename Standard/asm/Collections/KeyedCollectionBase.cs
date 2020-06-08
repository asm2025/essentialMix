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
	public abstract class KeyedCollectionBase<TKey, TValue> : System.Collections.ObjectModel.KeyedCollection<TKey, TValue>, IReadOnlyKeyedCollection<TKey, TValue>, IReadOnlyList<TValue>, IReadOnlyCollection<TValue>, IList<TValue>, IList, ISerializable, IDeserializationCallback
	{
		[NonSerialized]
		private SerializationInfo _siInfo;

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

		protected KeyedCollectionBase(SerializationInfo info, StreamingContext context)
		{
			_siInfo = info;
		}

		public bool IsFixedSize => false;

		public bool IsReadOnly => false;

		public IEnumerable<TKey> Keys => Dictionary?.Keys ?? Array.Empty<TKey>();

		public IEnumerable<TValue> Values => Items;

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