using System;
using System.ComponentModel;
using System.Globalization;

namespace essentialMix.Collections
{
	public class WrapperDisplayNameExpandableObjectConverter : DisplayNameExpandableObjectConverter
	{
		public WrapperDisplayNameExpandableObjectConverter() { }

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			IWrapper wrapper = value as IWrapper;
			return base.ConvertTo(context, culture, wrapper?.Source ?? value, destinationType);
		}
	}
}