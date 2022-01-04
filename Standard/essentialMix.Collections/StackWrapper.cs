using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections;

public class StackWrapper<T> : IStack<T>, ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
{
	private readonly Stack<T> _stack;
	private readonly ICollection _collection;

	public StackWrapper()
		: this(new Queue<T>())
	{
	}

	public StackWrapper(IEnumerable<T> collection)
		: this(new Stack<T>(collection))
	{
	}

	public StackWrapper(int capacity)
		: this(new Stack<T>(capacity))
	{
	}

	public StackWrapper([NotNull] Stack<T> stack)
	{
		_stack = stack;
		_collection = stack;
	}

	/// <inheritdoc cref="ICollection" />
	public int Count => _stack.Count;

	/// <inheritdoc />
	bool ICollection.IsSynchronized => _collection.IsSynchronized;

	/// <inheritdoc />
	object ICollection.SyncRoot => _collection.SyncRoot;

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator() { return _stack.GetEnumerator(); }

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	/// <inheritdoc />
	public void Push(T item) { _stack.Push(item); }

	/// <inheritdoc />
	public T Pop() { return _stack.Pop(); }

	/// <inheritdoc />
	public bool TryPop(out T item)
	{
		if (_stack.Count == 0)
		{
			item = default(T);
			return false;
		}

		item = _stack.Pop();
		return true;
	}

	/// <inheritdoc />
	public T Peek() { return _stack.Peek(); }

	/// <inheritdoc />
	public bool TryPeek(out T item)
	{
		if (Count == 0)
		{
			item = default(T);
			return false;
		}
		item = _stack.Peek();
		return true;
	}

	/// <inheritdoc />
	public void Clear() { _stack.Clear(); }

	public bool Contains(T item) { return _stack.Contains(item); }

	public void CopyTo(T[] array, int index) { _collection.CopyTo(array, index); }

	/// <inheritdoc />
	public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }

	[NotNull]
	public T[] ToArray() { return _stack.ToArray(); }
	public void TrimExcess() { _stack.TrimExcess(); }
}