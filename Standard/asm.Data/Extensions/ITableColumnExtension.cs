using System;
using System.Data;
using System.Globalization;
using System.Reflection;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Data.Patterns.Table;

namespace asm.Data.Extensions
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
			if (!thisValue.Formatting.HasValue) thisValue.Formatting = TableColumnFormatting.General;

			switch (thisValue.Formatting.Value)
			{
				case TableColumnFormatting.Text:
					return Convert.ToString(value);
				case TableColumnFormatting.Number:
					return string.Format(cultureInfo, "{0:N}", value);
				case TableColumnFormatting.Integer:
					return string.Format(cultureInfo, "{0:N0}", value);
				case TableColumnFormatting.Float:
					return string.Format(cultureInfo, "{0:F2}", value);
				case TableColumnFormatting.Currency:
					return string.Format(cultureInfo, "{0:C}", value);
				case TableColumnFormatting.Percent:
					return string.Format(cultureInfo, "{0:P}", value);
				case TableColumnFormatting.DateTime:
					return string.Format(cultureInfo, "{0:D}", value);
				case TableColumnFormatting.Date:
					return string.Format(cultureInfo, "{0:d}", value);
				case TableColumnFormatting.Time:
					return string.Format(cultureInfo, "{0:T}", value);
				case TableColumnFormatting.Raw:
					return value is byte[] bytes
								? bytes.ToHexString()
								: value;
				case TableColumnFormatting.Custom:
					return !string.IsNullOrEmpty(thisValue.CustomFormat)
								? string.Format(cultureInfo, thisValue.CustomFormat, value)
								: Convert.ToString(value, cultureInfo);
				default:
					return thisValue.Formattable
								? Convert.ToString(value, cultureInfo)
								: value;
			}
		}
	}
}