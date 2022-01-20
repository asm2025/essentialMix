using System.Collections.Generic;
using essentialMix.Extensions;
using essentialMix.Patterns.Pagination;
using essentialMix.Patterns.Sorting;

namespace essentialMix.Data.Patterns.Parameters;

public struct ListSettings : IListSettings
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
	public string FilterExpression { get; set; }

	/// <inheritdoc />
	public IList<SortField> OrderBy { get; set; }
}