using System;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace asm.Collections
{
	public class ObservableCollectionTypePropertyDescriptor<TObservable, TSource, T> : TypePropertyDescriptorBase
		where TObservable : ObservableCollectionTypeDescriptor<TSource, T>
		where TSource : ObservableCollection<T>
	{
		public ObservableCollectionTypePropertyDescriptor([NotNull] TObservable sourceTypeDescriptor, int index)
			: base($"#{index}", null)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
			TypeDescriptorSource = sourceTypeDescriptor;
			Index = index;
		}

		public override bool IsReadOnly => TypeDescriptorSource.IsReadOnly;

		protected override object GetTargetValue() { return TypeDescriptorSource.Source[Index]; }

		protected override void SetTargetValue(object value)
		{
			throw new NotSupportedException();
		}

		[NotNull] protected override string GetName() { return $"{Index}"; }

		[NotNull] protected override Type GetComponentType() { return TypeDescriptorSource.Source.GetType(); }

		protected TObservable TypeDescriptorSource { get; }
		protected int Index { get; }
	}
}