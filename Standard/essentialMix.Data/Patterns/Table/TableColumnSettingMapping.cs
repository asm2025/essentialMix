using System;
using System.Collections.Generic;
using System.Numerics;
using essentialMix.Extensions;
using essentialMix.Patterns.Text;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Table
{
	public static class TableColumnSettingMapping
	{
		private static readonly IReadOnlyDictionary<Type, TableColumnSettings> __settings = new Dictionary<Type, TableColumnSettings>
		{
			{
				typeof(bool), new TableColumnSettings
				{
					Sortable = true,
					Weight = 1
				}
			},
			{
				typeof(char), new TableColumnSettings
				{
					Sortable = true,
					Weight = 1
				}
			},
			{
				typeof(sbyte), new TableColumnSettings
				{
					Hidden = true,
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.Raw,
					CustomFormat = "N",
					TextCasing = TextCasing.Upper
				}
			},
			{
				typeof(byte), new TableColumnSettings
				{
					Hidden = true,
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.Raw,
					CustomFormat = "N",
					TextCasing = TextCasing.Upper
				}
			},
			{
				typeof(Guid), new TableColumnSettings
				{
					Hidden = true,
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.Raw,
					TextCasing = TextCasing.Upper
				}
			},
			{
				typeof(short), new TableColumnSettings
				{
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.Integer
				}
			},
			{
				typeof(ushort), new TableColumnSettings
				{
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.Integer
				}
			},
			{
				typeof(ushort), new TableColumnSettings
				{
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.Integer
				}
			},
			{
				typeof(int), new TableColumnSettings
				{
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.Integer
				}
			},
			{
				typeof(uint), new TableColumnSettings
				{
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.Integer
				}
			},
			{
				typeof(long), new TableColumnSettings
				{
					Sortable = true,
					Weight = 2,
					Formatting = TableColumnFormatting.Integer
				}
			},
			{
				typeof(ulong), new TableColumnSettings
				{
					Sortable = true,
					Weight = 2,
					Formatting = TableColumnFormatting.Integer
				}
			},
			{
				typeof(BigInteger), new TableColumnSettings
				{
					Sortable = true,
					Weight = 2,
					Formatting = TableColumnFormatting.Integer
				}
			},
			{
				typeof(float), new TableColumnSettings
				{
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.Float
				}
			},
			{
				typeof(double), new TableColumnSettings
				{
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.Float
				}
			},
			{
				typeof(decimal), new TableColumnSettings
				{
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.Float
				}
			},
			{
				typeof(string), new TableColumnSettings
				{
					Sortable = true
				}
			},
			{
				typeof(DateTime), new TableColumnSettings
				{
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.DateTime
				}
			},
			{
				typeof(TimeSpan), new TableColumnSettings
				{
					Sortable = true,
					Weight = 1,
					Formatting = TableColumnFormatting.Time
				}
			},
			{
				typeof(object), new TableColumnSettings
				{
					Sortable = false
				}
			}
		}.AsReadOnly();

		[NotNull]
		public static ITableColumnSettings GetSettings([NotNull] Type type)
		{
			type = type.ResolveType() ?? throw new ArgumentException($"Type {type} could not be resolved.");
			if (type.IsArray) type = type.GetElementType() ?? throw new ArgumentException($"Type {type} could not be resolved.");

			if (!__settings.TryGetValue(type, out TableColumnSettings settings)) settings = __settings[typeof(object)];
			return settings;
		}
	}
}
