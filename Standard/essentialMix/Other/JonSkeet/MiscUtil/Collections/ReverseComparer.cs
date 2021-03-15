using System.Collections.Generic;
using essentialMix.Comparers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Other.JonSkeet.MiscUtil.Collections
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

		public ReverseComparer(IComparer<T> comparer, IEqualityComparer<T> equalityComparer)
			: base(comparer ?? Comparer<T>.Default, equalityComparer)
		{
			Original = comparer ?? Comparer<T>.Default;
		}

		[NotNull]
		public IComparer<T> Original { get; }
		public override int Compare(T x, T y) { return base.Compare(y, x); }

		public new int Compare(object x, object y) { return base.Compare(y, x); }
	}
}
