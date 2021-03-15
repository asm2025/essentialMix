using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Producer
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
			baseProducer.Data += OriginalDataProduced;
			baseProducer.Completed += EndOfOriginalData;
		}

		public event EventHandler<T> Data;
		public event EventHandler Completed;

		public IDataProducer<T> BaseProducer { get; }

		public IComparer<T> Comparer { get; }

		protected virtual void OnDataProduced(T item) { Data?.Invoke(this, item); }

		protected virtual void OnEndOfData() { Completed?.Invoke(this, EventArgs.Empty); }

		private void OriginalDataProduced(object sender, T item)
		{
			if (_dataHasEnded) throw new InvalidOperationException("Completed already occurred");
			if (Data == null) return;
			// only get excited if somebody is listening
			_buffer ??= new List<T>();
			_buffer.Add(item);
		}

		private void EndOfOriginalData(object sender, EventArgs args)
		{
			if (_dataHasEnded) throw new InvalidOperationException("Completed already occurred");
			_dataHasEnded = true;

			// only do the sort if somebody is still listening
			if (Data != null && _buffer != null)
			{
				_buffer.Sort(Comparer);

				foreach (T item in _buffer) OnDataProduced(item);
			}
			_buffer = null;
			OnEndOfData();
		}
	}
}