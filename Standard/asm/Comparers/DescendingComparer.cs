using System;
using Other.JonSkeet.MiscUtil.Collections;

namespace asm.Comparers
{
	public sealed class DescendingComparer<T> : ReverseComparer<T>
		where T : struct, IComparable
	{
		public new static DescendingComparer<T> Default { get; } = new DescendingComparer<T>();

		public DescendingComparer()
		{
		}
	}
}