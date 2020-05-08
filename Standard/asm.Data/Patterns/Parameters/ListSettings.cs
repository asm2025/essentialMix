using System.Collections.Generic;
using asm.Extensions;
using asm.Patterns.Pagination;
using asm.Patterns.Sorting;

namespace asm.Data.Patterns.Parameters
{
	public struct ListSettings : IIncludeSettings, IFilterSettings, ISortable, IPagination
	{
		private int? _page;
		private int? _pageSize;
		private long? _count;

		/// <inheritdoc />
		public int Page
		{
			get => _page ??= 1; 
			set => _page = value.NotBelow(1);
		}

		/// <inheritdoc />
		public int PageSize
		{
			get => _pageSize ??= Pagination.PAGE_SIZE;
			set => _pageSize = value.IfLessThanOrEqual(0, Pagination.PAGE_SIZE);
		}

		public long Count
		{
			get => _count ??= 0L;
			set => _count = value.NotBelow(0L);
		}

		/// <inheritdoc />
		public IList<string> Include { get; set; }

		/// <inheritdoc />
		public DynamicFilter Filter { get; set; }

		/// <inheritdoc />
		public IList<SortField> OrderBy { get; set; }
	}
}