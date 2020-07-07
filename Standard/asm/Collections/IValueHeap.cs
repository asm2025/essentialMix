using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	public interface IValueHeap<T> : IHeap<T>
	{
		[NotNull]
		IComparer<T> Comparer { get; }
	}
}