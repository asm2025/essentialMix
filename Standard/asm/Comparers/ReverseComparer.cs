using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Comparers
{
	public class ReverseComparer<T> : GenericComparer<T>
	{
		public new static ReverseComparer<T> Default { get; } = new ReverseComparer<T>();

		public ReverseComparer()
			: this(null, null)
		{
		}

		public ReverseComparer([NotNull] IComparer<T> comparer)
			: this(comparer, null)
		{
		}

		public ReverseComparer([NotNull] IComparer<T> comparer, IEqualityComparer<T> equalityComparer)
			: base(comparer, equalityComparer)
		{
			Original = comparer;
		}

		[NotNull]
		public IComparer<T> Original { get; }
		public override int Compare(T x, T y) { return base.Compare(y, x); }

		public new int Compare(object x, object y) { return base.Compare(y, x); }
	}
}
