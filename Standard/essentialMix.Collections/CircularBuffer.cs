using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Threading;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[ComVisible(false)]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
	[Serializable]
	public class CircularBuffer<T> : ICollection, IReadOnlyCollection<T>, IEnumerable
	{
		[Serializable]
		private struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
		{
			private readonly CircularBuffer<T> _circularBuffer;
			private readonly int _version;

			private readonly int _read;
			private readonly int _capacity;
			private readonly int _count;
			private int _index;
			private int _position;

			internal Enumerator([NotNull] CircularBuffer<T> circularBuffer)
			{
				_circularBuffer = circularBuffer;
				_index = _position = -1;
				_read = circularBuffer._read;
				_capacity = circularBuffer._items.Length;
				_count = circularBuffer.Count;
				_version = circularBuffer._version;
				Current = default(T);
			}

			/// <inheritdoc />
			public void Dispose() { }

			/// <inheritdoc />
			public bool MoveNext()
			{
				if (_version == _circularBuffer._version && _index < _count - 1)
				{
					_position = _position < 0
									? _position = _read
									: _position = (_position + 1) % _capacity;
					Current = _circularBuffer._items[_position];
					_index++;
					return true;
				}
				return MoveNextRare();
			}

			private bool MoveNextRare()
			{
				if (_version != _circularBuffer._version) throw new VersionChangedException();
				_index = _count + 1;
				_position = -1;
				Current = default(T);
				return false;
			}

			/// <inheritdoc />
			public T Current { get; private set; }

			/// <inheritdoc />
			object IEnumerator.Current
			{
				get
				{
					if (!_index.InRangeRx(0, _count)) throw new InvalidOperationException();
					return Current;
				}
			}

			/// <inheritdoc />
			void IEnumerator.Reset()
			{
				if (_version != _circularBuffer._version) throw new VersionChangedException();
				_index = _position = -1;
				Current = default(T);
			}
		}

		private int _read;
		private int _write;
		private int _version;
		private T[] _items;

		[NonSerialized]
		private object _syncRoot;

		/// <summary>
		/// Initializes a new instance of the <see cref="CircularBuffer{T}" /> class with the specified capacity where capacity cannot be less than 1.
		/// </summary>
		/// <param name="capacity">The initial capacity. Must be greater than <c>0</c>.</param>
		public CircularBuffer(int capacity)
		{
			if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			_read = _write = -1;
			_items = new T[capacity];
		}

		public int Capacity
		{
			get => _items.Length;
			set
			{
				if (value < Count || value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
				if (value == _items.Length) return;
				T[] newItems = new T[value];
				CopyTo(newItems, 0);
				_items = newItems;
				_write = Count - 1;
				_read = Count == 0
							? -1
							: 0;
				_version++;
			}
		}

		/// <inheritdoc cref="ICollection{T}" />
		[field: ContractPublicPropertyName("Count")]
		public int Count { get; private set; }

		/// <inheritdoc />
		bool ICollection.IsSynchronized => false;

		/// <inheritdoc />
		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return new Enumerator(this); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public void Enqueue(T item)
		{
			_write = (_write + 1) % _items.Length;
			if (_read < 0) _read = 0;
			_items[_write] = item;
			if (Count < _items.Length) Count++;
			else _read = _write;
			_version++;
		}

		public T Dequeue()
		{
			if (Count == 0) throw new InvalidOperationException("Collection is empty.");
			T item = _items[_read];
			Count--;
			_read = (_read + 1) % _items.Length;
			return item;
		}

		public T Peek()
		{
			if (Count == 0) throw new InvalidOperationException("Collection is empty.");
			return _items[_read];
		}

		/// <inheritdoc cref="ICollection{T}" />
		public void Clear()
		{
			Count = 0;
			_read = _write = -1;
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
			if (Count == 0) return false;
			if (_write >= _read) return Array.IndexOf(_items, item, _read, Count) > -1;
			return Array.IndexOf(_items, item, _read, _items.Length - _read) > -1 
					|| Array.IndexOf(_items, item, 0, _write) > -1;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);

			if (_write < _read)
			{
				// The existing buffer is split, so we have to copy it in parts
				int length = _items.Length - _read;
				Array.Copy(_items, _read, array, arrayIndex, length);
				Array.Copy(_items, 0, array, arrayIndex + length, Count - length);
			}
			else
			{
				// The existing buffer is whole
				Array.Copy(_items, _read, array, arrayIndex, Count);
			}
		}

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));
			if (Count == 0) return;

			if (array is T[] tArray)
			{
				CopyTo(tArray, arrayIndex);
				return;
			}

			/*
			 * Catch the obvious case assignment will fail.
			 * We can find all possible problems by doing the check though.
			 * For example, if the element type of the Array is derived from T,
			 * we can't figure out if we can successfully copy the element beforehand.
			 */
			array.Length.ValidateRange(arrayIndex, Count);

			Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
			Type sourceType = typeof(T);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (array is not object[]) throw new ArgumentException("Invalid array type", nameof(array));

			if (_write < _read)
			{
				// The existing buffer is split, so we have to copy it in parts
				int length = _items.Length - _read;
				Array.Copy(_items, _read, array, arrayIndex, length);
				Array.Copy(_items, 0, array, arrayIndex + length, Count - length);
			}
			else
			{
				// The existing buffer is whole
				Array.Copy(_items, _read, array, arrayIndex, Count);
			}
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