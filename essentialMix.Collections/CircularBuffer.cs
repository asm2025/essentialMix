﻿using System;
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

namespace essentialMix.Collections;

[ComVisible(false)]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
[Serializable]
public class CircularBuffer<T> : ICircularBuffer<T>, ICollection, IReadOnlyCollection<T>, IEnumerable
{
	[Serializable]
	private struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly CircularBuffer<T> _buffer;
		private readonly int _version;

		private readonly int _head;
		private readonly int _capacity;
		private readonly int _count;
		private int _index;
		private int _position;
		private T _current;

		internal Enumerator([NotNull] CircularBuffer<T> buffer)
		{
			_buffer = buffer;
			_index = _position = -1;
			_head = buffer._head;
			_capacity = buffer.Capacity;
			_count = buffer.Count;
			_current = default(T);
			_version = buffer._version;
		}

		/// <inheritdoc />
		public T Current
		{
			get
			{
				if (_index < 0 || _index >= _count) throw new InvalidOperationException();
				return _current;
			}
			private set => _current = value;
		}

		/// <inheritdoc />
		object IEnumerator.Current => Current;

		/// <inheritdoc />
		public void Dispose() { }

		/// <inheritdoc />
		public bool MoveNext()
		{
			if (_version == _buffer._version && _index < _count - 1)
			{
				_position = _position < 0
								? _position = _head
								: _position = (_position + 1) % _capacity;
				Current = _buffer._items[_position];
				_index++;
				return true;
			}
			return MoveNextRare();
		}

		private bool MoveNextRare()
		{
			if (_version != _buffer._version) throw new InvalidOperationException();
			_index = _count + 1;
			_position = -1;
			_current = default(T);
			return false;
		}

		/// <inheritdoc />
		void IEnumerator.Reset()
		{
			if (_version != _buffer._version) throw new InvalidOperationException();
			_index = _position = -1;
			Current = default(T);
		}
	}

	private readonly bool _isDisposable;
	private readonly bool _isObject;

	private T[] _items;
	private int _head;
	private int _tail;
	private int _version;

	[NonSerialized]
	private object _syncRoot;

	/// <summary>
	/// Initializes a new instance of the <see cref="CircularBuffer{T}" /> class with the specified capacity where capacity cannot be less than 1.
	/// </summary>
	/// <param name="capacity">The initial capacity. Must be greater than <c>0</c>.</param>
	public CircularBuffer(int capacity)
	{
		if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
		_head = _tail = -1;
		_items = new T[capacity];
		Type type = typeof(T);
		_isDisposable = typeof(IDisposable).IsAssignableFrom(type);
		_isObject = type == typeof(object);
	}

	public int Capacity
	{
		get => _items.Length;
		set
		{
			if (value < Count || value < 1) throw new ArgumentOutOfRangeException(nameof(value));
			if (value == _items.Length) return;
			T[] newItems = new T[value];
			CopyTo(newItems, 0);
			_items = newItems;
			_tail = Count - 1;
			_head = Count == 0
						? -1
						: 0;
			_version++;
		}
	}

	/// <inheritdoc cref="ICollection" />
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
		if (_head < 0) _head = 0;
		_tail = (_tail + 1) % Capacity;

		if (Count == Capacity && _head == _tail)
		{
			if ((_isDisposable || _isObject) && _items[_head] is IDisposable disposable)
			{
				try
				{
					disposable.Dispose();
				}
				catch (ObjectDisposedException)
				{
					// ignored
				}
			}

			_head = (_head + 1) % Capacity;
		}

		_items[_tail] = item;
		if (Count < Capacity) Count++;
		_version++;
	}

	public T Dequeue()
	{
		if (Count == 0) throw new CollectionIsEmptyException();
		T item = _items[_head];
		Count--;
		if (Count == 0) _tail = _head = -1;
		else _head = (_head + 1) % Capacity;
		return item;
	}

	public T Pop()
	{
		if (Count == 0) throw new CollectionIsEmptyException();
		T item = _items[_tail];
		Count--;
		if (Count == 0) _tail = _head = -1;
		else _tail = (_tail - 1) % Capacity;
		return item;
	}

	public T Peek()
	{
		if (Count == 0) throw new CollectionIsEmptyException();
		return _items[_head];
	}

	public T PeekTail()
	{
		if (Count == 0) throw new CollectionIsEmptyException();
		return _items[_tail];
	}

	/// <inheritdoc cref="ICollection" />
	public void Clear()
	{
		if (Count == 0) return;
			
		if (_isDisposable || _isObject)
		{
			while (Count > 0)
			{
				if (_items[_head] is IDisposable disposable)
				{
					try
					{
						disposable.Dispose();
					}
					catch (ObjectDisposedException)
					{
						// ignored
					}
				}

				Count--;
				_head = (_head + 1) % Capacity;
			}
		}

		Count = 0;
		_head = _tail = -1;
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
		if (_tail >= _head) return Array.IndexOf(_items, item, _head, Count) > -1;
		return Array.IndexOf(_items, item, _head, _items.Length - _head) > -1
				|| Array.IndexOf(_items, item, 0, _tail) > -1;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (Count == 0) return;
		array.Length.ValidateRange(arrayIndex, Count);

		if (_tail < _head)
		{
			// The existing buffer is split, so we have to copy it in parts
			int length = _items.Length - _head;
			Array.Copy(_items, _head, array, arrayIndex, length);
			Array.Copy(_items, 0, array, arrayIndex + length, Count - length);
		}
		else
		{
			// The existing buffer is whole
			Array.Copy(_items, _head, array, arrayIndex, Count);
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

		if (_tail < _head)
		{
			// The existing buffer is split, so we have to copy it in parts
			int length = _items.Length - _head;
			Array.Copy(_items, _head, array, arrayIndex, length);
			Array.Copy(_items, 0, array, arrayIndex + length, Count - length);
		}
		else
		{
			// The existing buffer is whole
			Array.Copy(_items, _head, array, arrayIndex, Count);
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