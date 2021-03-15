using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class DataColumnExtension
	{
		[NotNull]
		public static DataColumn Copy([NotNull] this DataColumn thisValue)
		{
			DataColumn newColumn = new DataColumn(thisValue.ColumnName, thisValue.DataType)
			{
				AllowDBNull = thisValue.AllowDBNull,
				AutoIncrement = thisValue.AutoIncrement,
				AutoIncrementSeed = thisValue.AutoIncrementSeed,
				AutoIncrementStep = thisValue.AutoIncrementStep,
				Caption = thisValue.Caption,
				ColumnMapping = thisValue.ColumnMapping,
				DateTimeMode = thisValue.DateTimeMode,
				DefaultValue = thisValue.DefaultValue,
				Expression = thisValue.Expression,
				MaxLength = thisValue.MaxLength,
				Namespace = thisValue.Namespace,
				Prefix = thisValue.Prefix,
				ReadOnly = thisValue.ReadOnly,
				Site = thisValue.Site,
				Unique = thisValue.Unique
			};

			foreach (DictionaryEntry entry in thisValue.ExtendedProperties)
				newColumn.ExtendedProperties[entry.Key] = entry.Value;

			return newColumn;
		}

		public static bool Contains([NotNull] this IEnumerable<DataColumn> thisValue, [NotNull] string name)
		{
			if (name.Length == 0) throw new ArgumentNullException(nameof(name));
			return thisValue.Any(c => c.ColumnName.IsSame(name));
		}
	}
}