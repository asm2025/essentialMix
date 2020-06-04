using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	public class DictionaryTypePropertyDescriptor<TSource> : TypePropertyDescriptorBase
		where TSource : IDictionary
	{
		public DictionaryTypePropertyDescriptor([NotNull] DictionaryTypeDescriptor<TSource> sourceTypeDescriptor, [NotNull] object key)
			: base(Convert.ToString(key), null)
		{
			TypeDescriptorSource = sourceTypeDescriptor;
			Key = key ?? throw new ArgumentNullException(nameof(key));
		}

		public override bool IsReadOnly => TypeDescriptorSource.IsReadOnly;

		protected override object GetTargetValue() { return TypeDescriptorSource[Key]; }

		protected override void SetTargetValue(object value)
		{
			TypeDescriptorSource[Key] = value;
		}

		[NotNull]
		protected override string GetName() { return Convert.ToString(Key); }

		[NotNull]
		protected override Type GetComponentType() { return TypeDescriptorSource.Source.GetType(); }

		protected DictionaryTypeDescriptor<TSource> TypeDescriptorSource { get; }
		protected object Key { get; }
	}

	public class DictionaryTypePropertyDescriptor<TSource, TKey, TValue> : TypePropertyDescriptorBase
		where TSource : IDictionary<TKey, TValue>, IDictionary
	{
		public DictionaryTypePropertyDescriptor([NotNull] DictionaryTypeDescriptor<TSource, TKey, TValue> sourceTypeDescriptor, [NotNull] TKey key)
			: base(Convert.ToString(key), null)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			TypeDescriptorSource = sourceTypeDescriptor;
			Key = key;
		}

		public override bool IsReadOnly => TypeDescriptorSource.IsReadOnly;

		protected override object GetTargetValue() { return TypeDescriptorSource[Key]; }

		protected override void SetTargetValue(object value)
		{
			TypeDescriptorSource[Key] = (TValue)value;
		}

		[NotNull]
		protected override string GetName() { return Convert.ToString(Key); }

		[NotNull]
		protected override Type GetComponentType() { return TypeDescriptorSource.Source.GetType(); }

		protected DictionaryTypeDescriptor<TSource, TKey, TValue> TypeDescriptorSource { get; }
		protected TKey Key { get; }
	}
}