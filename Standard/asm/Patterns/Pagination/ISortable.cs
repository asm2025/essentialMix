using System.Collections.Generic;
using asm.Patterns.Sorting;

namespace asm.Patterns.Pagination
{
	public interface ISortable
	{
		/// <summary>
		/// https://github.com/StefH/System.Linq.Dynamic.Core/wiki/Dynamic-Expressions
		/// </summary>
		IList<SortField> OrderBy { get; set; }
	}
}