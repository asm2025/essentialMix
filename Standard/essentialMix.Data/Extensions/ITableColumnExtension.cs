using System;
using System.Data;
using System.Globalization;
using System.Reflection;
using JetBrains.Annotations;
using essentialMix.Data.Patterns.Table;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class ITableColumnExtension
	{
		public static object Get([NotNull] this ITableColumn thisValue, [NotNull] IDataReader reader, [NotNull] CultureInfo cultureInfo)
		{
			return Format(thisValue, reader[thisValue.Name], cultureInfo);
		}
		
		public static object Get([NotNull] this ITableColumn thisValue, [NotNull] DataRow row, [NotNull] CultureInfo cultureInfo)
		{
			return Format(thisValue, row[thisValue.Name], cultureInfo);
		}
		
		public static object Get([NotNull] this ITableColumn thisValue, [NotNull] object source, [NotNull] PropertyInfo property, [NotNull] CultureInfo cultureInfo)
		{
			return Format(thisValue, property.GetValue(source), cultureInfo);
		}
		
		public static object Format([NotNull] this ITableColumn thisValue, object value, [NotNull] CultureInfo cultureInfo)
		{
			thisValue.Formatting ??= TableColumnFormatting.General;

			return thisValue.Formatting.Value switch
			{
				TableColumnFormatting.Text => Convert.ToString(value),
				TableColumnFormatting.Number => string.Format(cultureInfo, "{0:N}", value),
				TableColumnFormatting.Integer => string.Format(cultureInfo, "{0:N0}", value),
				TableColumnFormatting.Float => string.Format(cultureInfo, "{0:F2}", value),
				TableColumnFormatting.Currency => string.Format(cultureInfo, "{0:C}", value),
				TableColumnFormatting.Percent => string.Format(cultureInfo, "{0:P}", value),
				TableColumnFormatting.DateTime => string.Format(cultureInfo, "{0:D}", value),
				TableColumnFormatting.Date => string.Format(cultureInfo, "{0:d}", value),
				TableColumnFormatting.Time => string.Format(cultureInfo, "{0:T}", value),
				TableColumnFormatting.Raw => value is byte[] bytes
												? bytes.ToHexString()
												: value,
				TableColumnFormatting.Custom => !string.IsNullOrEmpty(thisValue.CustomFormat)
													? string.Format(cultureInfo, thisValue.CustomFormat, value)
													: Convert.ToString(value, cultureInfo),
				_ => thisValue.Formattable
						? Convert.ToString(value, cultureInfo)
						: value
			};
		}
	}
}