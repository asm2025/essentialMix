using System;
using System.Data;
using System.Linq;
using asm.Data.Extensions;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Data.Patterns.Provider;

namespace asm.Data.Patterns.Settings
{
	public abstract class AppSettings<TProvider, TDbType>
		where TDbType : struct, IComparable
		where TProvider : IDataProvider<TDbType>
	{
		protected AppSettings([NotNull] TProvider provider)
			: this(provider, string.Empty) { }

		protected AppSettings([NotNull] TProvider provider, [NotNull] string tableName)
		{
			Provider = provider;
			TableName = tableName;
		}

		public string TableName
		{
			get => Table.TableName;
			set => Table.TableName = value?.Trim();
		}

		[NotNull]
		public TProvider Provider { get; set; }

		[NotNull]
		protected DataTable Table { get; } = new DataTable();

		public virtual T Get<T>([NotNull] string name, T defaultValue)
		{
			AssertProperty(name);
			return Table.GetFieldOfTable(name, defaultValue);
		}

		public virtual void Set<T>([NotNull] string name, T value)
		{
			AssertProperty(name);
			EnsureRows();

			DataRow row = Table.Rows[0];
			row[name] = value != null ? (object)value : DBNull.Value;
			row.AcceptChanges();
		}

		public virtual bool Load(bool resetSchema = false)
		{
			bool result;

			try
			{
				Table.Clear();

				if (resetSchema)
				{
					Table.Clear();
					Table.PrimaryKey = Array.Empty<DataColumn>();
					Table.Columns.Clear();
				}

				result = !string.IsNullOrWhiteSpace(Table.TableName) && LoadSettingsTable();
			}
			catch
			{
				result = false;
			}

			return result;
		}

		public virtual bool Save()
		{
			if (string.IsNullOrWhiteSpace(Table.TableName) || Table.Rows.Count == 0) return false;

			bool result;

			try
			{
				result = SaveSettingsTable();
			}
			catch
			{
				result = false;
			}

			return result;
		}

		protected abstract bool LoadSettingsTable();
		protected abstract bool SaveSettingsTable();

		protected virtual void EnsureRows()
		{
			if (Table.Rows.Count > 0) return;
			DataRow row = Table.NewRow();
			Table.Rows.Add(row);
			if (Table.PrimaryKey.Length == 0) return;

			foreach (DataColumn column in Table.PrimaryKey.Where(c => c.DataType.IsIntegral())) row[column.ColumnName] = -1;

			row.AcceptChanges();
		}

		protected void AssertProperty([NotNull] string name)
		{
			if (Table.Columns.Contains(name)) return;
			throw new NotSupportedException(string.Concat("The property '", name, "' is not found in the table columns."));
		}
	}
}