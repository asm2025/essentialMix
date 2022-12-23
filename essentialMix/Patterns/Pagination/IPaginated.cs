using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Pagination;

public interface IPaginated<out T, TPagination>
	where TPagination : IPagination
{
	[NotNull]
	IEnumerable<T> Result { get; }

	[NotNull]
	TPagination Pagination { get; }
}

public interface IPaginated<out T> : IPaginated<T, IPagination>
{
}
