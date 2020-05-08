﻿using System;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <inheritdoc />
	/// <summary>
	/// Simple implementation of IProducerGrouping which proxies to an existing
	/// IDataProducer.
	/// </summary>
	public class ProducerGrouping<TKey, TElement> : IProducerGrouping<TKey, TElement>
	{
		/// <summary>
		/// Constructs a new grouping with the given key
		/// </summary>
		public ProducerGrouping(TKey key, [NotNull] IDataProducer<TElement> source)
		{
			Key = key;
			Source = source;
			Source.DataProduced += DataProduced;
			Source.EndOfData += EndOfData;
		}

		/// <inheritdoc />
		/// <summary>
		/// Event which is raised when an item of data is produced.
		/// This will not be raised after EndOfData has been raised.
		/// The parameter for the event is the
		/// </summary>
		public event EventHandler<TElement> DataProduced;

		/// <inheritdoc />
		/// <summary>
		/// Event which is raised when the sequence has finished being
		/// produced. This will be raised exactly once, and after all
		/// DataProduced events (if any) have been raised.
		/// </summary>
		public event EventHandler EndOfData;

		protected virtual void OnDataProduced(TElement item)
		{
			DataProduced?.Invoke(this, item);
		}

		protected virtual void OnEndOfData()
		{
			EndOfData?.Invoke(this, EventArgs.Empty);
		}

		[NotNull]
		protected IDataProducer<TElement> Source { get; }

		/// <inheritdoc />
		/// <summary>
		/// The key for this grouping.
		/// </summary>
		public TKey Key { get; }
	}
}