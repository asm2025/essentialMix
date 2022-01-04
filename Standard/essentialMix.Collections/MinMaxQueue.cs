using System;
using System.Collections;
using System.Collections.Generic;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[Serializable]
public sealed class MinMaxQueue<T> : IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
	where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
{
	private readonly Queue<T> _queue;
	private readonly T _minimum;
	private readonly T _maximum;
	private readonly Stack<T> _minStack;
	private readonly Stack<T> _maxStack;

	[NonSerialized]
	private readonly ICollection _collectionRef;

	public MinMaxQueue()
		: this(0, null)
	{
	}

	public MinMaxQueue(int capacity)
		: this(capacity, null)
	{
	}

	public MinMaxQueue([NotNull] IEnumerable<T> enumerable)
		: this(0, enumerable)
	{
	}

	private MinMaxQueue(int capacity, IEnumerable<T> collection)
	{
		_queue = collection == null
					? new Queue<T>(capacity)
					: new Queue<T>(collection);
		_collectionRef = _queue;
		_minimum = TypeHelper.MinimumOf<T>();
		_maximum = TypeHelper.MaximumOf<T>();
		_minStack = new Stack<T>();
		_maxStack = new Stack<T>();
	}

	public int Count => _queue.Count;

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

	public IEnumerator<T> GetEnumerator() { return _queue.GetEnumerator(); }

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	public void Enqueue(T item)
	{
		if (item.IsLessThanOrEqual(Minimum)) _minStack.Push(item);
		if (item.IsGreaterThanOrEqual(Maximum)) _maxStack.Push(item);
		_queue.Enqueue(item);
	}

	public T Dequeue()
	{
		T item = _queue.Dequeue();
		if (item.IsEqual(Minimum)) _minStack.Pop();
		if (item.IsEqual(Maximum)) _maxStack.Pop();
		return item;
	}

	public T Peek() { return _queue.Peek(); }

	public void Clear() { _queue.Clear(); }
		
	public bool Contains(T item) { return _queue.Contains(item); }

	public void CopyTo([NotNull] T[] array, int index) { _queue.CopyTo(array, index); }

	void ICollection.CopyTo(Array array, int index)
	{
		_collectionRef.CopyTo(array, index);
	}

	[NotNull]
	public T[] ToArray() { return _queue.ToArray(); }

	public void TrimExcess() { _queue.TrimExcess(); }
}