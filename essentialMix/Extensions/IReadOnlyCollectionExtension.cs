using System;
using System.Collections.Generic;
using System.Linq;
using essentialMix.Collections;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class IReadOnlyCollectionExtension
{
	public static IEnumerable<IReadOnlyCollection<T>> Partition<T>([NotNull] this IReadOnlyCollection<T> thisValue, int size, PartitionSize type)
	{
		if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
		if (size == 0 || thisValue.Count == 0) return Enumerable.Empty<IReadOnlyCollection<T>>();
		if (type == PartitionSize.TotalCount) size = (int)Math.Ceiling(thisValue.Count / (double)size);
		return thisValue.Partition(size);
	}

	public static IEnumerable<IReadOnlyCollection<T>> PartitionUnique<T>([NotNull] this IReadOnlyCollection<T> thisValue, int size, PartitionSize type = PartitionSize.PerPartition, IEqualityComparer<T> comparer = null)
	{
		if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
		if (size == 0 || thisValue.Count == 0) return Enumerable.Empty<IReadOnlyCollection<T>>();
		if (type == PartitionSize.TotalCount) size = (int)Math.Ceiling(thisValue.Count / (double)size);
		return thisValue.PartitionUnique(size, comparer);
	}
}