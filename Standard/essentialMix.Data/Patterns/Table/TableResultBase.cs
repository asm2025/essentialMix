using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Table
{
	[DebuggerDisplay("Columns = {Columns.Count}")]
	[Serializable]
	public class TableResultBase : ITableResultBase
	{
		/// <inheritdoc />
		protected TableResultBase()
			: this(null)
		{
		}

		protected TableResultBase(string name)
		{
			Name = name;
		}

		/// <inheritdoc />
		public string Name { get; set; }

		/// <inheritdoc />
		public TableColumns Columns { get; } = new TableColumns();

		public virtual bool MapSchemaTable([NotNull] DataTable schema, Func<string, bool> filter = null, Func<string, TableColumnSettings?> onGetSettings = null)
		{
			return Columns.MapSchemaTable(schema, filter, onGetSettings);
		}
	}

	[DebuggerDisplay("Columns = {Columns.Count}, Count = {Items.Count}")]
	[Serializable]
	public class TableResultBase<T> : TableResultBase, ITableResultBase<T>
	{
		/// <inheritdoc />
		public TableResultBase()
			: this(null, null)
		{
		}

		/// <inheritdoc />
		public TableResultBase(ICollection<T> items)
			: this(null, items)
		{
		}

		/// <inheritdoc />
		public TableResultBase(string name)
			: this(name, null)
		{
		}

		/// <inheritdoc />
		public TableResultBase(string name, ICollection<T> items)
			: base(name)
		{
			Items = items ?? new List<T>();
		}

		public ICollection<T> Items { get; }

		public virtual bool MapSchemaTable<TInstance>([NotNull] TInstance instance, Func<string, bool> filter = null, Func<string, TableColumnSettings?> onGetSettings = null)
		{
			return Columns.MapSchemaTable(instance, filter, onGetSettings);
		}
	}
}
