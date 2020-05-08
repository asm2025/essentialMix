using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	public class DictionaryTypeDescriptor<TSource> : TypeDescriptorBase<TSource>, IDictionary
		where TSource : IDictionary
	{
		public DictionaryTypeDescriptor([NotNull] TSource source)
			: base(source)
		{
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties()
		{
			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);
			ForEach(entry => pds.Add(new DictionaryTypePropertyDescriptor<TSource>(this, entry.Key)));
			return pds;
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties([NotNull] Attribute[] attributes)
		{
			if (attributes == null) throw new ArgumentNullException(nameof(attributes));
			if (attributes.Length == 0) return GetProperties();

			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);
			ForEach(entry =>
			{
				if (entry.Value is ICustomAttributeProvider provider)
				{
					Attribute[] attr = provider.GetAttributes(true).ToArray();
					if (!attr.Contains(attributes)) return;
				}

				pds.Add(new DictionaryTypePropertyDescriptor<TSource>(this, entry.Key));
			});

			return pds;
		}

		public bool Contains(object key) { return Source.Contains(key); }

		public void Add(object key, object value) { Source.Add(key, value); }

		public void Clear() { Source.Clear(); }

		public IDictionaryEnumerator GetEnumerator() { return Source.GetEnumerator(); }

		public void Remove(object key) { Source.Remove(key); }

		[SuppressMessage("ReSharper", "PossibleStructMemberModificationOfNonVariableStruct")]
		public object this[object key]
		{
			get => Source[key];
			set => Source[key] = value;
		}

		public ICollection Keys => Source.Keys;

		public ICollection Values => Source.Values;

		public bool IsReadOnly => Source.IsReadOnly;

		public bool IsFixedSize => Source.IsFixedSize;

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public void CopyTo(Array array, int index) { Source.CopyTo(array, index); }
		public int Count => Source.Count;

		public object SyncRoot => Source.SyncRoot;

		public bool IsSynchronized => Source.IsSynchronized;

		public void ForEach([NotNull] Action<DictionaryEntry> action) { Source.Cast<DictionaryEntry>().ForEach(action); }

		public void ForEach([NotNull] Func<DictionaryEntry, bool> action) { Source.Cast<DictionaryEntry>().ForEach(action); }

		public void ForEach([NotNull] Action<DictionaryEntry, int> action) { Source.Cast<DictionaryEntry>().ForEach(action); }

		public void ForEach([NotNull] Func<DictionaryEntry, int, bool> action) { Source.Cast<DictionaryEntry>().ForEach(action); }
	}

	public class DictionaryTypeDescriptor<TSource, TKey, TValue> : TypeDescriptorBase<TSource>, IDictionary<TKey, TValue>, IDictionary
		where TSource : IDictionary<TKey, TValue>, IDictionary
	{
		public DictionaryTypeDescriptor([NotNull] TSource source)
			: base(source)
		{
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties()
		{
			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);
			ForEach(pair => pds.Add(new DictionaryTypePropertyDescriptor<TSource, TKey, TValue>(this, pair.Key)));
			return pds;
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties([NotNull] Attribute[] attributes)
		{
			if (attributes == null) throw new ArgumentNullException(nameof(attributes));
			if (attributes.Length == 0) return GetProperties();

			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);
			ForEach(pair =>
			{
				if (pair.Value is ICustomAttributeProvider provider)
				{
					Attribute[] attr = provider.GetAttributes(true).ToArray();
					if (!attr.Contains(attributes)) return;
				}

				pds.Add(new DictionaryTypePropertyDescriptor<TSource, TKey, TValue>(this, pair.Key));
			});

			return pds;
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return ((IDictionary<TKey, TValue>)Source).GetEnumerator(); }

		public void Remove(object key) { Source.Remove(key); }

		[SuppressMessage("ReSharper", "PossibleStructMemberModificationOfNonVariableStruct")]
		object IDictionary.this[object key]
		{
			get => Source[key];
			set => Source[key] = value;
		}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public void Add(KeyValuePair<TKey, TValue> item) { Source.Add(item); }

		public bool Contains(object key) { return Source.Contains(key); }

		public void Add(object key, object value) { Source.Add(key, value); }

		public void Clear() { ((IDictionary<TKey, TValue>)Source).Clear(); }
		IDictionaryEnumerator IDictionary.GetEnumerator() { return ((IDictionary)Source).GetEnumerator(); }

		public bool Contains(KeyValuePair<TKey, TValue> item) { return Source.Contains(item); }

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { Source.CopyTo(array, arrayIndex); }

		public bool Remove(KeyValuePair<TKey, TValue> item) { return Source.Remove(item); }

		public void CopyTo(Array array, int index) { Source.CopyTo(array, index); }

		public int Count => ((IDictionary<TKey, TValue>)Source).Count;

		public object SyncRoot => Source.SyncRoot;

		public bool IsSynchronized => Source.IsSynchronized;

		ICollection IDictionary.Values => ((IDictionary)Source).Values;

		public bool IsReadOnly => ((IDictionary<TKey, TValue>)Source).IsReadOnly;

		public bool IsFixedSize => Source.IsFixedSize;

		public bool ContainsKey(TKey key) { return Source.ContainsKey(key); }

		public void Add(TKey key, TValue value) { Source.Add(key, value); }

		public bool Remove(TKey key) { return Source.Remove(key); }

		public bool TryGetValue(TKey key, out TValue value) { return Source.TryGetValue(key, out value); }

		[SuppressMessage("ReSharper", "PossibleStructMemberModificationOfNonVariableStruct")]
		public TValue this[TKey key]
		{
			get => Source[key];
			set => Source[key] = value;
		}

		public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)Source).Keys;

		ICollection IDictionary.Keys => ((IDictionary)Source).Keys;

		public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)Source).Values;

		public void ForEach([NotNull] Action<KeyValuePair<TKey, TValue>> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Func<KeyValuePair<TKey, TValue>, bool> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Action<KeyValuePair<TKey, TValue>, int> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Func<KeyValuePair<TKey, TValue>, int, bool> action) { Source.ForEach(action); }
	}
}