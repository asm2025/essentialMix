using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Converters
{
	[Localizability(LocalizationCategory.NeverLocalize)]
	public class InverseBooleanToVisibilityConverter : IValueConverter
	{
		public InverseBooleanToVisibilityConverter()
		{
		}

		/// <inheritdoc />
		[NotNull]
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is true
						? Visibility.Collapsed
						: Visibility.Visible;
		}

		/// <inheritdoc />
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is not Visibility.Visible;
		}
	}
}