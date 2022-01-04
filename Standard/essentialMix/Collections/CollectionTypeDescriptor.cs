using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections;

public class CollectionTypeDescriptor<TSource> : TypeDescriptorBase<TSource>, ICollection
	where TSource : ICollection
{
	public CollectionTypeDescriptor([NotNull] TSource source)
		: base(source)
	{
	}

	[NotNull]
	public override PropertyDescriptorCollection GetProperties()
	{
		PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

		for (int i = 0; i < Count; i++)
			pds.Add(new CollectionTypePropertyDescriptor<TSource>(this, i));

		return pds;
	}

	[NotNull]
	public override PropertyDescriptorCollection GetProperties([NotNull] Attribute[] attributes)
	{
		if (attributes == null) throw new ArgumentNullException(nameof(attributes));
		if (attributes.Length == 0) return GetProperties();

		PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);
		ForEach((o, i) =>
		{
			if (o is ICustomAttributeProvider provider)
			{
				Attribute[] attr = provider.GetAttributes(true).ToArray();
				if (!attr.Contains(attributes)) return;
			}

			pds.Add(new CollectionTypePropertyDescriptor<TSource>(this, i));
		});

		return pds;
	}

	public IEnumerator GetEnumerator() { return Source.GetEnumerator(); }

	public void CopyTo(Array array, int index) { Source.CopyTo(array, index); }

	public int Count => Source.Count;

	public object SyncRoot => Source.SyncRoot;

	public bool IsSynchronized => Source.IsSynchronized;

	public void ForEach([NotNull] Action<object> action) { Source.Cast<object>().ForEach(action); }

	public void ForEach([NotNull] Func<object, bool> action) { Source.Cast<object>().ForEach(action); }

	public void ForEach([NotNull] Action<object, int> action) { Source.Cast<object>().ForEach(action); }

	public void ForEach([NotNull] Func<object, int, bool> action) { Source.Cast<object>().ForEach(action); }
}

public class CollectionTypeDescriptor<TSource, TValue> : TypeDescriptorBase<TSource>, ICollection<TValue>, ICollection
	where TSource : ICollection<TValue>, ICollection
{
	public CollectionTypeDescriptor([NotNull] TSource source)
		: base(source)
	{
	}

	[NotNull]
	public override PropertyDescriptorCollection GetProperties()
	{
		PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

		for (int i = 0; i < Count; i++)
			pds.Add(new CollectionTypePropertyDescriptor<TSource, TValue>(this, i));

		return pds;
	}

	[NotNull]
	public override PropertyDescriptorCollection GetProperties([NotNull] Attribute[] attributes)
	{
		if (attributes == null) throw new ArgumentNullException(nameof(attributes));
		if (attributes.Length == 0) return GetProperties();

		PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);
		ForEach((o, i) =>
		{
			if (o is ICustomAttributeProvider provider)
			{
				Attribute[] attr = provider.GetAttributes(true).ToArray();
				if (!attr.Contains(attributes)) return;
			}

			pds.Add(new CollectionTypePropertyDescriptor<TSource, TValue>(this, i));
		});

		return pds;
	}

	public IEnumerator<TValue> GetEnumerator() { return Source.GetEnumerator(); }

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	public void Add(TValue item) { Source.Add(item); }
	public void Clear() { Source.Clear(); }
	public bool Contains(TValue item) { return Source.Contains(item); }

	public void CopyTo(TValue[] array, int arrayIndex) { Source.CopyTo(array, arrayIndex); }
	public void CopyTo(Array array, int index) { Source.CopyTo(array, index); }
	public bool Remove(TValue item) { return Source.Remove(item); }

	public int Count => ((IList<TValue>)Source).Count;

	public object SyncRoot => Source.SyncRoot;

	public bool IsSynchronized => Source.IsSynchronized;

	public bool IsReadOnly => Source.IsReadOnly;

	public void ForEach([NotNull] Action<TValue> action) { Source.ForEach(action); }

	public void ForEach([NotNull] Func<TValue, bool> action) { Source.ForEach(action); }

	public void ForEach([NotNull] Action<TValue, int> action) { Source.ForEach(action); }

	public void ForEach([NotNull] Func<TValue, int, bool> action) { Source.ForEach(action); }
}