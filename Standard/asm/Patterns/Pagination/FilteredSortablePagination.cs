using System;

namespace asm.Patterns.Pagination
{
	[Serializable]
	public class FilteredSortablePagination : SortablePagination
	{
		public string Filter { get; set; }
	}
}