using System.Collections.Generic;

namespace essentialMix.Patterns.Sorting
{
	public interface ISortable
	{
		IList<SortField> OrderBy { get; set; }
	}
}