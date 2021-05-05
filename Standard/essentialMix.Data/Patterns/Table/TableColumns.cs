﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Patterns.Layout;
using essentialMix.Patterns.Text;

namespace essentialMix.Data.Patterns.Table
{
	[DebuggerDisplay("Count = {Count}")]
	[Serializable]
	public class TableColumns : KeyedCollection<string, ITableColumn>
	{
		private static readonly string[] __columnNames =
		{
			"ColumnName",
			"DataType",
			"ColumnSize",
			"IsKey",
			"IsUnique",
			"IsRowID",
			"AllowDBNull",
			"IsReadOnly",
			"IsAliased",
			"IsExpression",
			"IsHidden"
		};

		/// <inheritdoc />
		public TableColumns()
			: base(StringComparer.OrdinalIgnoreCase)
		{
		}

		/// <inheritdoc />
		[NotNull]
		protected override string GetKeyForItem(ITableColumn item) { return item.Name; }

		internal bool DetailsOwner { get; set; }

		public bool MapSchemaTable([NotNull] DataTable schema, Func<string, bool> filter = null, Func<string, ITableColumnSettings> onGetSettings = null)
		{
			ClearItems();
			if (schema.Columns.Count == 0
				|| schema.Rows.Count == 0
				|| __columnNames.Any(e => !schema.Columns.Contains(e))) return false;

			IDictionary<string, ITableColumnSettings> allSettings = new Dictionary<string, ITableColumnSettings>(StringComparer.OrdinalIgnoreCase);
			
			foreach (DataRow columnInfo in schema.Rows)
			{
				string name = columnInfo.Get<string>(__columnNames[0])?.ToLowerInvariant() ?? throw new InvalidOperationException("Column name is missing.");
				
				// filter the columns
				if (filter?.Invoke(name) == false) continue;

				Type type = columnInfo.Get<Type>(__columnNames[1]) ?? throw new InvalidOperationException("Column data type is missing.");
				// get the default settings for the type and specific column settings overrides then merge them
				ITableColumnSettings settings = SettingsMerger(name, type, onGetSettings);

				ITableColumn column = new TableColumn(name, type)
				{
					Size = columnInfo.Get<int>(__columnNames[2]),
					PrimaryKey = columnInfo.Get<bool>(__columnNames[3]),
					Unique = columnInfo.Get<bool>(__columnNames[4]),
					RowId = columnInfo.Get<bool>(__columnNames[5]),
					AllowDbNull = columnInfo.Get<bool>(__columnNames[6]),
					Sortable = type.IsPrimitive(),
					ReadOnly = columnInfo.Get<bool>(__columnNames[7]),
					Expression = columnInfo.Get<bool>(__columnNames[8]),
					Hidden = columnInfo.Get<bool>(__columnNames[9]),
					Align = type.IsNumeric() ? HorizontalAlignment.Right : HorizontalAlignment.Left,
					Order = Items.Count
				};

				allSettings[column.Name] = settings;
				settings.Apply(column);
				Items.Add(column);
			}

			AdjustWidths(allSettings);
			return true;
		}

		public bool MapSchemaTable<TInstance>([NotNull] TInstance instance, Func<string, bool> filter = null, Func<string, ITableColumnSettings> onGetSettings = null)
		{
			ClearItems();
			IReadOnlyDictionary<string, Type> schema = instance.ToSchema();
			if (schema.Count == 0) return false;

			IDictionary<string, ITableColumnSettings> allSettings = new Dictionary<string, ITableColumnSettings>(StringComparer.OrdinalIgnoreCase);
			
			foreach (KeyValuePair<string, Type> pair in schema)
			{
				// filter the columns
				if (filter?.Invoke(pair.Key) == false) continue;
				// get the default settings for the type and specific column settings overrides then merge them
				ITableColumnSettings settings = SettingsMerger(pair.Key, pair.Value, onGetSettings);

				ITableColumn column = new TableColumn(pair.Key, pair.Value)
				{
					//Size = pair.Get<int>(COLUMN_NAMES[2]),
					//PrimaryKey = pair.Get<bool>(COLUMN_NAMES[3]),
					//Unique = pair.Get<bool>(COLUMN_NAMES[4]),
					//RowId = pair.Get<bool>(COLUMN_NAMES[5]),
					AllowDbNull = pair.Value.IsClass,
					Sortable = pair.Value.IsPrimitive(),
					//ReadOnly = pair.Get<bool>(COLUMN_NAMES[7]),
					//Expression = pair.Get<bool>(COLUMN_NAMES[8]),
					//Hidden = pair.Get<bool>(COLUMN_NAMES[9]),
					Align = pair.Value.IsNumeric() ? HorizontalAlignment.Right : HorizontalAlignment.Left,
					Order = Items.Count
				};

				allSettings[column.Name] = settings;
				settings.Apply(column);
				Items.Add(column);
			}

			AdjustWidths(allSettings);
			return true;
		}

		[NotNull]
		private static ITableColumnSettings SettingsMerger(string name, Type type, Func<string, ITableColumnSettings> onGetSettings)
		{
			ITableColumnSettings target = TableColumnSettingMapping.GetSettings(type);
			ITableColumnSettings overrides = onGetSettings?.Invoke(name);
			TableColumnSettings.Apply(ref target, overrides);
			if (target.TextCasing.HasValue) return target;
			type = type.ResolveType();
			if (type == null) return target;

			if (type.IsArray)
			{
				type = type.GetElementType();
				if (type == null || !type.IsAssignableFrom(typeof(byte)) && !type.IsAssignableFrom(typeof(sbyte))) return target;
				target.Formatting = TableColumnFormatting.Raw;
				target.TextCasing = TextCasing.Upper;
			}
			else if (type == typeof(Guid))
			{
				target.Formatting = TableColumnFormatting.Raw;
				target.TextCasing = TextCasing.Upper;
			}

			return target;
		}

		private void AdjustWidths(IDictionary<string, ITableColumnSettings> allSettings)
		{
			if (DetailsOwner)
			{
				foreach (ITableColumn column in Items)
				{
					column.Weight = column.Hidden == true || !allSettings.TryGetValue(column.Name, out ITableColumnSettings settings) || settings.Weight is null or < 1
										? null
										: settings.Weight.Value.Within(1, 12);
				}
			}
			else
			{
				ITableColumn[] visibleColumns = Items.Where(e => e.Hidden is null or false).ToArray();
				double sum = visibleColumns.Length > 0
								? visibleColumns.Sum(e => allSettings[e.Name].Weight ?? 0)
								: 0.0d;

				if (sum > 0.0d)
				{
					foreach (ITableColumn column in Items)
					{
						column.Weight = column.Hidden == true || !allSettings.TryGetValue(column.Name, out ITableColumnSettings settings) || settings.Weight is null or < 1
											? null
											: (int)Math.Floor(settings.Weight.Value / sum * 12.0d);
					}
				}
				else
				{
					foreach (ITableColumn column in Items) 
						column.Weight = null;
				}
			}
		}
	}
}