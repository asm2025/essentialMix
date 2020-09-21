using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using asm.Data.Helpers;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class DataSetExtension
	{
		public static bool IsValid(this DataSet thisValue, bool bCount = false, bool bNoErrors = false)
		{
			bool b = thisValue != null && thisValue.IsInitialized;
			if (bCount && b) b &= thisValue.Tables.Count > 0;
			if (bNoErrors && b) b &= !thisValue.HasErrors;
			return b;
		}

		public static bool IsNullOrEmpty(this DataSet thisValue) { return thisValue == null || thisValue.Tables.Count == 0; }

		public static object[] CollectValues([NotNull] this DataSet thisValue, int index, [NotNull] params string[] fieldNames)
		{
			return CollectValues(thisValue, index, true, fieldNames);
		}

		public static object[] CollectValues([NotNull] this DataSet thisValue, int index, bool skipNulls, [NotNull] params string[] fieldNames)
		{
			return CollectValues(thisValue, index, -1, skipNulls, fieldNames);
		}

		public static object[] CollectValues([NotNull] this DataSet thisValue, int index, int rowIndex, [NotNull] params string[] fieldNames)
		{
			return CollectValues(thisValue, index, rowIndex, true, fieldNames);
		}

		public static object[] CollectValues([NotNull] this DataSet thisValue, int index, int rowIndex, bool skipNulls, [NotNull] params string[] fieldNames)
		{
			if (!index.InRangeRx(0, thisValue.Tables.Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (!rowIndex.InRangeRx(0, thisValue.Tables[index].Rows.Count)) throw new ArgumentOutOfRangeException(nameof(rowIndex));
			return thisValue.Tables[index].CollectValues(rowIndex, skipNulls, fieldNames);
		}

		public static object[] CollectValues([NotNull] this DataSet thisValue, [NotNull] string tableName, [NotNull] params string[] fieldNames)
		{
			return CollectValues(thisValue, tableName, true, fieldNames);
		}

		public static object[] CollectValues([NotNull] this DataSet thisValue, [NotNull] string tableName, bool skipNulls, [NotNull] params string[] fieldNames)
		{
			return CollectValues(thisValue, tableName, -1, skipNulls, fieldNames);
		}

		public static object[] CollectValues([NotNull] this DataSet thisValue, [NotNull] string tableName, int rowIndex, [NotNull] params string[] fieldNames)
		{
			return CollectValues(thisValue, tableName, rowIndex, true, fieldNames);
		}

		public static object[] CollectValues([NotNull] this DataSet thisValue, [NotNull] string tableName, int rowIndex, bool skipNulls, [NotNull] params string[] fieldNames)
		{
			if (tableName.Length == 0 || !thisValue.Tables.Contains(tableName)) throw new KeyNotFoundException();
			if (!rowIndex.InRangeRx(0, thisValue.Tables[tableName].Rows.Count)) throw new ArgumentOutOfRangeException(nameof(rowIndex));
			return thisValue.Tables[tableName].CollectValues(rowIndex, skipNulls, fieldNames);
		}

		public static DataTable GetSourceTable([NotNull] this DataSet thisValue, [NotNull] string tableName)
		{
			if (!thisValue.Tables.Contains(tableName)) throw new KeyNotFoundException();
			return thisValue.Tables[tableName];
		}

		public static DataTable GetSourceTable([NotNull] this DataSet thisValue, int index)
		{
			if (!index.InRangeRx(0, thisValue.Tables.Count)) throw new ArgumentOutOfRangeException(nameof(index));
			return thisValue.Tables[index];
		}

		public static string ToXml([NotNull] this DataSet thisValue, bool includeSchema = false, XmlWriterSettings options = null, Encoding encoding = null)
		{
			if (!IsValid(thisValue, true)) return null;

			StringBuilder sb = new StringBuilder();
			XmlWriterSettings opt = options ?? XmlWriterHelper.CreateSettings(level: ConformanceLevel.Fragment, encoding: encoding);

			using (XmlWriter writer = XmlWriter.Create(sb, opt))
				thisValue.WriteXml(writer, includeSchema ? XmlWriteMode.WriteSchema : XmlWriteMode.IgnoreSchema);

			return sb.ToString();
		}

		public static bool FromFile([NotNull] this DataSet thisValue, [NotNull] string fileName, bool includeSchema = true, XmlReaderSettings options = null)
		{
			if (!File.Exists(fileName)) return false;

			XmlReaderSettings opt = options ?? XmlReaderHelper.CreateSettings();
			bool result;

			try
			{
				using (XmlReader reader = XmlReader.Create(File.OpenRead(fileName), opt))
				{
					if (includeSchema)
					{
						thisValue.Clear();
						thisValue.ReadXml(reader, XmlReadMode.ReadSchema);
					}
					else
					{
						thisValue.ReadXml(reader, XmlReadMode.IgnoreSchema);
					}

					result = true;
				}
			}
			catch
			{
				result = false;
			}

			return result;
		}

		public static bool FromString([NotNull] this DataSet thisValue, [NotNull] string value, bool includeSchema = true, XmlReaderSettings options = null)
		{
			if (value.Length == 0) return false;

			XmlReaderSettings opt = options ?? XmlReaderHelper.CreateSettings();

			bool result;

			try
			{
				using (TextReader textReader = new StringReader(value))
				{
					using (XmlReader reader = XmlReader.Create(textReader, opt))
					{
						if (includeSchema)
						{
							thisValue.Clear();
							thisValue.ReadXml(reader, XmlReadMode.ReadSchema);
						}
						else
						{
							thisValue.ReadXml(reader, XmlReadMode.IgnoreSchema);
						}

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

		public static bool FromXmlNode([NotNull] this DataSet thisValue, [NotNull] XmlNode node, bool includeSchema = true)
		{
			bool result;

			try
			{
				using (XmlReader reader = new XmlNodeReader(node))
				{
					if (includeSchema)
					{
						thisValue.Clear();
						thisValue.ReadXml(reader, XmlReadMode.ReadSchema);
					}
					else
					{
						thisValue.ReadXml(reader, XmlReadMode.IgnoreSchema);
					}

					result = true;
				}
			}
			catch
			{
				result = false;
			}

			return result;
		}

		public static bool FromXElement([NotNull] this DataSet thisValue, [NotNull] XElement element, bool includeSchema = true, ReaderOptions options = ReaderOptions.None)
		{
			bool result;

			try
			{
				using (XmlReader reader = element.CreateReader(options))
				{
					if (includeSchema)
					{
						thisValue.Clear();
						thisValue.ReadXml(reader, XmlReadMode.ReadSchema);
					}
					else
					{
						thisValue.ReadXml(reader, XmlReadMode.IgnoreSchema);
					}

					result = true;
				}
			}
			catch
			{
				result = false;
			}

			return result;
		}

		public static bool SaveAs([NotNull] this DataSet thisValue, [NotNull] string fileName, bool includeSchema = true, XmlWriterSettings options = null)
		{
			if (!IsValid(thisValue, true)) return false;

			XmlWriterSettings opt = options ?? XmlWriterHelper.CreateSettings();
			bool result;

			try
			{
				using (XmlWriter writer = XmlWriter.Create(File.OpenWrite(fileName), opt))
				{
					thisValue.WriteXml(writer, includeSchema ? XmlWriteMode.WriteSchema : XmlWriteMode.IgnoreSchema);
				}

				result = true;
			}
			catch
			{
				result = false;
			}

			return result;
		}

		public static T GetFieldOfTable<T>([NotNull] this DataSet thisValue, [NotNull] string tableName, [NotNull] string fieldName, T defaultValue = default(T), int rowIndex = 0)
		{
			return GetSourceTable(thisValue, tableName).GetFieldOfTable(fieldName, defaultValue, rowIndex);
		}

		public static T GetFieldOfTable<T>([NotNull] this DataSet thisValue, int index, [NotNull] string fieldName, T defaultValue = default(T), int rowIndex = 0)
		{
			return GetSourceTable(thisValue, index).GetFieldOfTable(fieldName, defaultValue, rowIndex);
		}

		public static void CopyTo([NotNull] this DataSet thisValue, [NotNull] DataSet target, bool overwrite = false)
		{
			if (thisValue.Tables.Count == 0) return;
			if (overwrite) target.Tables.Remove(t => thisValue.Tables.Contains(t.TableName));
			target.Merge(thisValue, true, MissingSchemaAction.AddWithKey);
		}

		public static void CopyFrom([NotNull] this DataSet thisValue, [NotNull] DataSet source, bool overwrite = false)
		{
			if (source.Tables.Count == 0) return;
			if (overwrite) thisValue.Tables.Remove(t => source.Tables.Contains(t.TableName));
			thisValue.Merge(thisValue, true, MissingSchemaAction.AddWithKey);
		}
	}
}