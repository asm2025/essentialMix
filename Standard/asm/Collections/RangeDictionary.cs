using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class RangeDictionary<TKey, TValue> : ReadOnlyRangeDictionary<TKey, TValue>,
		IRangeDictionary<TKey, TValue>,
		IDictionary
		where TKey : IComparable
	{
		/// <inheritdoc />
		public RangeDictionary() 
			: this(0, null)
		{
		}

		/// <inheritdoc />
		public RangeDictionary(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		public RangeDictionary([NotNull] IDictionary<(TKey Minimum, TKey Maximum), TValue> dictionary)
			: this(0, dictionary)
		{
		}

		/// <inheritdoc />
		protected RangeDictionary(int capacity, IDictionary<(TKey Minimum, TKey Maximum), TValue> dictionary)
			: base(capacity, dictionary)
		{
		}

		/// <inheritdoc />
		protected RangeDictionary(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc cref="Dictionary{TKey,TValue}" />
		public override TValue this[(TKey Minimum, TKey Maximum) key]
		{
			get => Dictionary[key];
			set => Dictionary[key] = value;
		}

		/// <inheritdoc cref="Dictionary{TKey,TValue}" />
		public override TValue this[TKey key]
		{
			get => Dictionary[GetKey(key)];
			set => Dictionary[GetKey(key)] = value;
		}

		/// <inheritdoc />
		object IDictionary.this[object key]
		{
			get => this[((TKey Minimum, TKey Maximum))key];
			set => this[((TKey Minimum, TKey Maximum))key] = (TValue)value;
		}

		/// <inheritdoc />
		bool IDictionary.IsFixedSize => DictionaryAsDictionary.IsFixedSize;

		/// <inheritdoc />
		bool IDictionary.IsReadOnly => DictionaryAsGenericCollection.IsReadOnly;

		/// <inheritdoc />
		bool ICollection<KeyValuePair<(TKey Minimum, TKey Maximum), TValue>>.IsReadOnly => DictionaryAsGenericCollection.IsReadOnly;

		/// <inheritdoc />
		public ICollection<(TKey Minimum, TKey Maximum)> Keys => Dictionary.Keys;

		/// <inheritdoc />
		ICollection IDictionary.Keys => DictionaryAsDictionary.Keys;

		/// <inheritdoc />
		ICollection IDictionary.Values => DictionaryAsDictionary.Values;

		/// <inheritdoc />
		public ICollection<TValue> Values => Dictionary.Values;

		/// <inheritdoc />
		IDictionaryEnumerator IDictionary.GetEnumerator() { return DictionaryAsDictionary.GetEnumerator(); }

		/// <inheritdoc />
		public virtual void Add((TKey Minimum, TKey Maximum) key, TValue value)
		{
			if (Comparer.Compare(key.Minimum, key.Maximum) > 0) throw new InvalidOperationException($"{nameof(key.Minimum)} cannot be greater than {nameof(key.Maximum)}.");
			Dictionary.Add(key, value);
		}

		/// <inheritdoc />
		void ICollection<KeyValuePair<(TKey Minimum, TKey Maximum), TValue>>.Add(KeyValuePair<(TKey Minimum, TKey Maximum), TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public void Add(TKey minimum, TKey maximum, TValue value)
		{
			Add((minimum, maximum), value);
		}

		/// <inheritdoc />
		void IDictionary.Add(object key, object value)
		{
			(TKey Minimum, TKey Maximum) tuple = ((TKey Minimum, TKey Maximum))key;
			Add(tuple, (TValue)value);
		}

		/// <inheritdoc />
		public virtual bool Remove((TKey Minimum, TKey Maximum) key) { return Dictionary.Remove(key); }

		public void Remove(TKey minimum, TKey maximum)
		{
			Remove((minimum, maximum));
		}

		/// <inheritdoc />
		bool ICollection<KeyValuePair<(TKey Minimum, TKey Maximum), TValue>>.Remove(KeyValuePair<(TKey Minimum, TKey Maximum), TValue> item) { return Remove(item.Key); }

		/// <inheritdoc />
		void IDictionary.Remove(object key)
		{
			Remove(((TKey Minimum, TKey Maximum))key);
		}

		/// <inheritdoc cref="Dictionary{TKey,TValue}" />
		public virtual void Clear() { Dictionary.Clear(); }

		/// <inheritdoc />
		bool ICollection<KeyValuePair<(TKey Minimum, TKey Maximum), TValue>>.Contains(KeyValuePair<(TKey Minimum, TKey Maximum), TValue> item) { return ((ICollection<KeyValuePair<(TKey Minimum, TKey Maximum), TValue>>)Dictionary).Contains(item); }

		/// <inheritdoc />
		bool IDictionary.Contains(object key) { return ContainsKey(((TKey Minimum, TKey Maximum))key); }

		/// <inheritdoc />
		public void CopyTo(KeyValuePair<(TKey Minimum, TKey Maximum), TValue>[] array, int arrayIndex) { DictionaryAsGenericCollection.CopyTo(array, arrayIndex); }
	}
}