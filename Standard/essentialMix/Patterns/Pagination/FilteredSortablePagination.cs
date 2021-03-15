using System;

namespace essentialMix.Patterns.Pagination
{
	[Serializable]
	public class FilteredSortablePagination : SortablePagination
	{
		public string Filter { get; set; }
	}
}