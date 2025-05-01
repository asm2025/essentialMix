using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Pagination;

[Serializable]
public class Paginated<T, TPagination>([NotNull] IEnumerable<T> result, TPagination pagination)
	: IPaginated<T, TPagination>
	where TPagination : IPagination
{
	/// <inheritdoc />
	public IEnumerable<T> Result { get; } = result;

	/// <inheritdoc />
	public TPagination Pagination { get; } = pagination;
}

[Serializable]
public class Paginated<T>([NotNull] IEnumerable<T> result, IPagination pagination)
	: Paginated<T, IPagination>(result, pagination), IPaginated<T>;