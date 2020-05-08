using System.Collections;
using System.ComponentModel;
using JetBrains.Annotations;

namespace asm.ComponentModel
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
			ICollection values = Values;
			return new StandardValuesCollection(values);
		}

		[NotNull]
		protected abstract ICollection Values { get; }
	}
}