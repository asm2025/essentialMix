using System;
using asm.Extensions;

namespace asm.Patterns.Pagination
{
	[Serializable]
	public class Pagination : IPagination
	{
		public const int PAGE_SIZE = 10;

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
			get => _pageSize ??= PAGE_SIZE;
			set => _pageSize = value.IfLessThanOrEqual(0, PAGE_SIZE);
		}

		public long Count
		{
			get => _count ??= 0L;
			set => _count = value.NotBelow(0L);
		}
	}
}