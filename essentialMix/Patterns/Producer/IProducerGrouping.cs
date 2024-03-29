﻿namespace essentialMix.Patterns.Producer;

/// <inheritdoc />
/// <summary>
/// IProducerGrouping is to IDataProducer as IGrouping is to IEnumerable:
/// it's basically a data producer with a key. It's used by the GroupBy
/// operator.
/// </summary>
public interface IProducerGrouping<out TKey, TElement> : IDataProducer<TElement>
{
	/// <summary>
	/// The key for this grouping.
	/// </summary>
	TKey Key { get; }
}