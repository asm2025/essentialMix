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
	public class ListTypeDescriptor<TSource> : TypeDescriptorBase<TSource>, IList
		where TSource : IList
	{
		public ListTypeDescriptor([NotNull] TSource source)
			: base(source)
		{
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties()
		{
			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

			for (int i = 0; i < Count; i++)
				pds.Add(new ListTypePropertyDescriptor<TSource>(this, i));

			return pds;
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties([NotNull] Attribute[] attributes)
		{
			if (attributes == null) throw new ArgumentNullException(nameof(attributes));
			if (attributes.Length == 0) return GetProperties();

			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

			for (int i = 0; i < Count; i++)
			{
				if (this[i] is ICustomAttributeProvider provider)
				{
					Attribute[] attr = provider.GetAttributes(true).ToArray();
					if (!attr.Contains(attributes)) continue;
				}

				pds.Add(new ListTypePropertyDescriptor<TSource>(this, i));
			}

			return pds;
		}

		public IEnumerator GetEnumerator() { return Source.GetEnumerator(); }

		public void CopyTo(Array array, int index)
		{
			Source.CopyTo(array, index);
		}

		public int Count => Source.Count;

		public object SyncRoot => Source.SyncRoot;

		public bool IsSynchronized => Source.IsSynchronized;

		public int Add(object value) { return Source.Add(value); }

		public bool Contains(object value) { return Source.Contains(value); }

		public void Clear() { Source.Clear(); }

		public int IndexOf(object value) { return Source.IndexOf(value); }

		public void Insert(int index, object value) { Source.Insert(index, value); }

		public void Remove(object value) { Source.Remove(value); }

		public void RemoveAt(int index) { Source.RemoveAt(index); }

		[SuppressMessage("ReSharper", "PossibleStructMemberModificationOfNonVariableStruct")]
		public object this[int index]
		{
			get => Source[index];
			set => Source[index] = value;
		}

		public bool IsReadOnly => Source.IsReadOnly;

		public bool IsFixedSize => Source.IsFixedSize;

		public void ForEach([NotNull] Action<object> action) { Source.Cast<object>().ForEach(action); }

		public void ForEach([NotNull] Func<object, bool> action) { Source.Cast<object>().ForEach(action); }

		public void ForEach([NotNull] Action<object, int> action) { Source.Cast<object>().ForEach(action); }

		public void ForEach([NotNull] Func<object, int, bool> action) { Source.Cast<object>().ForEach(action); }
	}

	public class ListTypeDescriptor<TSource, T> : TypeDescriptorBase<TSource>, IList<T>, IList
		where TSource : IList<T>, IList
	{
		public ListTypeDescriptor([NotNull] TSource source)
			: base(source)
		{
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties()
		{
			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

			for (int i = 0; i < Count; i++)
				pds.Add(new ListTypePropertyDescriptor<TSource, T>(this, i));

			return pds;
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties([NotNull] Attribute[] attributes)
		{
			if (attributes == null) throw new ArgumentNullException(nameof(attributes));
			if (attributes.Length == 0) return GetProperties();

			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

			for (int i = 0; i < Count; i++)
			{
				if (this[i] is ICustomAttributeProvider provider)
				{
					Attribute[] attr = provider.GetAttributes(true).ToArray();
					if (!attr.Contains(attributes)) continue;
				}

				pds.Add(new ListTypePropertyDescriptor<TSource, T>(this, i));
			}

			return pds;
		}

		public IEnumerator<T> GetEnumerator() { return Source.GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public void Add(T item) { Source.Add(item); }

		public int Add(object value) { return Source.Add(value); }

		public bool Contains(object value) { return Source.Contains(value); }

		public void Clear() { ((IList<T>)Source).Clear(); }
		public int IndexOf(object value) { return Source.IndexOf(value); }

		public void Insert(int index, object value) { Source.Insert(index, value); }

		public void Remove(object value) { Source.Remove(value); }

		public bool Contains(T item) { return Source.Contains(item); }

		public void CopyTo(T[] array, int arrayIndex) { Source.CopyTo(array, arrayIndex); }

		public bool Remove(T item) { return Source.Remove(item); }

		public void CopyTo(Array array, int index) { Source.CopyTo(array, index); }

		public int Count => ((IList<T>)Source).Count;

		public object SyncRoot => Source.SyncRoot;

		public bool IsSynchronized => Source.IsSynchronized;

		public bool IsReadOnly => ((IList<T>)Source).IsReadOnly;

		public bool IsFixedSize => Source.IsFixedSize;

		public int IndexOf(T item) { return Source.IndexOf(item); }

		public void Insert(int index, T item) { Source.Insert(index, item); }

		public void RemoveAt(int index) { ((IList<T>)Source).RemoveAt(index); }

		object IList.this[int index]
		{
			get => ((IList)Source)[index];
			set => ((IList)Source)[index] = value;
		}

		public T this[int index]
		{
			get => ((IList<T>)Source)[index];
			set => ((IList<T>)Source)[index] = value;
		}

		public void ForEach([NotNull] Action<T> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Func<T, bool> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Action<T, int> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Func<T, int, bool> action) { Source.ForEach(action); }
	}
}