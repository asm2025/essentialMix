using System;

namespace essentialMix.Patterns.Pagination
{
	[Serializable]
	public class FilteredPagination : Pagination
	{
		public string Filter { get; set; }
	}
}