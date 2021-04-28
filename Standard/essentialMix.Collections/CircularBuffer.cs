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
	public class CircularBuffer<T> : ICollection<T>, IReadOnlyCollection<T>, ICollection, IEnumerable
	{
		[Serializable]
		internal class SynchronizedCollection : ICollection<T>, IReadOnlyCollection<T>, IEnumerable
		{
			private readonly CircularBuffer<T> _circularBuffer;
			private readonly ICollection<T> _collection;
			private readonly object _root;

			internal SynchronizedCollection(CircularBuffer<T> circularBuffer)
			{
				_circularBuffer = circularBuffer;
				_collection = circularBuffer;
				_root = ((ICollection)circularBuffer).SyncRoot;
			}

			public int Count
			{
				get
				{
					lock (_root)
					{
						return _circularBuffer.Count;
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
						return _collection.IsReadOnly;
					}
				}
			}

			/// <inheritdoc />
			public void Add(T item)
			{
				lock (_root)
				{
					_circularBuffer.Add(item);
				}
			}

			/// <inheritdoc />
			public bool Remove(T item)
			{
				lock (_root)
				{
					return _circularBuffer.Remove(item);
				}
			}

			/// <inheritdoc />
			public void Clear()
			{
				lock (_root)
				{
					_circularBuffer.Clear();
				}
			}

			/// <inheritdoc />
			public bool Contains(T item)
			{
				lock (_root)
				{
					return _circularBuffer.Contains(item);
				}
			}

			/// <inheritdoc />
			public void CopyTo(T[] array, int arrayIndex)
			{
				lock (_root)
				{
					_circularBuffer.CopyTo(array, arrayIndex);
				}
			}

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator()
			{
				lock (_root)
				{
					return _circularBuffer.GetEnumerator();
				}
			}

			/// <inheritdoc />
			public IEnumerator<T> GetEnumerator()
			{
				lock (_root)
				{
					return _circularBuffer.GetEnumerator();
				}
			}
		}

		[Serializable]
		public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
		{
			private readonly CircularBuffer<T> _circularBuffer;
			private readonly int _version;

			private readonly int _head;
			private readonly int _tail;
			private readonly int _length;
			private readonly int _count;
			private int _index;
			private int _position;

			internal Enumerator([NotNull] CircularBuffer<T> circularBuffer)
			{
				_circularBuffer = circularBuffer;
				_index = 0;
				_position = -1;
				_head = circularBuffer._head;
				_tail = circularBuffer._tail;
				_length = circularBuffer.Items.Length;
				_count = circularBuffer.Count;
				_version = circularBuffer._version;
				Current = default(T);
			}

			/// <inheritdoc />
			public void Dispose() { }

			/// <inheritdoc />
			public bool MoveNext()
			{
				if (_version == _circularBuffer._version && _index < _count)
				{
					if (_position < 0)
					{
						_position = _head;
					}
					else if (_tail < _head)
					{
						_position = (_position + 1) % _length;
						if (_position < _head && _position > _tail) return MoveNextRare();
					}
					else
					{
						_position++;
						if (_position > _tail) return MoveNextRare();
					}

					Current = _circularBuffer.Items[_position];
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
				_index = 0;
				_position = -1;
				Current = default(T);
			}
		}

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
			_tail = -1;
			Items = new T[capacity];
		}

		public int Capacity
		{
			get => Items.Length;
			set
			{
				if (value < Count || value < 1) throw new ArgumentOutOfRangeException(nameof(value));
				if (value == Items.Length) return;
				T[] newItems = new T[value];
				CopyTo(newItems, 0);
				Items = newItems;
				_head = 0;
				_tail = Count - 1;
				_version++;
			}
		}

		/// <inheritdoc cref="ICollection{T}" />
		[field: ContractPublicPropertyName("Count")]
		public int Count { get; private set; }

		/// <inheritdoc />
		bool ICollection<T>.IsReadOnly => false;

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

		/// <summary>
		/// The circular Items that holds the view.
		/// </summary>
		[NotNull]
		protected T[] Items { get; private set; }

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return new Enumerator(this); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void Add(T item)
		{
			_tail = (_tail + 1) % Items.Length;
			Items[_tail] = item;
			if (Count == Items.Length && _tail == _head) _head = (_head + 1) % Items.Length;
			Count = (Count + 1).NotAbove(Items.Length);
			_version++;
		}

		public void Insert(T item)
		{
			if (_tail > -1)
			{
				if (_tail >= _head)
				{
					if (_tail < Items.Length - 1)
					{
						_tail++;

						for (int i = _tail; i > _head; i--) 
							Items[i] = Items[i - 1];
					}
					else
					{
						T last = Items[_tail];

						for (int i = _tail; i > _head; i--) 
							Items[i] = Items[i - 1];

						_tail = 0;
						if (_tail == _head) _head = (_head + 1) % Items.Length;
						Items[_tail] = last;
					}
				}
				else
				{
					T last = Items[Items.Length - 1];

					if (_head < Items.Length - 1)
					{
						for (int i = Items.Length - 1; i > _head; i--) 
							Items[i] = Items[i - 1];
					}
					else
					{
						_head = 0;
					}

					_tail = (_tail + 1) % Items.Length;

					for (int i = _tail; i > 0; i--) 
						Items[i] = Items[i - 1];

					Items[0] = last;
					if (_tail == _head) _head = (_head + 1) % Items.Length;
				}
			}
			else
			{
				_tail = 0;
			}

			Items[_head] = item;
			Count = (Count + 1).NotAbove(Items.Length);
			_version++;
		}

		/// <inheritdoc />
		public bool Remove(T item)
		{
			if (Count == 0) return false;
			int index = Array.IndexOf(Items, item, 0, Count);
			if (index < 0) return false;
			RemoveAtInternal(index);
			return true;
		}

		public T Dequeue()
		{
			if (Count == 0) throw new InvalidOperationException("Collection is empty.");
			return RemoveAtInternal(_head);
		}

		public T Pop()
		{
			if (Count == 0) throw new InvalidOperationException("Collection is empty.");
			return RemoveAtInternal(_tail);
		}

		public T PeekHead()
		{
			if (Count == 0) throw new InvalidOperationException("Collection is empty.");
			return Items[_head];
		}

		public T PeekTail()
		{
			if (Count == 0) throw new InvalidOperationException("Collection is empty.");
			return Items[_tail];
		}

		/// <inheritdoc cref="ICollection{T}" />
		public void Clear()
		{
			Count = 0;
			_head = 0;
			_tail = -1;
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
			if (_tail >= _head) return Array.IndexOf(Items, item, _head, Count) > -1;
			return Array.IndexOf(Items, item, _head, Items.Length - _head) > -1 
					|| Array.IndexOf(Items, item, 0, _tail) > -1;
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);

			if (_tail < _head)
			{
				// The existing buffer is split, so we have to copy it in parts
				int length = Items.Length - _head;
				Array.Copy(Items, _head, array, arrayIndex, length);
				Array.Copy(Items, 0, array, arrayIndex + length, Count - length);
			}
			else
			{
				// The existing buffer is whole
				Array.Copy(Items, _head, array, arrayIndex, Count);
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
				int length = Items.Length - _head;
				Array.Copy(Items, _head, array, arrayIndex, length);
				Array.Copy(Items, 0, array, arrayIndex + length, Count - length);
			}
			else
			{
				// The existing buffer is whole
				Array.Copy(Items, _head, array, arrayIndex, Count);
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

		private T RemoveAtInternal(int index)
		{
			if (_tail < 0) throw new ArgumentOutOfRangeException(nameof(index));

			T item = Items[index];

			if (_tail >= _head)
			{
				// case 1: tail >= head
				if (index < _head) throw new ArgumentOutOfRangeException(nameof(index));

				if (index == _head)
				{
					// case a: index == head
					_head = (_head + 1) % Items.Length;
				}
				else if (index <= _tail)
				{
					// case b: index > head and index <= tail
					for (int i = index; i < _tail - 1; i++) 
						Items[i] = Items[i + 1];

					_tail--;
					if (_tail < 0) _tail = Items.Length - 1;
				}
				else
				{
					// case c: index > head and index > tail
					throw new ArgumentOutOfRangeException(nameof(index));
				}
			}
			else
			{
				// case 2: tail < head
				if (index <= _tail)
				{
					// case a: index <= tail
					_tail--;
					if (_tail < 0) _tail = Items.Length - 1;
				}
				else
				{
					// index > tail
					if (index < _head)
					{
						// case b: index > tail and index < head
						throw new ArgumentOutOfRangeException(nameof(index));
					}

					// case c: index > tail and index >= head
					for (int i = index; i > _head; i--) 
						Items[i] = Items[i - 1];

					_head = (_head + 1) % Items.Length;
				}
			}

			Count = (Count - 1).NotBelow(0);
			
			if (Count == 0)
			{
				_head = 0;
				_tail = -1;
			}

			_version++;
			return item;
		}

		[NotNull]
		public static ICollection<T> Synchronized(CircularBuffer<T> list)
		{
			return new SynchronizedCollection(list);
		}
	}
}