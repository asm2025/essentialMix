using System;
using System.Collections.Generic;
using System.Diagnostics;
using essentialMix.Patterns.Pagination;

namespace essentialMix.Data.Patterns.Table
{
	[DebuggerDisplay("Columns = {Columns.Count}, Count = {Items.Count}")]
	[Serializable]
	public class TableResult<T> : TableResultBase<T>, ITableResult<T>
	{
		private IPagination _paging;

		/// <inheritdoc />
		public TableResult()
			: this(null, null, null)
		{
		}

		/// <inheritdoc />
		public TableResult(string name)
			: this(name, null, null)
		{
		}

		/// <inheritdoc />
		public TableResult(ICollection<T> items)
			: this(null, items, null)
		{
		}

		/// <inheritdoc />
		public TableResult(string name, ICollection<T> items)
			: this(name, items, null)
		{
		}

		/// <inheritdoc />
		public TableResult(string name, IPagination paging)
			: this(name, null, paging)
		{
		}

		/// <inheritdoc />
		public TableResult(string name, ICollection<T> items, IPagination paging)
			: base(name, items)
		{
			_paging = paging;
		}

		/// <inheritdoc />
		public IPagination Paging => _paging ??= new Pagination();
	}

	[Serializable]
	public class TableResult : TableResult<IDictionary<string, object>>, ITableResult
	{
		/// <inheritdoc />
		public TableResult()
			: this(null, null, null)
		{
		}

		/// <inheritdoc />
		public TableResult(string name)
			: this(name, null, null)
		{
		}

		/// <inheritdoc />
		public TableResult(ICollection<IDictionary<string, object>> items)
			: this(null, items, null)
		{
		}

		/// <inheritdoc />
		public TableResult(string name, ICollection<IDictionary<string, object>> items)
			: this(name, items, null)
		{
		}

		/// <inheritdoc />
		public TableResult(string name, IPagination paging)
			: this(name, null, paging)
		{
		}

		/// <inheritdoc />
		public TableResult(string name, ICollection<IDictionary<string, object>> items, IPagination paging)
			: base(name, items, paging)
		{
		}
	}
}
