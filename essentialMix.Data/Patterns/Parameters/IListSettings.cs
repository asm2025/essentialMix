using essentialMix.Patterns.Pagination;
using essentialMix.Patterns.Sorting;

namespace essentialMix.Data.Patterns.Parameters;

public interface IListSettings : IIncludeSettings, IFilterSettings, ISortable, IPagination
{
}