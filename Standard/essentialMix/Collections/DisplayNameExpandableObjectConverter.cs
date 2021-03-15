using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using essentialMix.Extensions;

namespace essentialMix.Collections
{
	public class DisplayNameExpandableObjectConverter : ExpandableObjectConverter
	{
		protected const string TARGET_NAME = "DisplayName";

		protected static readonly Type TargetType = typeof(string);

		public DisplayNameExpandableObjectConverter() { }

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType != TargetType) return base.ConvertTo(context, culture, value, destinationType);
			if (value == null) return string.Empty;

			string name = Convert.ToString(value);
			if (value is ICustomAttributeProvider provider) return provider.GetDisplayName(name);
			return value.GetPropertyValue(TARGET_NAME, out string displayName, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE, TargetType, name) ? displayName : name;
		}
	}
}