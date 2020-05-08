using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Patterns.Pagination
{
	public interface IPaginated<out T>
	{
		[NotNull]
		IEnumerable<T> Result { get; }
		
		[NotNull]
		IPagination Pagination { get; }
	}
}