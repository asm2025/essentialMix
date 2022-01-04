using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer;

public interface IProducerQueue<out TQueue, T>
	where TQueue : ICollection, IReadOnlyCollection<T>
{
	[NotNull]
	TQueue Queue { get; }

	/// <inheritdoc cref="ICollection.IsSynchronized" />
	bool IsSynchronized { get; }

	/// <inheritdoc cref="ICollection.SyncRoot" />
	[NotNull]
	object SyncRoot { get; }

	/// <summary>
	/// Use lock statement or <see cref="Monitor" /> enter methods with <see cref="SyncRoot" /> to synchronize the queue
	/// </summary>
	bool TryDequeue(out T item);
	/// <summary>
	/// Use lock statement or <see cref="Monitor" /> enter methods with <see cref="SyncRoot" /> to synchronize the queue
	/// </summary>
	bool TryPeek(out T item);
	/// <summary>
	/// Use lock statement or <see cref="Monitor" /> enter methods with <see cref="SyncRoot" /> to synchronize the queue
	/// </summary>
	void RemoveWhile([NotNull] Predicate<T> predicate);
}

public interface IProducerQueue<T> : IProducerQueue<Queue<T>, T>
{
}