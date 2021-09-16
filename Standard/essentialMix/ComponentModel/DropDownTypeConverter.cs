using System.Collections;
using System.ComponentModel;
using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.ComponentModel
{
	public abstract class DropDownTypeConverter<T> : TypeConverter<T>
	{
		private static volatile StandardValuesCollection __values;

		protected DropDownTypeConverter()
		{
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }

		[NotNull]
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if (__values == null) Interlocked.CompareExchange(ref __values, new StandardValuesCollection(GetValues()), null);
			return __values;
		}

		[NotNull]
		protected abstract ICollection GetValues();
	}
}