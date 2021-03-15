using System;
using System.Collections.Generic;
using System.Diagnostics;
using essentialMix.Extensions;

namespace essentialMix.Data.Patterns.Table
{
	[DebuggerDisplay("Columns = {Columns.Count}")]
	[Serializable]
	public class TableDetailResult<T> : TableResultBase, ITableDetailResult<T>
	{
		private int _layoutColumns = 2;

		/// <inheritdoc />
		public TableDetailResult()
			: this(null, default(T))
		{
		}

		/// <inheritdoc />
		public TableDetailResult(string name)
			: this(name, default(T))
		{
		}

		/// <inheritdoc />
		public TableDetailResult(T item)
			: this(null, item)
		{
		}

		/// <inheritdoc />
		public TableDetailResult(string name, T item)
		{
			Name = name;
			Columns.DetailsOwner = true;
			Item = item;
		}

		public T Item { get; set; }

		public int LayoutColumns
		{
			get => _layoutColumns;
			set => _layoutColumns = value.Within(0, 12);
		}
	}

	[Serializable]
	public class TableDetailResult : TableDetailResult<IDictionary<string, object>>, ITableDetailResult
	{
		/// <inheritdoc />
		public TableDetailResult()
			: this(null, null)
		{
		}

		/// <inheritdoc />
		public TableDetailResult(string name)
			: this(name, null)
		{
		}

		/// <inheritdoc />
		public TableDetailResult(IDictionary<string, object> item)
			: this(null, item)
		{
		}

		/// <inheritdoc />
		public TableDetailResult(string name, IDictionary<string, object> item)
			: base(name, item ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase))
		{
		}
	}
}
