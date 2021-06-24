using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using essentialMix.Collections.DebugView;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[ComVisible(false)]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
	[Serializable]
	public class LinkedCircularBuffer<T> : ICollection<T>, ICollection, IReadOnlyCollection<T>, IEnumerable
	{
		[Serializable]
		private struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
		{
			private readonly LinkedCircularBuffer<T> _buffer;
			private readonly LinkedList<T> _items;
			private readonly int _version;

			private LinkedListNode<T> _node;
			private bool _done;

			internal Enumerator([NotNull] LinkedCircularBuffer<T> buffer)
			{
				_buffer = buffer;
				_items = buffer._items;
				_node = null;
				_done = false;
				_version = buffer._version;
			}

			/// <inheritdoc />
			public void Dispose() { }

			/// <inheritdoc />
			public bool MoveNext()
			{
				if (_version == _buffer._version && !_done)
				{
					_node = _node == null
								? _items.First
								: _node.Next;
					_done = _node == _items.Last;
					return true;
				}
				return MoveNextRare();
			}

			private bool MoveNextRare()
			{
				if (_version != _buffer._version) throw new InvalidOperationException();
				_node = null;
				_done = true;
				return false;
			}

			/// <inheritdoc />
			public T Current
			{
				get
				{
					if (_node == null) throw new InvalidOperationException();
					return _node.Value;
				}
			}

			/// <inheritdoc />
			object IEnumerator.Current => Current;

			/// <inheritdoc />
			void IEnumerator.Reset()
			{
				if (_version != _buffer._version) throw new InvalidOperationException();
				_node = null;
				_done = false;
			}
		}

		private readonly ICollection _collection;
		private readonly Action<T> _onItemDispose;

		private int _capacity;
		private int _version;
		private LinkedList<T> _items;

		/// <summary>
		/// Initializes a new instance of the <see cref="LinkedCircularBuffer{T}" /> class with the specified capacity where capacity cannot be less than 1.
		/// </summary>
		/// <param name="capacity">The initial capacity. Must be greater than <c>0</c>.</param>
		public LinkedCircularBuffer(int capacity)
			: this(capacity, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LinkedCircularBuffer{T}" /> class with the specified capacity where capacity cannot be less than 1.
		/// </summary>
		/// <param name="capacity">The initial capacity. Must be greater than <c>0</c>.</param>
		/// <param name="onItemReplace">A method to be called when an item is about to be overwritten.</param>
		public LinkedCircularBuffer(int capacity, Action<T> onItemReplace)
		{
			if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			_capacity = capacity;
			_items = new LinkedList<T>();
			_collection = _items;

			Type type = typeof(T);

			if (typeof(IDisposable).IsAssignableFrom(type))
			{
				_onItemDispose = onItemReplace != null
									? item =>
									{
										try
										{
											onItemReplace(item);
											((IDisposable)item)?.Dispose();
										}
										catch (ObjectDisposedException)
										{
											// ignored
										}
									}
				: item =>
				{
					try
					{
						((IDisposable)item)?.Dispose();
					}
					catch (ObjectDisposedException)
					{
						// ignored
					}
				};
			}
			else if (type == typeof(object))
			{
				_onItemDispose = onItemReplace != null
									? item =>
									{
										try
										{
											onItemReplace(item);
											IDisposable disposable = item as IDisposable;
											disposable?.Dispose();
										}
										catch (ObjectDisposedException)
										{
											// ignored
										}
									}
				: item =>
				{
					try
					{
						IDisposable disposable = item as IDisposable;
						disposable?.Dispose();
					}
					catch (ObjectDisposedException)
					{
						// ignored
					}
				};
			}
			else
			{
				_onItemDispose = null;
			}
		}

		public int Capacity
		{
			get => _capacity;
			set
			{
				if (value < Count || value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
				_capacity = value;
				_version++;
			}
		}

		/// <inheritdoc cref="ICollection" />
		public int Count => _items.Count;

		/// <inheritdoc />
		public bool IsReadOnly { get; }

		/// <inheritdoc />
		bool ICollection.IsSynchronized => _collection.IsSynchronized;

		/// <inheritdoc />
		object ICollection.SyncRoot => _collection.SyncRoot;

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return new Enumerator(this); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public void Enqueue(T item)
		{
			if (_items.Count == Capacity)
			{
				T first = _items.First.Value;
				_items.RemoveFirst();
				_onItemDispose?.Invoke(first);
			}

			_items.AddLast(item);
			_version++;
		}

		/// <inheritdoc />
		void ICollection<T>.Add(T item) { Enqueue(item); }

		public void Insert(int index, T item)
		{
			_items.Count.ValidateRange(index);

			LinkedListNode<T> node = _items.First;

			for (int i = 1; i < index && node != null; i++)
			{
				node = node.Next;
			}

			if (node == null) throw new InvalidOperationException();
			_items.AddAfter(node, item);
		}

		public T Dequeue()
		{
			if (_items.Count == 0) throw new InvalidOperationException("Collection is empty.");
			T item = _items.First.Value;
			_items.RemoveFirst();
			return item;
		}

		public T Pop()
		{
			if (_items.Count == 0) throw new InvalidOperationException("Collection is empty.");
			T item = _items.Last.Value;
			_items.RemoveLast();
			return item;
		}

		/// <inheritdoc />
		bool ICollection<T>.Remove(T item)
		{
			if (!_items.Remove(item)) return false;
			_version++;
			return true;
		}

		public T Peek()
		{
			if (_items.Count == 0) throw new InvalidOperationException("Collection is empty.");
			return _items.First.Value;
		}

		public T PeekTail()
		{
			if (_items.Count == 0) throw new InvalidOperationException("Collection is empty.");
			return _items.Last.Value;
		}

		/// <inheritdoc cref="ICollection" />
		public void Clear()
		{
			if (_items.Count == 0) return;

			if (_onItemDispose != null)
			{
				while (_items.Count > 0)
				{
					T item = _items.First.Value;
					_items.RemoveFirst();
					_onItemDispose(item);
				}

				_version++;
				return;
			}

			_items.Clear();
			_version++;
		}

		/// <summary>
		/// Determines whether this list contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in this list.</param>
		/// <returns>
		/// true if <paramref name="item" /> is found in this list; otherwise, false.
		/// </returns>
		public bool Contains(T item)
		{
			return _items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_items.CopyTo(array, arrayIndex);
		}

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			_collection.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Creates and returns a new array containing the elements in this deque.
		/// </summary>
		[NotNull]
		public T[] ToArray()
		{
			T[] result = new T[Count];
			CopyTo(result, 0);
			return result;
		}
	}
}