using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public class EnumerableTypeDescriptor<TSource> : TypeDescriptorBase<TSource>, IEnumerable
		where TSource : IEnumerable
	{
		public EnumerableTypeDescriptor([NotNull] TSource source)
			: base(source)
		{
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties()
		{
			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);
			ForEach((o, i) => pds.Add(new EnumerableTypePropertyDescriptor<TSource>(this, i)));
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

				pds.Add(new EnumerableTypePropertyDescriptor<TSource>(this, i));
			});

			return pds;
		}

		public IEnumerator GetEnumerator() { return Source.GetEnumerator(); }

		public void ForEach([NotNull] Action<object> action) { Source.Cast<object>().ForEach(action); }

		public void ForEach([NotNull] Func<object, bool> action) { Source.Cast<object>().ForEach(action); }

		public void ForEach([NotNull] Action<object, int> action) { Source.Cast<object>().ForEach(action); }

		public void ForEach([NotNull] Func<object, int, bool> action) { Source.Cast<object>().ForEach(action); }
	}

	public class EnumerableTypeDescriptor<TSource, TValue> : TypeDescriptorBase<TSource>, IEnumerable<TValue>, IEnumerable
		where TSource : IEnumerable<TValue>, IEnumerable
	{
		public EnumerableTypeDescriptor([NotNull] TSource source)
			: base(source)
		{
		}

		[NotNull]
		public override PropertyDescriptorCollection GetProperties()
		{
			PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);
			ForEach((o, i) => pds.Add(new EnumerableTypePropertyDescriptor<TSource, TValue>(this, i)));
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

				pds.Add(new EnumerableTypePropertyDescriptor<TSource, TValue>(this, i));
			});

			return pds;
		}

		public IEnumerator<TValue> GetEnumerator() { return Source.GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public void ForEach([NotNull] Action<TValue> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Func<TValue, bool> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Action<TValue, int> action) { Source.ForEach(action); }

		public void ForEach([NotNull] Func<TValue, int, bool> action) { Source.ForEach(action); }
	}
}