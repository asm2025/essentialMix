using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	public class ListTypePropertyDescriptor<TSource> : TypePropertyDescriptorBase
		where TSource : IList
	{
		public ListTypePropertyDescriptor([NotNull] ListTypeDescriptor<TSource> sourceTypeDescriptor, int index)
			: base($"#{index}", null)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
			TypeDescriptorSource = sourceTypeDescriptor;
			Index = index;
		}

		public override bool IsReadOnly => TypeDescriptorSource.IsReadOnly;

		protected override object GetTargetValue() { return TypeDescriptorSource[Index]; }

		protected override void SetTargetValue(object value)
		{
			TypeDescriptorSource[Index] = value;
		}

		[NotNull] protected override string GetName() { return $"{Index}"; }

		[NotNull] protected override Type GetComponentType() { return TypeDescriptorSource.Source.GetType(); }

		protected ListTypeDescriptor<TSource> TypeDescriptorSource { get; }
		protected int Index { get; }
	}

	public class ListTypePropertyDescriptor<TSource, T> : TypePropertyDescriptorBase
		where TSource : IList<T>, IList
	{
		public ListTypePropertyDescriptor([NotNull] ListTypeDescriptor<TSource, T> sourceTypeDescriptor, int index)
			: base($"#{index}", null)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
			TypeDescriptorSource = sourceTypeDescriptor;
			Index = index;
		}

		public override bool IsReadOnly => TypeDescriptorSource.IsReadOnly;

		protected override object GetTargetValue() { return TypeDescriptorSource[Index]; }

		protected override void SetTargetValue(object value)
		{
			TypeDescriptorSource[Index] = (T)value;
		}

		[NotNull] protected override string GetName() { return $"{Index}"; }

		[NotNull] protected override Type GetComponentType() { return TypeDescriptorSource.Source.GetType(); }

		protected ListTypeDescriptor<TSource, T> TypeDescriptorSource { get; }
		protected int Index { get; }
	}
}