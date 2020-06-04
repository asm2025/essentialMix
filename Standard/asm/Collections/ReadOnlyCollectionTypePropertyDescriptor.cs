using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using asm.Exceptions.Collections;

namespace asm.Collections
{
	public class ReadOnlyCollectionTypePropertyDescriptor<TSource, T> : TypePropertyDescriptorBase
		where TSource : IReadOnlyCollection<T>, ICollection
	{
		public ReadOnlyCollectionTypePropertyDescriptor([NotNull] ReadOnlyCollectionTypeDescriptor<TSource, T> sourceTypeDescriptor, int index)
			: base($"#{index}", null)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
			TypeDescriptorSource = sourceTypeDescriptor;
			Index = index;
			IsReadOnly = true;
		}

		public override bool IsReadOnly { get; }

		protected override object GetTargetValue() { return TypeDescriptorSource.ElementAt(Index); }

		protected override void SetTargetValue(object value)
		{
			throw new ReadOnlyException();
		}

		[NotNull]
		protected override string GetName() { return $"{Index}"; }

		[NotNull]
		protected override Type GetComponentType() { return TypeDescriptorSource.Source.GetType(); }

		protected ReadOnlyCollectionTypeDescriptor<TSource, T> TypeDescriptorSource { get; }
		protected int Index { get; }
	}
}