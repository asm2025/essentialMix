using System;

namespace asm.Patterns.Pagination
{
	[Serializable]
	public class FilteredPagination : Pagination
	{
		public string Filter { get; set; }
	}
}