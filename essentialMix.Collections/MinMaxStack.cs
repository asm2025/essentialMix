using System;
using System.Collections;
using System.Collections.Generic;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Collections;

public sealed class MinMaxStack<T> : IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
	where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
{
	private readonly Stack<T> _stack;
	private readonly T _minimum;
	private readonly T _maximum;
	private readonly Stack<T> _minStack;
	private readonly Stack<T> _maxStack;

	[NonSerialized]
	private readonly ICollection _collectionRef;

	public MinMaxStack()
		: this(0, null)
	{
	}

	public MinMaxStack(int capacity)
		: this(capacity, null)
	{
	}

	public MinMaxStack([NotNull] IEnumerable<T> collection)
		: this(0, collection)
	{
	}

	private MinMaxStack(int capacity, IEnumerable<T> collection)
	{
		_stack = collection == null
					? new Stack<T>(capacity)
					: new Stack<T>(collection);
		_collectionRef = _stack;
		_minimum = TypeHelper.MinimumOf<T>();
		_maximum = TypeHelper.MaximumOf<T>();
		_minStack = new Stack<T>();
		_maxStack = new Stack<T>();
	}

	public int Count => _stack.Count;

	public object SyncRoot => _collectionRef.SyncRoot;

	public bool IsSynchronized => _collectionRef.IsSynchronized;

	public T Minimum =>
		_minStack.Count == 0
			? _maximum
			: _minStack.Peek();

	public T Maximum =>
		_maxStack.Count == 0
			? _minimum
			: _maxStack.Peek();

	public IEnumerator<T> GetEnumerator() { return _stack.GetEnumerator(); }

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	public void Push(T item)
	{
		if (item.IsLessThanOrEqual(Minimum)) _minStack.Push(item);
		if (item.IsGreaterThanOrEqual(Maximum)) _maxStack.Push(item);
		_stack.Push(item);
	}

	public T Pop()
	{
		T item = _stack.Pop();
		if (item.IsEqual(Minimum)) _minStack.Pop();
		if (item.IsEqual(Maximum)) _maxStack.Pop();
		return item;
	}

	public T Peek() { return _stack.Peek(); }

	public void Clear() { _stack.Clear(); }
		
	public bool Contains(T item) { return _stack.Contains(item); }

	public void CopyTo([NotNull] T[] array, int index) { _stack.CopyTo(array, index); }

	void ICollection.CopyTo(Array array, int index)
	{
		_collectionRef.CopyTo(array, index);
	}

	[NotNull]
	public T[] ToArray() { return _stack.ToArray(); }

	public void TrimExcess() { _stack.TrimExcess(); }
}