using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	///     Key Lookup Reduction optimization: this custom data structure halves the number of
	///     <see cref="IEqualityComparer{T}.GetHashCode(T)" />
	///     and <see cref="IEqualityComparer{T}.Equals(T, T)" /> operations by building in the increment/decrement operations
	///     of a counting dictionary.
	/// </summary>
	public sealed class CountingSet<T>
	{
		[DebuggerDisplay("{Value}, {Count}, {HashCode}")]
		private struct Bucket
		{
			// note: 0 (default) means the bucket is unoccupied
			internal uint HashCode;

			internal T Value;
			internal int Count;
		}

		// picked based on observing unit test performance
		private const double MAX_LOAD = 0.62;

		private readonly IEqualityComparer<T> _comparer;
		private Bucket[] _buckets;
		private int _populatedBucketCount;

		/// <summary>
		///     When we reach this count, we need to resize
		/// </summary>
		private int _nextResizeCount;

		public CountingSet(IEqualityComparer<T> comparer, int capacity = 0)
		{
			_comparer = comparer;
			// we pick the initial length by assuming our current table is one short of the desired
			// capacity and then using our standard logic of picking the next valid table size
			_buckets = new Bucket[GetNextTableSize((int)(capacity / MAX_LOAD) - 1)];
			_nextResizeCount = CalculateNextResizeCount();
		}

		public bool IsEmpty => _populatedBucketCount == 0;

		public void Increment([NotNull] T item)
		{
			if (TryFindBucket(item, out int bucketIndex, out uint hashCode))
			{
				// if a bucket already existed, just update it's count
				++_buckets[bucketIndex].Count;
			}
			else
			{
				// otherwise, claim a new bucket
				_buckets[bucketIndex].HashCode = hashCode;
				_buckets[bucketIndex].Value = item;
				_buckets[bucketIndex].Count = 1;
				++_populatedBucketCount;

				// resize the table if we've grown too full
				if (_populatedBucketCount == _nextResizeCount)
				{
					Bucket[] newBuckets = new Bucket[GetNextTableSize(_buckets.Length)];

					// rehash
					foreach (Bucket oldBucket in _buckets)
						if (oldBucket.HashCode != 0)
						{
							long newBucketIndex = oldBucket.HashCode % newBuckets.Length;
							while (true)
							{
								if (newBuckets[newBucketIndex].HashCode == 0)
								{
									newBuckets[newBucketIndex] = oldBucket;
									break;
								}

								newBucketIndex = (newBucketIndex + 1) % newBuckets.Length;
							}
						}

					_buckets = newBuckets;
					_nextResizeCount = CalculateNextResizeCount();
				}
			}
		}

		public bool TryDecrement([NotNull] T item)
		{
			if (TryFindBucket(item, out int bucketIndex, out _) && _buckets[bucketIndex].Count > 0)
			{
				if (--_buckets[bucketIndex].Count == 0)
				{
					// Note: we can't do this because it messes up our try-find logic
					//// mark as unpopulated. Not strictly necessary because CollectionEquals always does all increments
					//// before all decrements currently. However, this is very cheap to do and allowing the collection to
					//// "just work" in any situation is a nice benefit
					//// this.buckets[bucketIndex].HashCode = 0;
					--_populatedBucketCount;
				}

				return true;
			}

			return false;
		}

		private bool TryFindBucket([NotNull] T item, out int index, out uint hashCode)
		{
			// we convert the raw hash code to a uint to get correctly-signed mod operations
			// and get rid of the zero value so that we can use 0 to mean "unoccupied"
			int rawHashCode = _comparer.GetHashCode(item);
			hashCode = rawHashCode == 0
							? uint.MaxValue
							: unchecked((uint)rawHashCode);

			int bestBucketIndex = (int)(hashCode % _buckets.Length);
			int bucketIndex = bestBucketIndex;

			while (true) // guaranteed to terminate because of how we set load factor
			{
				Bucket bucket = _buckets[bucketIndex];

				if (bucket.HashCode == 0)
				{
					// found unoccupied bucket
					index = bucketIndex;
					return false;
				}

				if (bucket.HashCode == hashCode && _comparer.Equals(bucket.Value, item))
				{
					// found matching bucket
					index = bucketIndex;
					return true;
				}

				// otherwise march on to the next adjacent bucket
				bucketIndex = (bucketIndex + 1) % _buckets.Length;
			}
		}

		private int CalculateNextResizeCount() { return (int)(MAX_LOAD * _buckets.Length) + 1; }

		/// <summary>
		///     Constructs a count dictionary, staying mindful of the known number of elements
		///     so that we bail early (returning null) if we detect a count mismatch
		/// </summary>
		public static CountingSet<TKey> TryBuildElementCountsWithKnownCount<TKey>([NotNull] IEnumerator<TKey> elements, int remaining, IEqualityComparer<TKey> comparer = null)
		{
			if (remaining == 0)
			{
				// don't build the dictionary at all if nothing should be in it
				return null;
			}

			const int MAX_INITIAL_ELEMENT_COUNTS_CAPACITY = 1024;

			CountingSet<TKey> elementCounts = new CountingSet<TKey>(comparer, Math.Min(remaining, MAX_INITIAL_ELEMENT_COUNTS_CAPACITY));
			elementCounts.Increment(elements.Current);

			while (elements.MoveNext())
			{
				if (--remaining < 0)
				{
					// too many elements
					return null;
				}

				elementCounts.Increment(elements.Current);
			}

			return remaining > 0
						? null
						: elementCounts;
		}

		private static int GetNextTableSize(int currentSize) { return Numeric.Math.GetPrime(currentSize); }
	}
}