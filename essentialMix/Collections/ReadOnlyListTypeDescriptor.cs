using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections;

public class ReadOnlyListTypeDescriptor<TSource, T>([NotNull] TSource source)
	: TypeDescriptorBase<TSource>(source), IReadOnlyList<T>, ICollection
	where TSource : IReadOnlyList<T>, IList
{
	[NotNull]
	public override PropertyDescriptorCollection GetProperties()
	{
		PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

		for (int i = 0; i < Count; i++)
			pds.Add(new ReadOnlyListTypePropertyDescriptor<TSource, T>(this, i));

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

			pds.Add(new ReadOnlyListTypePropertyDescriptor<TSource, T>(this, i));
		}

		return pds;
	}

	public IEnumerator<T> GetEnumerator() { return Source.GetEnumerator(); }

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	public void CopyTo(Array array, int index) { Source.CopyTo(array, index); }

	public int Count => ((IReadOnlyList<T>)Source).Count;

	public object SyncRoot => Source.SyncRoot;

	public bool IsSynchronized => Source.IsSynchronized;

	public T this[int index] => ((IReadOnlyList<T>)Source)[index];

	public void ForEach([NotNull] Action<T> action) { Source.ForEach(action); }

	public void ForEach([NotNull] Func<T, bool> action) { Source.ForEach(action); }

	public void ForEach([NotNull] Action<T, int> action) { Source.ForEach(action); }

	public void ForEach([NotNull] Func<T, int, bool> action) { Source.ForEach(action); }
}