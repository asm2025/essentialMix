using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace asm.Collections
{
	public class CollectionTypePropertyDescriptor<TSource> : TypePropertyDescriptorBase
		where TSource : ICollection
	{
		public CollectionTypePropertyDescriptor([NotNull] CollectionTypeDescriptor<TSource> sourceTypeDescriptor, int index)
			: base($"#{index}", null)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
			TypeDescriptorSource = sourceTypeDescriptor;
			Index = index;
			IsReadOnly = true;
		}

		public override bool IsReadOnly { get; }

		protected override object GetTargetValue() { return TypeDescriptorSource.Cast<object>().ElementAt(Index); }

		protected override void SetTargetValue(object value)
		{
			throw new NotSupportedException();
		}

		[NotNull] protected override string GetName() { return $"{Index}"; }

		[NotNull] protected override Type GetComponentType() { return TypeDescriptorSource.Source.GetType(); }

		protected CollectionTypeDescriptor<TSource> TypeDescriptorSource { get; }
		protected int Index { get; }
	}

	public class CollectionTypePropertyDescriptor<TSource, T> : TypePropertyDescriptorBase
		where TSource : ICollection<T>, ICollection
	{
		public CollectionTypePropertyDescriptor([NotNull] CollectionTypeDescriptor<TSource, T> sourceTypeDescriptor, int index)
			: base($"#{index}", null)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
			TypeDescriptorSource = sourceTypeDescriptor;
			Index = index;
		}

		public override bool IsReadOnly => TypeDescriptorSource.IsReadOnly;

		protected override object GetTargetValue() { return TypeDescriptorSource.ElementAt(Index); }

		protected override void SetTargetValue(object value)
		{
			throw new NotSupportedException();
		}

		[NotNull] protected override string GetName() { return $"{Index}"; }

		[NotNull] protected override Type GetComponentType() { return TypeDescriptorSource.Source.GetType(); }

		protected CollectionTypeDescriptor<TSource, T> TypeDescriptorSource { get; }
		protected int Index { get; }
	}
}