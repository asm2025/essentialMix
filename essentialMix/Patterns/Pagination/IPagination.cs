namespace essentialMix.Patterns.Pagination;

public interface IPagination
{
	int Page { get; set; }
	int PageSize { get; set; }
	long Count { get; set; }
}