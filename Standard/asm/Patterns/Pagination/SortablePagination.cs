using System;
using System.Collections.Generic;
using asm.Patterns.Sorting;

namespace asm.Patterns.Pagination
{
	[Serializable]
	public class SortablePagination : Pagination, ISortable
	{
		/// <inheritdoc />
		public IList<SortField> OrderBy { get; set; }
	}
}