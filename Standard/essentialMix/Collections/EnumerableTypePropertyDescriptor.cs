using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public class EnumerableTypePropertyDescriptor<TSource> : TypePropertyDescriptorBase
		where TSource : IEnumerable
	{
		public EnumerableTypePropertyDescriptor([NotNull] EnumerableTypeDescriptor<TSource> sourceTypeDescriptor, int index)
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

		[NotNull]
		protected override string GetName() { return $"{Index}"; }

		[NotNull]
		protected override Type GetComponentType() { return TypeDescriptorSource.Source.GetType(); }

		protected EnumerableTypeDescriptor<TSource> TypeDescriptorSource { get; }
		protected int Index { get; }
	}

	public class EnumerableTypePropertyDescriptor<TSource, T> : TypePropertyDescriptorBase
		where TSource : IEnumerable<T>
	{
		public EnumerableTypePropertyDescriptor([NotNull] EnumerableTypeDescriptor<TSource, T> sourceTypeDescriptor, int index)
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
			throw new NotSupportedException();
		}

		[NotNull]
		protected override string GetName() { return $"{Index}"; }

		[NotNull]
		protected override Type GetComponentType() { return TypeDescriptorSource.Source.GetType(); }

		protected EnumerableTypeDescriptor<TSource, T> TypeDescriptorSource { get; }
		protected int Index { get; }
	}
}