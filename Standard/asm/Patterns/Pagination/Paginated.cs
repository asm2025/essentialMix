using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Patterns.Pagination
{
	[Serializable]
	public class Paginated<T> : IPaginated<T>
	{
		public Paginated([NotNull] IEnumerable<T> result, [NotNull] IPagination pagination)
		{
			Result = result;
			Pagination = pagination;
		}

		/// <inheritdoc />
		public IEnumerable<T> Result { get; }

		/// <inheritdoc />
		public IPagination Pagination { get; }
	}
}