using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace asm.Patterns.Producer
{
	/// <inheritdoc />
	/// <summary>
	/// Very simple implementation of IDataProducer.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DataProducer<T> : IDataProducer<T>
	{
		public DataProducer() 
		{
		}

		public bool CompleteMarked { get; protected set; }

		/// <inheritdoc />
		/// <summary>
		/// Event which is raised when an item of data is produced.
		/// This will not be raised after Completed has been raised.
		/// The parameter for the event is the
		/// </summary>
		/// <seealso cref="E:asm.Components.IDataProducer`1.Data" />
		public event EventHandler<T> Data;

		/// <inheritdoc />
		/// <summary>
		/// Event which is raised when the sequence has finished being
		/// produced. This will be raised exactly once, and after all
		/// Data events (if any) have been raised.
		/// </summary>
		/// <seealso cref="E:asm.Components.IDataProducer`1.Completed" />
		public event EventHandler Completed;

		protected virtual void OnData(T item)
		{
			Data?.Invoke(this, item);
		}

		protected virtual void OnCompleted()
		{
			Completed?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Signals a single item of data.
		/// </summary>
		public void Produce(T item)
		{
			if (CompleteMarked) throw new InvalidOperationException("Cannot produce data after completion of data has been marked.");
			OnData(item);
		}

		/// <summary>
		/// Signals multiple items of data, one at a time, then ends.
		/// Note that this method only exists to support the params modifier.
		/// In every other way it's equivalent to the ProduceAndComplete(IEnumerable{T}).
		/// </summary>
		public void ProduceAndComplete([NotNull] params T[] items)
		{
			ProduceAndComplete((IEnumerable<T>)items);
		}

		/// <summary>
		/// Signals multiple items of data, one at a time, then ends.
		/// </summary>
		public void ProduceAndComplete([NotNull] IEnumerable<T> items)
		{
			foreach (T item in items)
				Produce(item);

			Complete();
		}

		/// <summary>
		/// Pumps the specified items into this data producer, yielding results
		/// as they are received. Before an item is pumped, an internal queue is
		/// created. Pumping an item may yield results at the other end of the pipeline
		/// - any such results are buffered in the queue. When the pumping of a particular
		/// item has finished, all results in the queue are yielded. This means that
		/// naturally streaming operations (projection and filtering) require only a single item
		/// buffer. This producer "ends" when all the items have been produced. If the result
		/// pipeline ends before all items have been pumped, the buffered results are yielded
		/// but no more items are pumped.
		/// </summary>
		/// <typeparam name="TResult">Type of element in the result pipeline</typeparam>
		/// <param name="items">Items to insert into the pipeline</param>
		/// <param name="pipeline">The pipeline to subscribe to for items to yield</param>
		/// <returns>A sequence of yielded items.</returns>
		public IEnumerable<TResult> PumpProduceAndComplete<TResult>([NotNull] IEnumerable<T> items, [NotNull] IDataProducer<TResult> pipeline)
		{
			bool stop = false;
			Queue<TResult> resultBuffer = new Queue<TResult>();
			pipeline.Data += (sender, result) => resultBuffer.Enqueue(result);
			pipeline.Completed += (sender, args) => stop = true;

			foreach (T item in items)
			{
				Produce(item);

				// Un-buffer as we go
				while (resultBuffer.Count > 0)
					yield return resultBuffer.Dequeue();

				if (stop) yield break;
			}

			Complete();

			// Yield any final items which may have been produced due to ending the pipeline
			while (resultBuffer.Count > 0)
			{
				yield return resultBuffer.Dequeue();
			}
		}

		/// <summary>
		/// Signal the end of data. This can only be called once, and
		/// afterwards the Produce method must not be called.
		/// </summary>
		public void Complete()
		{
			if (CompleteMarked) throw new InvalidOperationException("Completion has been already marked.");
			CompleteMarked = true;
			OnCompleted();
		}
	}
}