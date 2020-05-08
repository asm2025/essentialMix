using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using asm.Exceptions.Collections;

namespace asm.Collections
{
	/// <inheritdoc cref="IDictionary" />
	[Serializable]
	public class ProtectableDictionary : IDictionary, IProtectable
	{
		/// <inheritdoc />
		public ProtectableDictionary([NotNull] IDictionary source)
		{
			Source = source;
		}

		[NotNull]
		protected IDictionary Source { get; }

		/// <inheritdoc />
		public int Count => Source.Count;

		/// <inheritdoc />
		public object SyncRoot => Source.SyncRoot;

		/// <inheritdoc />
		public bool IsSynchronized => Source.IsSynchronized;

		/// <inheritdoc />
		public bool IsReadOnly => Source.IsReadOnly;

		/// <inheritdoc />
		public bool IsFixedSize => Source.IsFixedSize;

		/// <inheritdoc />
		[Browsable(false)]
		public bool IsProtected { get; set; }

		public object this[object key]
		{
			get => Source[key];
			set
			{
				if (IsProtected) throw new CollectionLockedException();
				Source[key] = value;
			}
		}

		/// <inheritdoc />
		public ICollection Keys => Source.Keys;

		/// <inheritdoc />
		public ICollection Values => Source.Values;

		/// <inheritdoc />
		public IDictionaryEnumerator GetEnumerator() { return Source.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { Source.CopyTo(array, index); }

		/// <inheritdoc />
		public bool Contains(object value) { return Source.Contains(value); }

		/// <inheritdoc />
		public void Add(object key, object value)
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.Add(key, value);
		}

		/// <inheritdoc />
		public void Remove(object key)
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.Remove(key);
		}

		/// <inheritdoc />
		public void Clear()
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.Clear();
		}
	}

	/// <inheritdoc cref="IDictionary{TKey,TValue}" />
	/// <inheritdoc cref="ProtectableDictionary" />
	[Serializable]
	public class ProtectableDictionary<TKey, TValue> : ProtectableDictionary, IDictionary<TKey, TValue>, IDictionary
	{
		[NonSerialized]
		private IDictionary<TKey, TValue> _source;

		public ProtectableDictionary([NotNull] IDictionary<TKey, TValue> source)
			: base(source as IDictionary ?? throw new ArgumentException("Source does not implement IDictionary.", nameof(source)))
		{
		}

		[NotNull]
		protected new IDictionary<TKey, TValue> Source => _source ??= (IDictionary<TKey, TValue>)base.Source;

		/// <inheritdoc />
		public TValue this[TKey key]
		{
			get => Source[key];
			set
			{
				if (IsProtected) throw new CollectionLockedException();
				Source[key] = value;
			}
		}

		object IDictionary.this[object key]
		{
			get => this[key];
			set => this[(TKey)key] = (TValue)value;
		}

		/// <inheritdoc />
		public new ICollection<TKey> Keys => Source.Keys;

		/// <inheritdoc />
		public new ICollection<TValue> Values => Source.Values;

		/// <inheritdoc />
		public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return Source.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { Source.CopyTo(array, arrayIndex); }

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int index) { base.CopyTo(array, index); }

		/// <inheritdoc />
		public bool ContainsKey(TKey key) { return Source.ContainsKey(key); }

		/// <inheritdoc />
		public bool Contains(KeyValuePair<TKey, TValue> item) { return Source.Contains(item); }

		/// <inheritdoc />
		bool IDictionary.Contains(object value) { return base.Contains(value); }

		/// <inheritdoc />
		public void Add(TKey key, TValue value)
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.Add(key, value);
		}

		/// <inheritdoc />
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.Add(item);
		}

		/// <inheritdoc />
		void IDictionary.Add(object key, object value)
		{
			base.Add(key, value);
		}

		/// <inheritdoc />
		public bool Remove(TKey key)
		{
			if (IsProtected) throw new CollectionLockedException();
			return Source.Remove(key);
		}

		/// <inheritdoc />
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			if (IsProtected) throw new CollectionLockedException();
			return Source.Remove(item);
		}

		/// <inheritdoc />
		void IDictionary.Remove(object key)
		{
			base.Remove(key);
		}

		/// <inheritdoc />
		public bool TryGetValue(TKey key, out TValue value) { return Source.TryGetValue(key, out value); }
	}
}