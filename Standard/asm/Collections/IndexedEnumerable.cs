using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// Type chaining an <see cref="IEnumerable{T}"/> to allow the iterating code
	/// to detect the first and last entries simply.
	/// </summary>
	public class IndexedEnumerable : IEnumerable<IndexedEntry>, IEnumerable
	{
		/// <summary>
		/// Enumerable we proxy to
		/// </summary>
		private readonly IEnumerable _enumerable;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="enumerable">Collection to enumerate. Must not be null.</param>
		public IndexedEnumerable([NotNull] IEnumerable enumerable)
		{
			_enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns an enumeration of IndexedEntry objects, each of which knows
		/// whether it is the first/last of the enumeration, as well as the
		/// current value.
		/// </summary>
		public IEnumerator<IndexedEntry> GetEnumerator()
		{
			IEnumerator enumerator = _enumerable.GetEnumerator();
			if (!enumerator.MoveNext()) yield break;

			bool isFirst = true;
			bool isLast = false;
			int index = 0;

			while (!isLast)
			{
				object current = enumerator.Current;
				isLast = !enumerator.MoveNext();
				yield return IndexedEntry.Create(isFirst, isLast, current, index++);
				isFirst = false;
			}
		}

		/// <inheritdoc />
		/// <summary>
		/// Non-generic form of GetEnumerator.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	public class IndexedEnumerable<T> : IEnumerable<IndexedEntry<T>>, IEnumerable
	{
		/// <summary>
		/// Enumerable we proxy to
		/// </summary>
		private readonly IEnumerable<T> _enumerable;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="enumerable">Collection to enumerate. Must not be null.</param>
		public IndexedEnumerable([NotNull] IEnumerable<T> enumerable)
		{
			_enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns an enumeration of IndexedEntry objects, each of which knows
		/// whether it is the first/last of the enumeration, as well as the
		/// current value.
		/// </summary>
		public IEnumerator<IndexedEntry<T>> GetEnumerator()
		{
			using (IEnumerator<T> enumerator = _enumerable.GetEnumerator())
			{
				if (!enumerator.MoveNext()) yield break;

				bool isFirst = true;
				bool isLast = false;
				int index = 0;

				while (!isLast)
				{
					T current = enumerator.Current;
					isLast = !enumerator.MoveNext();
					yield return IndexedEntry<T>.Create(isFirst, isLast, current, index++);
					isFirst = false;
				}
			}
		}

		/// <inheritdoc />
		/// <summary>
		/// Non-generic form of GetEnumerator.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}