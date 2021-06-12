using System;
using System.Collections;
using System.Threading;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public interface IProducerQueue<T>
	{
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
}
