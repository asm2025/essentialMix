using System;
using System.Collections;
using System.Collections.Generic;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections
{
	public sealed class MinMaxQueue<T> : IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
		where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
	{
		private readonly dynamic _queueOrStack;
		private readonly ICollection _collection;
		private readonly Action<T> _enqueue;
		private readonly Func<T> _dequeue;
		private readonly T _minimum;
		private readonly T _maximum;
		private readonly Stack<T> _minStack;
		private readonly Stack<T> _maxStack;

		public MinMaxQueue(DequeuePriority priority)
			: this(priority, 0, null)
		{
		}

		public MinMaxQueue(DequeuePriority priority, int capacity)
			: this(priority, capacity, null)
		{
		}

		public MinMaxQueue(DequeuePriority priority, [NotNull] IEnumerable<T> collection)
			: this(priority, 0, collection)
		{
		}

		private MinMaxQueue(DequeuePriority priority, int capacity, IEnumerable<T> collection)
		{
			Priority = priority;

			switch (priority)
			{
				case DequeuePriority.FIFO:
					_queueOrStack = collection == null
										? new Queue<T>(capacity)
										: new Queue<T>(collection);
					_enqueue = e => _queueOrStack.Enqueue(e);
					_dequeue = () => _queueOrStack.Dequeue();
					break;
				case DequeuePriority.LIFO:
					_queueOrStack = collection == null
										? new Stack<T>(capacity)
										: new Stack<T>(collection);
					_enqueue = e => _queueOrStack.Push(e);
					_dequeue = () => _queueOrStack.Pop();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(priority), priority, null);
			}

			_collection = _queueOrStack;
			_minimum = TypeHelper.MinimumOf<T>();
			_maximum = TypeHelper.MaximumOf<T>();
			_minStack = new Stack<T>();
			_maxStack = new Stack<T>();
		}

		public DequeuePriority Priority { get; }

		public int Count => _queueOrStack.Count;

		public object SyncRoot => _collection.SyncRoot;

		public bool IsSynchronized => _collection.IsSynchronized;

		public T Minimum =>
			_minStack.Count == 0
				? _maximum
				: _minStack.Peek();

		public T Maximum =>
			_maxStack.Count == 0
				? _minimum
				: _maxStack.Peek();

		public IEnumerator<T> GetEnumerator() { return _queueOrStack.GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public void Enqueue(T item)
		{
			if (item.IsLessThanOrEqual(Minimum)) _minStack.Push(item);
			if (item.IsGreaterThanOrEqual(Maximum)) _maxStack.Push(item);
			_enqueue(item);
		}

		public T Dequeue()
		{
			T item = _dequeue();
			if (item.IsEqual(Minimum)) _minStack.Pop();
			if (item.IsEqual(Maximum)) _maxStack.Pop();
			return item;
		}

		public T Peek() { return _queueOrStack.Peek(); }

		public void Clear() { _queueOrStack.Clear(); }

		void ICollection.CopyTo(Array array, int index)
		{
			_collection.CopyTo(array, index);
		}

		public void CopyTo(T[] array, int index) { _queueOrStack.CopyTo(array, index); }
		
		public bool Contains(T item) { return _queueOrStack.Contains(item); }

		public T[] ToArray() { return _queueOrStack.ToArray(); }

		public void TrimExcess() { _queueOrStack.TrimExcess(); }
	}
}