using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections;

// based on https://github.com/microsoft/referencesource/blob/master/System/compmod/system/collections/generic/linkedlist.cs
[ComVisible(false)]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
[Serializable]
public class LinkedCircularBuffer<T> : ICircularBuffer<T>, ICollection<T>, ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, ISerializable, IDeserializationCallback
{
	[ComVisible(false)]
	[DebuggerDisplay("{Value}")]
	[Serializable]
	private class Node(LinkedCircularBuffer<T> buffer, T value)
	{
		internal LinkedCircularBuffer<T> _buffer = buffer;
		internal Node _previous;
		internal Node _next;

		public Node(T value)
			: this(null, value)
		{
		}

		public T Value { get; set; } = value;

		public Node Previous
		{
			get => _previous == null || _previous == _buffer._head
						? null
						: _previous;
			set => _previous = value;
		}

		public Node Next
		{
			get => _next == null || _next == _buffer._head
						? null
						: _next;
			set => _next = value;
		}

		/// <inheritdoc />
		public override string ToString() { return Convert.ToString(Value); }

		internal void Invalidate()
		{
			_buffer = null;
			_previous = null;
			_next = null;
		}

		public static implicit operator T([NotNull] Node node) { return node.Value; }
	}

	[Serializable]
	private struct Enumerator : IEnumerator<T>, IEnumerator, ISerializable, IDeserializationCallback, IDisposable
	{
		private LinkedCircularBuffer<T> _buffer;
		private Node _node;
		private T _current;
		private int _index;
		private int _version;
		private SerializationInfo siInfo;

		internal Enumerator([NotNull] LinkedCircularBuffer<T> buffer)
		{
			_buffer = buffer;
			_node = _buffer._head;
			_current = default(T);
			_index = -1;
			_version = buffer._version;
			siInfo = null;
		}

		internal Enumerator(SerializationInfo info, StreamingContext context)
		{
			siInfo = info;
			_buffer = null;
			_node = null;
			_current = default(T);
			_index = -1;
			_version = 0;
		}

		/// <inheritdoc />
		public T Current
		{
			get
			{
				if (!_index.InRangeRx(0, _buffer.Count)) throw new InvalidOperationException();
				return _current;
			}
		}

		/// <inheritdoc />
		object IEnumerator.Current => Current;

		/// <inheritdoc />
		public void Dispose() { }

		/// <inheritdoc />
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null) throw new ArgumentNullException(nameof(info));
			info.AddValue(nameof(LinkedCircularBuffer<T>), _buffer);
			info.AddValue(nameof(_version), _version);
			info.AddValue(nameof(Current), _current);
			info.AddValue(nameof(_index), _index);
		}

		/// <inheritdoc />
		void IDeserializationCallback.OnDeserialization(object sender)
		{
			if (_buffer != null) return;
			if (siInfo == null) throw new SerializationException();
			_buffer = (LinkedCircularBuffer<T>)siInfo.GetValue(nameof(LinkedCircularBuffer<T>), typeof(LinkedCircularBuffer<T>));
			_version = siInfo.GetInt32(nameof(_version));
			_current = (T)siInfo.GetValue(nameof(Current), typeof(T));
			_index = siInfo.GetInt32(nameof(_index));
			if (_buffer.siInfo != null) ((IDeserializationCallback)_buffer).OnDeserialization(sender);

			if (_index >= _buffer.Count)
			{
				_node = null;
			}
			else
			{
				_node = _buffer._head;

				if (_node != null && _index > 0)
				{
					for (int i = 0; i < _index && _node != null; i++)
					{
						_node = _node._next;
					}

					if (_node == _buffer._head) _node = null;
				}
			}

			siInfo = null;
		}

		/// <inheritdoc />
		public bool MoveNext()
		{
			if (_version != _buffer._version) throw new InvalidOperationException();

			if (_node == null)
			{
				_index = _buffer.Count + 1;
				return false;
			}

			_index++;
			_current = _node.Value;
			_node = _node._next;
			if (_node == _buffer._head) _node = null;
			return true;
		}

		/// <inheritdoc />
		void IEnumerator.Reset()
		{
			if (_version != _buffer._version) throw new InvalidOperationException();
			_node = _buffer._head;
			_current = default(T);
			_index = -1;
		}
	}

	private const string ITEMS_NAME = "item[]";

	private readonly bool _isDisposable;
	private readonly bool _isObject;

	private Node _head;
	private int _capacity;
	private int _version;

	[NonSerialized]
	private object _syncRoot;

	private SerializationInfo siInfo;

	/// <summary>
	/// Initializes a new instance of the <see cref="LinkedCircularBuffer{T}" /> class with the specified capacity where capacity cannot be less than 1.
	/// </summary>
	/// <param name="capacity">The initial capacity. Must be greater than <c>0</c>.</param>
	public LinkedCircularBuffer(int capacity)
	{
		if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
		_capacity = capacity;
		Type type = typeof(T);
		_isDisposable = typeof(IDisposable).IsAssignableFrom(type);
		_isObject = type == typeof(object);
	}

	public int Capacity
	{
		get => _capacity;
		set
		{
			if (value < Count || value < 1) throw new ArgumentOutOfRangeException(nameof(value));
			_capacity = value;
			_version++;
		}
	}

	/// <inheritdoc cref="ICollection" />
	[field: ContractPublicPropertyName("Count")]
	public int Count { get; private set; }

	/// <inheritdoc />
	public bool IsReadOnly => false;

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

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		/*
		* Customized serialization for LinkedList.
		* We need to do this because it will be too expensive to serialize each node.
		* This will give us the flexibility to change internal implementation freely in future.
		*/
		if (info == null) throw new ArgumentNullException(nameof(info));
		info.AddValue(nameof(_version), _version);
		info.AddValue(nameof(Capacity), Capacity);
		info.AddValue(nameof(Count), Count);
		if (Count == 0) return;
		T[] array = ToArray();
		info.AddValue(ITEMS_NAME, array);
	}

	void IDeserializationCallback.OnDeserialization(object sender)
	{
		if (siInfo == null) return;
		Capacity = siInfo.GetInt32(nameof(Capacity));

		int version = siInfo.GetInt32(nameof(_version));
		int count = siInfo.GetInt32(nameof(Count));

		if (count > 0)
		{
			T[] array = (T[])siInfo.GetValue(ITEMS_NAME, typeof(T[]));
			if (array == null) throw new SerializationDataMissingException();

			foreach (T value in array)
			{
				AddNode(value);
			}
		}
		else
		{
			_head = null;
		}

		_version = version;
		siInfo = null;
	}

	public void Enqueue(T item)
	{
		if (Count == Capacity)
		{
			T tmp = _head.Value;
			RemoveNode(_head);

			if ((_isDisposable || _isObject) && tmp is IDisposable disposable)
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
		}

		AddNode(item);
		_version++;
	}

	/// <inheritdoc />
	void ICollection<T>.Add(T item) { Enqueue(item); }

	public void Insert(int index, T item)
	{
		Count.ValidateRange(index);

		Node node = _head;

		for (int i = 1; i < index && node != null; i++) 
			node = node.Next; // use Next the instead of "_next"

		if (node == null) throw new InvalidOperationException();
		AddNode(item, node);
	}

	public T Dequeue()
	{
		if (Count == 0) throw new CollectionIsEmptyException();
		T item = _head.Value;
		RemoveNode(_head);
		return item;
	}

	public T Pop()
	{
		if (Count == 0) throw new CollectionIsEmptyException();
		T item = Last().Value;
		RemoveNode(_head._previous);
		return item;
	}

	/// <inheritdoc />
	bool ICollection<T>.Remove(T item)
	{
		Node node = Find(item);
		if (node == null) return false;
		RemoveNode(node);
		return true;
	}

	public T Peek()
	{
		if (Count == 0) throw new CollectionIsEmptyException();
		return _head.Value;
	}

	public T PeekTail()
	{
		if (Count == 0) throw new CollectionIsEmptyException();
		return Last().Value;
	}

	/// <inheritdoc cref="ICollection" />
	public void Clear()
	{
		Node current = _head;

		if (_isDisposable || _isObject)
		{
			while (current != null)
			{
				Node tmp = current;
				current = current.Next; // use Next the instead of "_next", otherwise it will loop forever

				if (tmp.Value is IDisposable disposable)
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

				tmp.Invalidate();
			}
		}
		else
		{
			while (current != null)
			{
				Node tmp = current;
				current = current.Next; // use Next the instead of "_next", otherwise it will loop forever
				tmp.Invalidate();
			}
		}

		_head = null;
		Count = 0;
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
		return Find(item) != null;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (Count == 0) return;
		array.Length.ValidateRange(arrayIndex, Count);

		Node node = _head;
		if (node == null) return;

		do
		{
			array[arrayIndex++] = node.Value;
			node = node._next;
		}
		while (node != _head);
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

		//
		// Catch the obvious case assignment will fail.
		// We can found all possible problems by doing the check though.
		// For example, if the element type of the Array is derived from T,
		// we can't figure out if we can successfully copy the element beforehand.
		//
		array.Length.ValidateRange(arrayIndex, Count);

		Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
		Type sourceType = typeof(T);
		if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
		if (array is not object[] objects) throw new ArgumentException("Invalid array type", nameof(array));

		Node node = _head;
			
		try
		{
			if (node == null) return;
				
			do
			{
				objects[arrayIndex++] = node.Value;
				node = node._next;
			} while (node != _head);
		}
		catch (ArrayTypeMismatchException)
		{
			throw new ArgumentException("Invalid array type", nameof(array));
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

	private Node Last()
	{
		return _head?._previous;
	}

	private void AddNode(T value, Node node = null)
	{
		Node newNode = new Node(this, value);

		if (_head == null)
		{
			Debug.Assert(node == null && Count == 0, "LinkedCircularBuffer must be empty and no parent should be passed!");
			newNode._next = newNode;
			newNode._previous = newNode;
			_head = newNode;
		}
		else
		{
			node ??= _head;
			newNode._next = node;
			newNode._previous = node._previous;
			node._previous._next = newNode;
			node._previous = newNode;
		}

		Count++;
		_version++;
	}

	private void RemoveNode([NotNull] Node node)
	{
		Debug.Assert(node._buffer == this, "Deleting the node from another container!");
		Debug.Assert( _head != null, "This method shouldn't be called on empty list!");

		if (node._next == node)
		{
			Debug.Assert(Count == 1 && _head == node, "this should only be true for a list with only one node");
			_head = null;
		}
		else
		{
			node._next._previous = node._previous;
			node._previous._next = node._next;
			if (_head == node) _head = node._next;
		}

		node.Invalidate();
		Count--;
		_version++;
	}

	private Node Find(T value)
	{
		Node node = _head;
		if (node == null) return null;

		if (value is not null)
		{
			IEqualityComparer<T> c = EqualityComparer<T>.Default;

			do
			{
				if (c.Equals(node.Value, value)) return node;
				node = node._next;
			}
			while (node != _head);
		}
		else
		{
			do
			{
				if (node.Value is null) return node;
				node = node._next;
			}
			while (node != _head);
		}

		return null;
	}
}