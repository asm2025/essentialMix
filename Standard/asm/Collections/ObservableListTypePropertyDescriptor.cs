using System;
using JetBrains.Annotations;

namespace asm.Collections
{
	public class ObservableListTypePropertyDescriptor<TObservable, TSource, T> : TypePropertyDescriptorBase
		where TObservable : ObservableListTypeDescriptor<TSource, T>
		where TSource : ObservableList<T>
	{
		public ObservableListTypePropertyDescriptor([NotNull] TObservable sourceTypeDescriptor, int index)
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

		protected TObservable TypeDescriptorSource { get; }
		protected int Index { get; }
	}
}