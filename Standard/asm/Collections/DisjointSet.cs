using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	// https://www.isical.ac.in/~pdslab/2018/lectures/2018-day17-disjoint-sets.pdf
	// https://www.geeksforgeeks.org/union-find-algorithm-set-2-union-by-rank/
	// https://cp-algorithms.com/data_structures/disjoint_set_union.html#toc-tgt-3
	[DebuggerDisplay("Count = {Count}")]
	[Serializable]
	public class DisjointSet<T> : ICollection<T>, ICollection, IReadOnlyCollection<T>
	{
		[DebuggerDisplay("{Parent} :{Rank}")]
		[StructLayout(LayoutKind.Sequential)]
		protected sealed class Subset
		{
			public Subset([NotNull] T parent)
			{
				Parent = parent;
				Rank = 0;
			}

			public int Rank;
			[NotNull]
			public T Parent;
		}

		[NonSerialized]
		private object _syncRoot;

		/// <inheritdoc />
		public DisjointSet()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		public DisjointSet(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		public DisjointSet(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		public DisjointSet(int capacity, IEqualityComparer<T> comparer)
		{
			if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			Comparer = comparer ?? EqualityComparer<T>.Default;
			Items = new Dictionary<T, Subset>(capacity, comparer);
		}

		public DisjointSet([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		public DisjointSet([NotNull] IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
			: this(enumerable.FastCount().NotBelow(0), comparer)
		{
			foreach (T item in enumerable)
				Add(item);
		}

		/// <inheritdoc cref="ICollection{T}" />
		public int Count => Items.Count;

		/// <inheritdoc />
		bool ICollection<T>.IsReadOnly => false;

		/// <inheritdoc />
		bool ICollection.IsSynchronized => false;

		/// <inheritdoc />
		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		[NotNull]
		public IEqualityComparer<T> Comparer { get; }

		[NotNull]
		protected IDictionary<T, Subset> Items { get; }

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return Items.Keys.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void Add(T item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));
			Items.Add(item, new Subset(item));
		}

		/// <inheritdoc />
		public bool Remove(T item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));
			return Items.Remove(item);
		}

		/// <inheritdoc />
		public void Clear()
		{
			Items.Clear();
		}

		/// <inheritdoc />
		public bool Contains(T item) { return item != null && Items.ContainsKey(item); }

		public void MakeSet([NotNull] T item)
		{
			Subset subset = Items[item];
			subset.Parent = item;
			subset.Rank = 0;
		}

		/// <summary>
		/// Find the subset of an element
		/// </summary>
		/// <param name="item">the item to search for.</param>
		/// <returns>The parent of the item to search for.</returns>
		[NotNull]
		public T Find([NotNull] T item)
		{
			// let it throw error if item was not found
			Subset subset = Items[item];

			while (subset.Rank > 0)
			{
				if (!Items.TryGetValue(subset.Parent, out subset))
				{
					// cleanup if parents were removed
					MakeSet(item);
					return item;
				}

				subset = Items[subset.Parent];
			}

			return subset.Parent;
		}

		/// <summary>
		/// Merge the subsets of two items if they are not in the same set by grouping their parents.
		/// </summary>
		/// <param name="x">The first item.</param>
		/// <param name="y">The second item.</param>
		public void Union([NotNull] T x, [NotNull] T y)
		{
			if (Comparer.Equals(x, y)) return;

			T xroot = Find(x);
			T yroot = Find(y);
			if (Comparer.Equals(xroot, yroot)) return;

			if (Items[xroot].Rank > Items[yroot].Rank)
			{
				Items[yroot].Parent = xroot;
			}
			else
			{
				Subset xsubset = Items[xroot];
				Subset ysubset = Items[yroot];
				xsubset.Parent = yroot;
				if (xsubset.Rank < ysubset.Rank) ysubset.Rank += xsubset.Rank;
			}
		}

		/// <summary>
		/// Checks if the two items belong to the same set.
		/// </summary>
		/// <param name="x">The first item.</param>
		/// <param name="y">The second item.</param>
		public bool IsConnected([NotNull] T x, [NotNull] T y)
		{
			T xroot = Find(x);
			T yroot = Find(y);
			return Comparer.Equals(xroot, yroot);
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);

			int lo = arrayIndex, hi = lo + Count;

			foreach (T value in Items.Keys)
			{
				array[lo++] = value;
				if (lo >= hi) break;
			}
		}

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));

			if (array is T[] tArray)
			{
				CopyTo(tArray, index);
				return;
			}

			/*
			* Catch the obvious case assignment will fail.
			* We can find all possible problems by doing the check though.
			* For example, if the element type of the Array is derived from T,
			* we can't figure out if we can successfully copy the element beforehand.
			*/
			array.Length.ValidateRange(index, Count);
			if (Count == 0) return;

			Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
			Type sourceType = typeof(T);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[] objects)) throw new ArgumentException("Invalid array type", nameof(array));

			int lo = index, hi = lo + Count;

			foreach (T value in Items.Keys)
			{
				objects[lo++] = value;
				if (lo >= hi) break;
			}
		}
	}
}