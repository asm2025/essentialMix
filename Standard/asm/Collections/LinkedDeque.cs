using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using asm.Other.Microsoft.Collections;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// A double-ended queue (deque), which provides O(n) access, O(1) removals from the front and back, amortized
	/// O(1) insertions to the front and back, and O(N) insertions and removals anywhere else (with the operations getting
	/// slower as the index approaches the middle).
	/// </summary>
	/// <typeparam name="T">The type of elements contained in the deque.</typeparam>
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(asm_Mscorlib_CollectionDebugView<>))]
	[Serializable]
	public class LinkedDeque<T> : ICollection<T>, ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
	{
		[Serializable]
		internal class SynchronizedCollection : ICollection<T>
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
		public void Add(T item) { Items.AddLast(item); }

		/// <inheritdoc />
		public bool Remove(T item) { return Items.Remove(item); }

		public void Enqueue(T item) { Items.AddLast(item); }

		public void Enqueue([NotNull] IEnumerable<T> enumerable)
		{
			foreach (T item in enumerable) 
				Items.AddLast(item);
		}

		public T Dequeue()
		{
			if (Count == 0) throw new InvalidOperationException("Collection is empty.");
			T value = Items.First.Value;
			Items.RemoveFirst();
			return value;
		}

		public void Push(T item) { Items.AddLast(item); }

		public void Push([NotNull] IEnumerable<T> enumerable)
		{
			foreach (T item in enumerable.Reverse()) 
				Items.AddLast(item);
		}

		public T Pop()
		{
			if (Count == 0) throw new InvalidOperationException("Collection is empty.");
			T value = Items.Last.Value;
			Items.RemoveLast();
			return value;
		}

		public T PeekQueue()
		{
			if (Count == 0) throw new InvalidOperationException("Collection is empty.");
			return Items.First.Value;
		}

		public T PeekStack()
		{
			if (Count == 0) throw new InvalidOperationException("Collection is empty.");
			return Items.Last.Value;
		}

		/// <inheritdoc />
		public void Clear() { Items.Clear(); }

		/// <inheritdoc />
		public bool Contains(T item) { return Items.Contains(item); }

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex) { Items.CopyTo(array, arrayIndex); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { _collection.CopyTo(array, index); }

		[NotNull]
		public static ICollection<T> Synchronized(LinkedDeque<T> list)
		{
			return new SynchronizedCollection(list);
		}
	}
}