using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Converters;

[Localizability(LocalizationCategory.NeverLocalize)]
public class InverseBooleanConverter : IValueConverter
{
	public InverseBooleanConverter()
	{
	}

	/// <inheritdoc />
	[NotNull]
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value is false;
	}

	/// <inheritdoc />
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value is not true;
	}
}