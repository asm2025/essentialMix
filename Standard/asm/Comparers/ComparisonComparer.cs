using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Comparers
{
	/// <summary>
	/// Utility to build an IComparer implementation from a Comparison delegate,
	/// and a static method to do the reverse.
	/// </summary>
	public class ComparisonComparer<T> : IGenericComparer<T>
	{
		public static ComparisonComparer<T> Default { get; } = new ComparisonComparer<T>();

		public ComparisonComparer()
			: this(null, null) { }

		public ComparisonComparer(Comparison<T> comparison)
			: this(comparison, null) { }

		public ComparisonComparer(Comparison<T> comparison, EqualityComparison<T> equalityComparison)
		{
			Comparison = comparison ?? ComparisonComparer.CreateComparison(Comparer<T>.Default);
			EqualityComparison = equalityComparison ?? ComparisonComparer.CreateEqualityComparison(EqualityComparer<T>.Default);
		}

		protected Comparison<T> Comparison { get; }
		protected EqualityComparison<T> EqualityComparison { get; }

		public virtual int Compare(T x, T y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return 1;
			if (y == null) return -1;
			return Comparison(x, y);
		}

		public virtual bool Equals(T x, T y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (x == null) return false;
			return y != null && EqualityComparison(x, y);
		}

		public int GetHashCode(T obj) { return obj.GetHashCode(); }

		public int Compare(object x, object y) { return ReferenceComparer.Default.Compare(x, y); }

		public new bool Equals(object x, object y) { return ReferenceComparer.Default.Equals(x, y); }

		public int GetHashCode(object obj) { return ReferenceComparer.Default.GetHashCode(obj); }
	}

	public static class ComparisonComparer
	{
		[NotNull] public static Comparison<T> CreateComparison<T>([NotNull] IComparer<T> comparer) { return comparer.Compare; }

		[NotNull] public static EqualityComparison<T> CreateEqualityComparison<T>([NotNull] IEqualityComparer<T> comparer) { return comparer.Equals; }

		[NotNull] public static ComparisonComparer<T> Create<T>() { return Create<T>(null, null); }

		[NotNull]
		public static ComparisonComparer<T> Create<T>(Comparison<T> comparison, EqualityComparison<T> equalityComparison)
		{
			return new ComparisonComparer<T>(comparison, equalityComparison);
		}

		[NotNull] public static ComparisonComparer<T> FromComparison<T>(Comparison<T> comparison) { return Create(comparison, null); }

		[NotNull] public static ComparisonComparer<T> FromEqualityComparison<T>(EqualityComparison<T> equalityComparison) { return Create(null, equalityComparison); }
	}
}
