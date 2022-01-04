using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using essentialMix.Data.Helpers;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class DataTableExtension
{
	public static bool IsValid(this DataTable thisValue, bool bCount = false, bool bNoErrors = false)
	{
		bool b = thisValue is { IsInitialized: true };
		if (bCount && b) b &= thisValue.Rows.Count > 0;
		if (bNoErrors && b) b &= !thisValue.HasErrors;
		return b;
	}

	public static bool IsNullOrEmpty(this DataTable thisValue) { return thisValue == null || thisValue.Rows.Count == 0; }

	public static object[] CollectValues([NotNull] this DataTable thisValue, [NotNull] params string[] fieldNames) { return CollectValues(thisValue, true, fieldNames); }

	public static object[] CollectValues([NotNull] this DataTable thisValue, bool skipNulls, [NotNull] params string[] fieldNames)
	{
		return CollectValues(thisValue, -1, skipNulls, fieldNames);
	}

	public static object[] CollectValues([NotNull] this DataTable thisValue, int rowIndex, [NotNull] params string[] fieldNames)
	{
		return CollectValues(thisValue, rowIndex, true, fieldNames);
	}

	public static object[] CollectValues([NotNull] this DataTable thisValue, int rowIndex, bool skipNulls, [NotNull] params string[] fieldNames)
	{
		if (!rowIndex.InRangeRx(0, thisValue.Rows.Count)) throw new ArgumentOutOfRangeException(nameof(rowIndex));

		List<string> fields = new List<string>();

		foreach (string fieldName in fieldNames)
		{
			string field = fieldName;

			if (!thisValue.Columns.Contains(fieldName))
			{
				field = fieldName.ToUpper();
				if (!thisValue.Columns.Contains(field)) field = fieldName.ToLower();
				if (!thisValue.Columns.Contains(field)) throw new MissingFieldException();
			}

			if (fields.Contains(field)) throw new DuplicateNameException();
			fields.Add(field);
		}

		if (fields.Count == 0) return null;

		List<object> values = new List<object>();

		if (rowIndex > -1)
		{
			DataRow row = thisValue.Rows[rowIndex];

			if (fields.Count == 1)
			{
				if (skipNulls && row[fields[0]].IsNull()) return null;
				values.Add(row[fields[0]]);
			}
			else
			{
				int n = skipNulls ? fields.Count(t => !row[t].IsNull()) : fields.Count;
				if (n == 0) return null;
				object[] rowValues = new object[n];

				for (int i = 0; i < fields.Count; i++)
				{
					if (skipNulls && row[fields[i]].IsNull()) continue;
					rowValues[i] = row[fields[i]];
				}

				values.Add(rowValues);
			}
		}
		else
		{
			foreach (DataRow row in thisValue.Rows)
			{
				if (fields.Count == 1)
				{
					if (skipNulls && row[fields[0]].IsNull()) continue;
					values.Add(row[fields[0]]);
				}
				else
				{
					int n = skipNulls ? fields.Count(t => !row[t].IsNull()) : fields.Count;
					if (n == 0) continue;
					object[] rowValues = new object[n];

					for (int i = 0; i < fields.Count; i++)
					{
						if (skipNulls && row[fields[i]].IsNull()) continue;
						rowValues[i] = row[fields[i]];
					}

					values.Add(rowValues);
				}
			}
		}

		return values.ToArray();
	}

	public static T GetFieldOfTable<T>([NotNull] this DataTable thisValue, [NotNull] string fieldName, T defaultValue = default(T), int rowIndex = 0)
	{
		if (fieldName.Length == 0 || !thisValue.Columns.Contains(fieldName)) throw new MissingFieldException();
		if (!thisValue.IsValid(true)) return defaultValue;
		if (!rowIndex.InRangeRx(0, thisValue.Rows.Count)) throw new ArgumentOutOfRangeException(nameof(rowIndex));
		return thisValue.Rows[rowIndex].Get(fieldName, defaultValue);
	}

	public static T GetExtendedProperty<T>([NotNull] this DataTable thisValue, [NotNull] string propertyName, T defaultValue = default(T))
	{
		if (propertyName.Length == 0 || !thisValue.ExtendedProperties.ContainsKey(propertyName)) throw new KeyNotFoundException();
		return thisValue.ExtendedProperties[propertyName].To(defaultValue);
	}

	public static void SetExtendedProperty<T>([NotNull] this DataTable thisValue, [NotNull] string propertyName, T value)
	{
		if (propertyName.Length == 0 || !thisValue.ExtendedProperties.ContainsKey(propertyName)) throw new KeyNotFoundException();
		thisValue.ExtendedProperties[propertyName] = value;
	}

	public static DataRow SelectRow([NotNull] this DataTable thisValue, [NotNull] string expression)
	{
		if (!IsValid(thisValue, true)) return null;

		DataRow row;

		if (string.IsNullOrEmpty(expression)) row = thisValue.Rows[0];
		else
		{
			DataRow[] rows = thisValue.Select(expression);
			row = rows.Length > 0 ? rows[0] : null;
		}

		return row;
	}

	public static T SelectValue<T>([NotNull] this DataTable thisValue, string expression, [NotNull] string fieldName, T defaultValue = default(T))
	{
		if (!IsValid(thisValue, true)) return defaultValue;
		if (!thisValue.Columns.Contains(fieldName)) throw new MissingFieldException(string.Empty, fieldName);

		T value;

		if (string.IsNullOrEmpty(expression))
			value = thisValue.Rows[0].Get(fieldName, defaultValue);
		else
		{
			DataRow[] rows = thisValue.Select(expression);
			value = rows.Length > 0 ? rows[0].Get(fieldName, defaultValue) : defaultValue;
		}

		return value;
	}

	public static string ToXml([NotNull] this DataTable thisValue, bool includeSchema = false, XmlWriterSettings options = null)
	{
		if (!IsValid(thisValue, true)) return null;

		StringBuilder sb = new StringBuilder();
		XmlWriterSettings opt = options ?? XmlWriterHelper.CreateSettings();

		using (XmlWriter writer = XmlWriter.Create(sb, opt))
			thisValue.WriteXml(writer, includeSchema ? XmlWriteMode.WriteSchema : XmlWriteMode.IgnoreSchema);

		return sb.ToString();
	}

	public static bool SaveAs([NotNull] this DataTable thisValue, [NotNull] string fileName, bool includeSchema = true, XmlWriterSettings options = null)
	{
		if (!IsValid(thisValue, true)) return false;

		XmlWriterSettings opt = options ?? XmlWriterHelper.CreateSettings();

		bool result;

		try
		{
			using (Stream stream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write))
			{
				using (XmlWriter writer = XmlWriter.Create(stream, opt))
				{
					thisValue.WriteXml(writer, includeSchema ? XmlWriteMode.WriteSchema : XmlWriteMode.IgnoreSchema);
					result = true;
				}
			}
		}
		catch
		{
			result = false;
		}

		return result;
	}

	public static void CopyTo([NotNull] this DataTable thisValue, [NotNull] DataSet target, bool overwrite = false)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));
		if (overwrite) target.Tables.Remove(thisValue.TableName);
		target.Merge(thisValue, true, MissingSchemaAction.AddWithKey);
	}

	public static bool NeedsToMergeSchema([NotNull] this DataTable thisValue, [NotNull] DataTable schema)
	{
		if (schema.Columns.Count == 0) return false;
		if (schema.Columns.Count != thisValue.Columns.Count || schema.PrimaryKey.Length != thisValue.PrimaryKey.Length) return true;
		if (schema.PrimaryKey.Length > 0 && schema.PrimaryKey.Any(pk => !thisValue.PrimaryKey.Contains(pk.ColumnName))) return true;
		return schema.Columns.Cast<DataColumn>().Any(c => !thisValue.Columns.Contains(c.ColumnName));
	}
}