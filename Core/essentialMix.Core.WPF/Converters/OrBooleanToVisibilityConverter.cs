using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Converters
{
	[Localizability(LocalizationCategory.NeverLocalize)]
	public class OrBooleanToVisibilityConverter : IMultiValueConverter
	{
		public OrBooleanToVisibilityConverter()
		{
		}

		/// <inheritdoc />
		[NotNull]
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values == null || values.Length == 0) return Visibility.Visible;

			foreach (object value in values)
			{
				if (value is true) return Visibility.Visible;
			}

			return Visibility.Collapsed;
		}

		/// <inheritdoc />
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}