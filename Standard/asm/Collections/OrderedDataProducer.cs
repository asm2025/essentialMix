using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// A DataProducer with ordering capabilities
	/// </summary>
	/// <remarks>Note that this may cause data to be buffered</remarks>
	/// <typeparam name="T"></typeparam>
	internal class OrderedDataProducer<T> : IOrderedDataProducer<T>
	{
		private bool _dataHasEnded;
		private List<T> _buffer;

		public OrderedDataProducer([NotNull] IDataProducer<T> baseProducer)
			: this(baseProducer, null)
		{
		}

		/// <summary>
		/// Create a new OrderedDataProducer
		/// </summary>
		/// <param name="baseProducer">The base source which will supply data</param>
		/// <param name="comparer">The comparer to use when sorting the data (once complete)</param>
		public OrderedDataProducer([NotNull] IDataProducer<T> baseProducer, IComparer<T> comparer)
		{
			Comparer = comparer ?? Comparer<T>.Default;

			BaseProducer = baseProducer;
			baseProducer.DataProduced += OriginalDataProduced;
			baseProducer.EndOfData += EndOfOriginalData;
		}

		public event EventHandler<T> DataProduced;
		public event EventHandler EndOfData;

		public IDataProducer<T> BaseProducer { get; }

		public IComparer<T> Comparer { get; }

		protected virtual void OnDataProduced(T item) { DataProduced?.Invoke(this, item); }

		protected virtual void OnEndOfData() { EndOfData?.Invoke(this, EventArgs.Empty); }

		private void OriginalDataProduced(object sender, T item)
		{
			if (_dataHasEnded) throw new InvalidOperationException("EndOfData already occurred");

			if (DataProduced != null)
			{
				// only get excited if somebody is listening
				if (_buffer == null) _buffer = new List<T>();
				_buffer.Add(item);
			}
		}

		private void EndOfOriginalData(object sender, EventArgs args)
		{
			if (_dataHasEnded) throw new InvalidOperationException("EndOfData already occurred");
			_dataHasEnded = true;

			// only do the sort if somebody is still listening
			if (DataProduced != null && _buffer != null)
			{
				_buffer.Sort(Comparer);

				foreach (T item in _buffer) OnDataProduced(item);
			}
			_buffer = null;
			OnEndOfData();
		}
	}
}