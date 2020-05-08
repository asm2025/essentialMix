using System;
using System.ComponentModel;
using System.Globalization;

namespace asm.Collections
{
	public class EmptyDisplayNameExpandableObjectConverter : DisplayNameExpandableObjectConverter
	{
		public EmptyDisplayNameExpandableObjectConverter() { }

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			return destinationType == TARGET_TYPE
						? string.Empty
						: base.ConvertTo(context, culture, value, destinationType);
		}
	}
}