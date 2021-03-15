using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace essentialMix.Comparers
{
	public class ReferenceComparer<T> : EqualityComparer<T>, IGenericComparer<T>
		where T : class
	{
		public new static ReferenceComparer<T> Default { get; } = new ReferenceComparer<T>();

		public ReferenceComparer()
		{
		}

		public virtual int Compare(T x, T y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (ReferenceEquals(x, null)) return 1;
			if (ReferenceEquals(y, null)) return -1;
			return GetHashCode(x) - GetHashCode(y);
		}

		public override bool Equals(T x, T y) { return ReferenceEquals(x, y); }

		public override int GetHashCode(T obj) { return RuntimeHelpers.GetHashCode(obj); }

		public int Compare(object x, object y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (ReferenceEquals(x, null)) return 1;
			if (ReferenceEquals(y, null)) return -1;
			return GetHashCode(x) - GetHashCode(y);
		}

		public int GetHashCode(object obj) { return RuntimeHelpers.GetHashCode(obj); }
	}

	public class ReferenceComparer : ReferenceComparer<object>
	{
		public ReferenceComparer()
		{
		}
	}
}