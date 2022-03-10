using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions;
using essentialMix.Exceptions.Collections;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <summary>
/// A double-ended queue (deque), which provides O(n) access, O(1) removals from the front and back, amortized
/// O(1) insertions to the front and back, and O(N) insertions and removals anywhere else (with the operations getting
/// slower as the index approaches the middle).
/// </summary>
/// <typeparam name="T">The type of elements contained in the deque.</typeparam>
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
[Serializable]
public class LinkedDeque<T> : IDeque<T>, ICollection<T>, ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
{
	[NotNull]
	[NonSerialized]
	private ICollection _collection;

	public LinkedDeque()
	{
		Items = new LinkedList<T>();
		_collection = Items;
	}

	/// <inheritdoc cref="ICollection{T}" />
	public int Count => Items.Count;

	/// <inheritdoc />
	bool ICollection<T>.IsReadOnly => false;

	/// <inheritdoc />
	public bool IsSynchronized => _collection.IsSynchronized;

	/// <inheritdoc />
	object ICollection.SyncRoot => _collection.SyncRoot;

	[NotNull]
	protected LinkedList<T> Items { get; }

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator() { return Items.GetEnumerator(); }

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	/// <inheritdoc />
	public void Add(T value) { Items.AddLast(value); }

	/// <inheritdoc />
	public bool Remove(T value) { return Items.Remove(value); }

	public void Enqueue(T item) { Items.AddLast(item); }

	public void Enqueue([NotNull] IEnumerable<T> enumerable)
	{
		foreach (T item in enumerable)
			Items.AddLast(item);
	}

	public void EnqueueBefore(T key, T value)
	{
		LinkedListNode<T> node = Items.Find(key) ?? throw new NotFoundException();
		EnqueueBefore(node, value);
	}

	public void EnqueueBefore([NotNull] LinkedListNode<T> node, T value)
	{
		Items.AddBefore(node, value);
	}

	public void EnqueueAfter(T key, T value)
	{
		LinkedListNode<T> node = Items.Find(key) ?? throw new NotFoundException();
		EnqueueAfter(node, value);
	}

	public void EnqueueAfter([NotNull] LinkedListNode<T> node, T value)
	{
		Items.AddAfter(node, value);
	}

	public void EnqueueLast(T value) { Items.AddLast(value); }

	public T Dequeue()
	{
		if (Count == 0) throw new CollectionIsEmptyException();
		T value = Items.First.Value;
		Items.RemoveFirst();
		return value;
	}

	/// <inheritdoc />
	public bool TryDequeue(out T item)
	{
		if (Count == 0)
		{
			item = default(T);
			return false;
		}
		item = Items.First.Value;
		Items.RemoveFirst();
		return true;
	}

	public void Push(T value) { Items.AddLast(value); }

	public void Push([NotNull] IEnumerable<T> enumerable)
	{
		foreach (T item in enumerable.Reverse())
			Items.AddLast(item);
	}

	public void PushBefore(T key, T value)
	{
		LinkedListNode<T> node = Items.Find(key) ?? throw new NotFoundException();
		PushBefore(node, value);
	}

	public void PushBefore([NotNull] LinkedListNode<T> node, T value)
	{
		Items.AddAfter(node, value);
	}

	public void PushAfter(T key, T value)
	{
		LinkedListNode<T> node = Items.Find(key) ?? throw new NotFoundException();
		PushAfter(node, value);
	}

	public void PushAfter([NotNull] LinkedListNode<T> node, T value)
	{
		Items.AddBefore(node, value);
	}

	public void PushLast(T value) { Items.AddFirst(value); }

	public T Pop()
	{
		if (Count == 0) throw new CollectionIsEmptyException();
		T value = Items.Last.Value;
		Items.RemoveLast();
		return value;
	}

	/// <inheritdoc />
	public bool TryPop(out T item)
	{
		if (Count == 0)
		{
			item = default(T);
			return false;
		}
		item = Items.Last.Value;
		return true;
	}

	public T Peek()
	{
		if (Count == 0) throw new CollectionIsEmptyException();
		return Items.First.Value;
	}

	/// <inheritdoc />
	public bool TryPeek(out T item)
	{
		if (Count == 0)
		{
			item = default(T);
			return false;
		}
		item = Items.First.Value;
		return true;
	}

	public T PeekTail()
	{
		if (Count == 0) throw new CollectionIsEmptyException();
		return Items.Last.Value;
	}

	/// <inheritdoc />
	public bool TryPeekTail(out T item)
	{
		if (Count == 0)
		{
			item = default(T);
			return false;
		}
		item = Items.Last.Value;
		return true;
	}

	/// <inheritdoc cref="ICollection{T}" />
	public void Clear() { Items.Clear(); }

	/// <inheritdoc />
	public bool Contains(T value) { return Items.Contains(value); }

	public LinkedListNode<T> Find(T value)
	{
		return Items.Find(value);
	}

	public LinkedListNode<T> FindNext([NotNull] LinkedListNode<T> node)
	{
		if (node.List != Items) throw new InvalidOperationException("Node does not belong to the collection.");

		T value = node.Value;
		LinkedListNode<T> search = null, next = node.Next;

		if (value != null)
		{
			EqualityComparer<T> comparer = EqualityComparer<T>.Default;

			while (next != null)
			{
				if (comparer.Equals(value, next.Value))
				{
					search = next;
					break;
				}
				next = next.Next;
			}
		}
		else
		{
			while (next != null)
			{
				if (next.Value == null)
				{
					search = next;
					break;
				}
				next = next.Next;
			}
		}

		return search;
	}

	public LinkedListNode<T> FindLast(T value)
	{
		return Items.FindLast(value);
	}

	/// <inheritdoc />
	public void CopyTo(T[] array, int arrayIndex) { Items.CopyTo(array, arrayIndex); }

	/// <inheritdoc />
	public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }
}

public static class LinkedDeque
{
	[Serializable]
	private class SynchronizedCollection<T> : ICollection<T>
	{
		private readonly LinkedDeque<T> _deque;
		private readonly ICollection _collection;
		private readonly object _root;

		internal SynchronizedCollection(LinkedDeque<T> deque)
		{
			_deque = deque;
			_collection = deque;
			_root = _collection.SyncRoot;
		}

		/// <inheritdoc />
		public int Count
		{
			get
			{
				lock (_root)
				{
					return _deque.Count;
				}
			}
		}

		/// <inheritdoc />
		public bool IsReadOnly
		{
			get
			{
				lock (_root)
				{
					return ((ICollection<T>)_collection).IsReadOnly;
				}
			}
		}

		/// <inheritdoc />
		public void Add(T item)
		{
			lock (_root)
			{
				_deque.Add(item);
			}
		}

		/// <inheritdoc />
		public bool Remove(T item)
		{
			lock (_root)
			{
				return _deque.Remove(item);
			}
		}

		/// <inheritdoc />
		public void Clear()
		{
			lock (_root)
			{
				_deque.Clear();
			}
		}

		/// <inheritdoc />
		public bool Contains(T item)
		{
			lock (_root)
			{
				return _deque.Contains(item);
			}
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex)
		{
			lock (_root)
			{
				_deque.CopyTo(array, arrayIndex);
			}
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			lock (_root)
			{
				return _deque.GetEnumerator();
			}
		}

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator()
		{
			lock (_root)
			{
				return _deque.GetEnumerator();
			}
		}
	}

	[NotNull]
	public static ICollection<T> Synchronized<T>([NotNull] LinkedDeque<T> list) { return new SynchronizedCollection<T>(list); }
}