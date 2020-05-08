using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace asm.Statistics
{
	public class ChainState<T> : IReadOnlyList<T>, IComparable<ChainState<T>>, IComparable, IEquatable<ChainState<T>>
		where T : IEquatable<T>
	{
		[NotNull]
		private readonly IReadOnlyList<T> _items;

		public ChainState([NotNull] IEnumerable<T> items)
		{
			T[] array;

			switch (items)
			{
				case ISet<T> set:
					array = new T[set.Count];
					if (set.Count > 0) set.CopyTo(array, 0);
					break;
				case ICollection<T> collection:
					array = new T[collection.Count];
					if (collection.Count > 0) collection.CopyTo(array, 0);
					break;
				default:
					array = items.ToArray();
					break;
			}

			_items = array;
		}

		public ChainState([NotNull] params T[] items)
		{
			T[] array = new T[items.Length];
			if (items.Length > 0) Array.Copy(items, array, array.Length);
			_items = array;
		}

		/// <inheritdoc />
		public int Count => _items.Count;

		/// <inheritdoc />
		public T this[int index] => _items[index];

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return _items.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public int CompareTo(object obj) { return CompareTo(obj as ChainState<T>); }

		/// <inheritdoc />
		public int CompareTo(ChainState<T> other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return -1;
			return GetHashCode().CompareTo(other.GetHashCode());
		}

		/// <inheritdoc />
		public override int GetHashCode() { return _items.GetHashCode(); }

		/// <inheritdoc />
		public override bool Equals(object obj) { return Equals(obj as ChainState<T>); }

		/// <inheritdoc />
		public bool Equals(ChainState<T> other) { return !(other is null) && other.Count == Count && this.SequenceEqual(other); }

		public static bool operator ==(ChainState<T> a, ChainState<T> b)
		{
			if (ReferenceEquals(a, b)) return true;
			if (a is null || b is null) return false;
			return a.Equals(b);
		}

		public static bool operator !=(ChainState<T> a, ChainState<T> b) { return !(a == b); }
	}
}
