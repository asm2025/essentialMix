using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace essentialMix.Core.WPF.Converters;

[Localizability(LocalizationCategory.NeverLocalize)]
public class InverseOrBooleanConverter : IMultiValueConverter
{
	public InverseOrBooleanConverter()
	{
	}

	/// <inheritdoc />
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values == null || values.Length == 0) return false;

		foreach (object value in values)
		{
			if (value is true) return false;
		}

		return true;
	}

	/// <inheritdoc />
	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}