using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using essentialMix.ComponentModel;

namespace essentialMix.Web.Drawing;

[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
[AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
public class PxUnitConverter : TypeConverter<PxUnit>
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) { return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType); }

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
	}

	protected override object CustomConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null) return null;
		if (value is not string str) return new PxUnit();

		string s = str.Trim();
		return s.Length == 0 ? new PxUnit() : PxUnit.Parse(s, culture);
	}

	protected override object CustomConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			if (value == null) return null;

			PxUnit unit = (PxUnit)value;
			MemberInfo member;
			object[] arguments = null;
			Type type = typeof(PxUnit);

			if (unit.IsEmpty)
			{
				member = type.GetField("Empty", Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC) ??
						(MemberInfo)type.GetProperty("Empty", Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE_STATIC);
			}
			else
			{
				member = type.GetConstructor(new[] { typeof(double), typeof(PxUnitType) });
				arguments = new object[] { unit.Value, unit.Type };
			}

			return member != null ? new InstanceDescriptor(member, arguments) : null;
		}

		if (destinationType == typeof(string))
		{
			if (value is PxUnit unit) return unit.ToString(culture);
		}

		return PxUnit.Parse(Convert.ToString(value), culture);
	}
}