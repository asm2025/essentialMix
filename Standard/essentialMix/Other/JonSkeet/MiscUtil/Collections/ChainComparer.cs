using System.Collections.Generic;
using System.Linq;
using essentialMix.Collections;
using essentialMix.Comparers;
using essentialMix.Threading;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Other.JonSkeet.MiscUtil.Collections
{
	/// <summary>
	/// Comparer to daisy-chain two existing comparers and 
	/// apply in sequence (i.e. sort by x then y)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ChainComparer<T> : IGenericComparer<T>
	{
		public ChainComparer()
			: this(null, null) { }

		public ChainComparer(IEnumerable<IComparer<T>> comparers)
			: this(comparers, null) { }

		public ChainComparer(IEnumerable<IComparer<T>> comparers, IEnumerable<IEqualityComparer<T>> equalityComparers)
		{
			if (comparers != null)
			{
				Comparers.AddRange(comparers.Where(e => e != null)
											.Select(e => new Lockable<IComparer<T>>(e)));
			}

			if (equalityComparers != null)
			{
				EqualityComparers.AddRange(equalityComparers.Where(e => e != null)
															.Select(e => new Lockable<IEqualityComparer<T>>(e)));
			}
		}

		[NotNull]
		protected SynchronizedList<IComparer<T>> Comparers { get; } = new SynchronizedList<IComparer<T>>
		{
			new Lockable<IComparer<T>>(Comparer<T>.Default)
		};

		[NotNull]
		protected SynchronizedList<IEqualityComparer<T>> EqualityComparers { get; } = new SynchronizedList<IEqualityComparer<T>>
		{
			new List<IEqualityComparer<T>>(new List<IEqualityComparer<T>>
			{
				EqualityComparer<T>.Default
			})
		};

		public virtual int Compare(T x, T y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return 1;
			if (y == null) return -1;
			return Comparers.Select(lockable => lockable.Value.Compare(x, y)).FirstOrDefault(cmp => cmp != 0);
		}

		public virtual bool Equals(T x, T y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (x == null) return false;
			return y != null && EqualityComparers.Any(lockable => lockable.Value.Equals(x, y));
		}

		public int GetHashCode(T obj) { return obj.GetHashCode(); }

		public int Compare(object x, object y) { return ReferenceComparer.Default.Compare(x, y); }

		public new bool Equals(object x, object y) { return ReferenceComparer.Default.Equals(x, y); }

		public int GetHashCode(object obj) { return ReferenceComparer.Default.GetHashCode(obj); }
	}
}
