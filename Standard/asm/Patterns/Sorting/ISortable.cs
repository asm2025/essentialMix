using System.Collections.Generic;

namespace asm.Patterns.Sorting
{
	public interface ISortable
	{
		IList<SortField> OrderBy { get; set; }
	}
}