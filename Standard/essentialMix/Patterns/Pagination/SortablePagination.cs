using System;
using System.Collections.Generic;
using essentialMix.Patterns.Sorting;

namespace essentialMix.Patterns.Pagination;

[Serializable]
public class SortablePagination : Pagination, ISortable
{
	/// <inheritdoc />
	public IList<SortField> OrderBy { get; set; }
}