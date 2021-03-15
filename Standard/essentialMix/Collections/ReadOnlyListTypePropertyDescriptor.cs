using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using essentialMix.Exceptions.Collections;

namespace essentialMix.Collections
{
	public class ReadOnlyListTypePropertyDescriptor<TSource, T> : TypePropertyDescriptorBase
		where TSource : IReadOnlyList<T>, IList
	{
		public ReadOnlyListTypePropertyDescriptor([NotNull] ReadOnlyListTypeDescriptor<TSource, T> sourceTypeDescriptor, int index)
			: base($"#{index}", null)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
			TypeDescriptorSource = sourceTypeDescriptor;
			Index = index;
			IsReadOnly = true;
		}

		public override bool IsReadOnly { get; }

		protected override object GetTargetValue() { return TypeDescriptorSource[Index]; }

		protected override void SetTargetValue(object value)
		{
			throw new ReadOnlyException();
		}

		[NotNull]
		protected override string GetName() { return $"{Index}"; }

		[NotNull]
		protected override Type GetComponentType() { return TypeDescriptorSource.Source.GetType(); }

		protected ReadOnlyListTypeDescriptor<TSource, T> TypeDescriptorSource { get; }
		protected int Index { get; }
	}
}