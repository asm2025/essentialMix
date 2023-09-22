using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Pagination;

[Serializable]
public class Paginated<T, TPagination> : IPaginated<T, TPagination>
	where TPagination : IPagination
{
	public Paginated([NotNull] IEnumerable<T> result, TPagination pagination)
	{
		Result = result;
		Pagination = pagination;
	}

	/// <inheritdoc />
	public IEnumerable<T> Result { get; }

	/// <inheritdoc />
	public TPagination Pagination { get; }
}

[Serializable]
public class Paginated<T> : Paginated<T, IPagination>, IPaginated<T>
{
	public Paginated([NotNull] IEnumerable<T> result, IPagination pagination)
		: base(result, pagination)
	{
	}
}