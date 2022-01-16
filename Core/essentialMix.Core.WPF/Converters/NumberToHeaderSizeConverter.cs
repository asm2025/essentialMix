using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Converters;

[Localizability(LocalizationCategory.NeverLocalize)]
public sealed class NumberToHeaderSizeConverter : IValueConverter
{
	/// <inheritdoc />
	[NotNull]
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return HeaderSizeHelper.From(value).ToFontSize();
	}

	/// <inheritdoc />
	[NotNull]
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value is not HeaderSize headerSize
					? throw new FormatException()
					: System.Convert.ChangeType((int)headerSize, targetType);
	}
}