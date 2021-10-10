using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using JetBrains.Annotations;

namespace essentialMix.ComponentModel
{
	public abstract class DropDownTypeConverter<T> : TypeConverter<T>
	{
		protected DropDownTypeConverter()
		{
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }

		[NotNull]
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			return DropDownTypeConverter.GetValues<T>(GetValues);
		}

		[NotNull]
		protected abstract ICollection GetValues();
	}

	internal static class DropDownTypeConverter
	{
		private static readonly ConcurrentDictionary<Type, TypeConverter.StandardValuesCollection> __cache = new ConcurrentDictionary<Type, TypeConverter.StandardValuesCollection>();

		public static TypeConverter.StandardValuesCollection GetValues<T>([NotNull] Func<ICollection> getValues)
		{
			return __cache.GetOrAdd(typeof(T), _ => new TypeConverter.StandardValuesCollection(getValues()));
		}
	}
}