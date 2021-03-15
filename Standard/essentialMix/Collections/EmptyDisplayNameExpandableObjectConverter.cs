using System;
using System.ComponentModel;
using System.Globalization;

namespace essentialMix.Collections
{
	public class EmptyDisplayNameExpandableObjectConverter : DisplayNameExpandableObjectConverter
	{
		public EmptyDisplayNameExpandableObjectConverter() { }

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			return destinationType == TargetType
						? string.Empty
						: base.ConvertTo(context, culture, value, destinationType);
		}
	}
}