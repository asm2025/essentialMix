﻿using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.ComponentModel;

/// <inheritdoc />
public class ExpandableObjectConverter<T> : ExpandableObjectConverter
{
	/// <inheritdoc />
	public ExpandableObjectConverter()
	{
	}

	/// <inheritdoc />
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) { return sourceType.Is<T>() || base.CanConvertFrom(context, sourceType); }

	/// <inheritdoc />
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		switch (value)
		{
			case null:
				return null;
			case T t:
				return t;
			default:
				return CustomConvertFrom(context, culture, value);
		}
	}

	/// <inheritdoc />
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return destinationType == typeof(T) || destinationType == typeof(InstanceDescriptor) || base.CanConvertTo(context, destinationType);
	}

	/// <inheritdoc />
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value == null) return destinationType.Default();
		return destinationType.Is<T>()
					? (T)value
					: CustomConvertTo(context, culture, value, destinationType);
	}

	protected virtual object CustomConvertFrom([NotNull] ITypeDescriptorContext context, [NotNull] CultureInfo culture, object value) { return base.ConvertFrom(context, culture, value); }

	protected virtual object CustomConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, [NotNull] Type destinationType)
	{
		return base.ConvertTo(context, culture, value, destinationType);
	}
}