using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using asm.Exceptions;
using asm.Exceptions.Collections;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Other.Microsoft.Collections
{
	// based on https://github.com/microsoft/referencesource/blob/master/System/compmod/system/collections/generic/linkedlist.cs
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(Other_Mscorlib_CollectionDebugView<>))]
	[ComVisible(false)]
	public class SinglyLinkedList<T> : ICollection<T>, ICollection, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback
	{
		private const string VALUES = "Values";

		[Serializable]
		public struct Enumerator : IEnumerator<T>, IEnumerator, ISerializable, IDeserializationCallback
		{
			private SinglyLinkedList<T> _list;
			private SinglyLinkedListNode<T> _node;
			private int _version;
			private int _index;

			private SerializationInfo siInfo; //A temporary variable which we need during deserialization.

			internal Enumerator([NotNull] SinglyLinkedList<T> list)
			{
				_list = list;
				_version = list._version;
				_node = list._head;
				Current = default(T);
				_index = 0;
				siInfo = null;
			}

			internal Enumerator(SerializationInfo info, StreamingContext context)
			{
				siInfo = info;
				_list = null;
				_version = 0;
				_node = null;
				Current = default(T);
				_index = 0;
			}

			public T Current { get; private set; }

			object IEnumerator.Current
			{
				get
				{
					if (!_index.InRangeRx(0, _list.Count)) throw new InvalidOperationException();
					return Current;
				}
			}

			public bool MoveNext()
			{
				if (_version != _list._version) throw new VersionChangedException();

				if (_node == null)
				{
					_index = _list.Count + 1;
					return false;
				}

				++_index;
				Current = _node._item;
				_node = _node._next;
				if (_node == _list._head) _node = null;
				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _list._version) throw new VersionChangedException();
				Current = default(T);
				_node = _list._head;
				_index = 0;
			}

			public void Dispose()
			{
			}

			void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue(nameof(_list), _list);
				info.AddValue(nameof(_version), _version);
				info.AddValue(nameof(Current), Current);
				info.AddValue(nameof(_index), _index);
			}

			void IDeserializationCallback.OnDeserialization(object sender)
			{
				if (_list != null) return; //Somebody had a dependency on this Dictionary and fixed us up before the ObjectManager got to it.
				if (siInfo == null) throw new SerializationException();

				_list = (SinglyLinkedList<T>)siInfo.GetValue(nameof(_list), typeof(SinglyLinkedList<T>));
				_version = siInfo.GetInt32(nameof(_version));
				Current = (T)siInfo.GetValue(nameof(Current), typeof(T));
				_index = siInfo.GetInt32(nameof(_index));
				if (_list.siInfo != null) _list.OnDeserialization(sender);

				if (!_index.InRangeRx(0, _list.Count))
				{
					// end of enumeration
					_node = null;
				}
				else
				{
					_node = _list.First;
					// We don't care if we can point to the correct node if the SinglyLinkedList was changed   
					// MoveNext will throw upon next call and Current has the correct value. 
					if (_node != null && _index != 0)
					{
						for (int i = 0; i < _index; i++) 
							_node = _node._next;

						if (_node == _list.First) _node = null;
					}
				}

				siInfo = null;
			}
		}

		protected internal int _version;

		// This LinkedList is a singly-Linked circular list.
		internal SinglyLinkedListNode<T> _head;
		internal SinglyLinkedListNode<T> _tail;
		internal int _count;

		private object _syncRoot;
		private SerializationInfo siInfo; //A temporary variable which we need during deserialization.
		
		public SinglyLinkedList()
		{
		}

		public SinglyLinkedList([NotNull] IEnumerable<T> collection)
		{
			foreach (T item in collection) 
				AddLast(item);
		}

		protected SinglyLinkedList(SerializationInfo info, StreamingContext context)
		{
			siInfo = info;
		}

		public int Count => _count;

		public SinglyLinkedListNode<T> First => _head;

		bool ICollection<T>.IsReadOnly => false;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) System.Threading.Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		[SecurityCritical]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			// Customized serialization for SinglyLinkedList.
			// We need to do this because it will be too expensive to Serialize each node.
			// This will give us the flexibility to change internal implementation freely in future.
			info.AddValue(nameof(_version), _version);
			info.AddValue(nameof(Count), _count); //This is the length of the bucket array.
			if (_count == 0) return;
			
			T[] array = new T[Count];
			CopyTo(array, 0);
			info.AddValue(VALUES, array, typeof(T[]));
		}

		public virtual void OnDeserialization(object sender)
		{
			if (siInfo == null) return; //Somebody had a dependency on this Dictionary and fixed us up before the ObjectManager got to it.

			int realVersion = siInfo.GetInt32(nameof(_version));
			int count = siInfo.GetInt32(nameof(Count));

			if (count > 0)
			{
				T[] array = (T[])siInfo.GetValue(VALUES, typeof(T[]));
				if (array == null) throw new SerializationException("Missing keys");

				foreach (T value in array) 
					AddLast(value);
			}
			else
			{
				_head = _tail = null;
			}

			_version = realVersion;
			siInfo = null;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		void ICollection<T>.Add(T value)
		{
			AddLast(value);
		}

		[NotNull]
		public SinglyLinkedListNode<T> AddAfter([NotNull] SinglyLinkedListNode<T> node, T value)
		{
			ValidateNode(node);
			SinglyLinkedListNode<T> result = new SinglyLinkedListNode<T>(node._list, value);
			InsertNodeAfterInternal(node, result);
			return result;
		}

		public void AddAfter([NotNull] SinglyLinkedListNode<T> node, [NotNull] SinglyLinkedListNode<T> newNode)
		{
			ValidateNode(node);
			ValidateNewNode(newNode);
			InsertNodeAfterInternal(node, newNode);
			newNode._list = this;
		}

		[NotNull]
		public SinglyLinkedListNode<T> AddBefore([NotNull] SinglyLinkedListNode<T> node, T value)
		{
			ValidateNode(node);
			SinglyLinkedListNode<T> previous = GetPreviousNode(node);
			SinglyLinkedListNode<T> result = new SinglyLinkedListNode<T>(node._list, value);
			InsertNodeAfterInternal(previous, result);
			return result;
		}

		public void AddBefore([NotNull] SinglyLinkedListNode<T> node, [NotNull] SinglyLinkedListNode<T> newNode)
		{
			ValidateNode(node);
			ValidateNewNode(newNode);
			SinglyLinkedListNode<T> previous = GetPreviousNode(node);
			InsertNodeAfterInternal(previous, newNode);
			newNode._list = this;
		}

		[NotNull]
		public SinglyLinkedListNode<T> AddFirst(T value)
		{
			SinglyLinkedListNode<T> result = new SinglyLinkedListNode<T>(this, value);
			InsertNodeAfterInternal(null, result);
			return result;
		}

		public void AddFirst([NotNull] SinglyLinkedListNode<T> node)
		{
			ValidateNewNode(node);
			InsertNodeAfterInternal(null, node);
			node._list = this;
		}

		[NotNull]
		public SinglyLinkedListNode<T> AddLast(T value)
		{
			SinglyLinkedListNode<T> result = new SinglyLinkedListNode<T>(this, value);
			InsertNodeAfterInternal(_tail, result);
			return result;
		}

		public void AddLast([NotNull] SinglyLinkedListNode<T> node)
		{
			ValidateNewNode(node);
			InsertNodeAfterInternal(_tail, node);
			node._list = this;
		}

		public bool Remove(T value)
		{
			SinglyLinkedListNode<T> node = Find(value);
			if (node == null) return false;
			RemoveNodeInternal(node);
			return true;
		}

		public void Remove([NotNull] SinglyLinkedListNode<T> node)
		{
			ValidateNode(node);
			RemoveNodeInternal(node);
		}

		public void RemoveFirst()
		{
			if (_head == null) throw new InvalidOperationException("List is empty.");
			RemoveNodeInternal(_head);
		}

		public void RemoveLast()
		{
			if (_tail == null) throw new InvalidOperationException("List is empty.");
			RemoveNodeInternal(_tail);
		}

		public void Clear()
		{
			SinglyLinkedListNode<T> current = _head;

			while (current != null)
			{
				SinglyLinkedListNode<T> temp = current;
				current = current.Next;   // use Next the instead of "next", otherwise it will loop forever
				temp.Invalidate();
			}

			_head = null;
			_count = 0;
			_version++;
		}

		public bool Contains(T value)
		{
			return Find(value) != null;
		}

		public SinglyLinkedListNode<T> Find(T value)
		{
			if (_head == null) return null;

			SinglyLinkedListNode<T> node = _head;
			EqualityComparer<T> comparer = EqualityComparer<T>.Default;
			
			if (value != null)
			{
				do
				{
					if (comparer.Equals(node._item, value)) return node;
					node = node._next;
				} while (node != _head);
			}
			else
			{
				do
				{
					if (node._item == null) return node;
					node = node._next;
				} while (node != _head);
			}

			return null;
		}

		public SinglyLinkedListNode<T> FindLast(T value)
		{
			if (_head == null) return null;

			SinglyLinkedListNode<T> node = null, next = _head;
			EqualityComparer<T> comparer = EqualityComparer<T>.Default;

			if (value != null)
			{
				if (comparer.Equals(_tail._item, value)) return _tail;

				while (next != null)
				{
					if (comparer.Equals(next._item, value)) node = next;
					next = next.Next;
				}
			}
			else
			{
				if (_tail._item == null) return _tail;

				while (next != null)
				{
					if (next._item == null) node = next;
					next = next.Next;
				}
			}

			return node;
		}

		public void CopyTo(T[] array, int index)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(index, Count);

			SinglyLinkedListNode<T> node = _head;
			if (node == null) return;

			do
			{
				array[index++] = node._item;
				node = node._next;
			} while (node != _head);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));
			if (Count == 0) return;

			if (array is T[] tArray)
			{
				CopyTo(tArray, index);
				return;
			}

			//
			// Catch the obvious case assignment will fail.
			// We can found all possible problems by doing the check though.
			// For example, if the element type of the Array is derived from T,
			// we can't figure out if we can successfully copy the element beforehand.
			//
			array.Length.ValidateRange(index, Count);

			Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
			Type sourceType = typeof(T);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[] objects)) throw new ArgumentException("Invalid array type", nameof(array));

			SinglyLinkedListNode<T> node = _head;
			
			try
			{
				if (node == null) return;
				
				do
				{
					objects[index++] = node._item;
					node = node._next;
				} while (node != _head);
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Invalid array type", nameof(array));
			}
		}

		private SinglyLinkedListNode<T> GetPreviousNode(SinglyLinkedListNode<T> node)
		{
			SinglyLinkedListNode<T> previous = null, next = _head;

			do
			{
				if (next == node) return previous;
				previous = next;
				next = next._next;
			}
			while (next != _head);

			return null;
		}

		private void InsertNodeAfterInternal(SinglyLinkedListNode<T> node, [NotNull] SinglyLinkedListNode<T> newNode)
		{
			if (node == null)
			{
				newNode._next = _head;

				if (_head == null)
				{
					_head = newNode;
					_head._next = _head;
					_tail = _head;
					_tail._next = _head;
				}
				else
				{
					if (_tail == _head) _tail = newNode;
					_head = newNode;
					_head._next ??= _head;
					_tail._next = _head;
				}
			}
			else
			{
				newNode._next = node._next;
				node._next = newNode;
				if (_tail == node) _tail = newNode;
			}

			_version++;
			_count++;
		}

		private void RemoveNodeInternal([NotNull] SinglyLinkedListNode<T> node)
		{
			Debug.Assert(node._list == this, "Deleting the node from another list!");
			Debug.Assert(_head != null, "This method shouldn't be called on empty list!");

			if (_head == node)
			{
				if (_head._next == _head)
				{
					_head = _tail = null;
				}
				else
				{
					_head = _head._next;
					_tail._next = _head;
				}
			}
			else
			{
				SinglyLinkedListNode<T> previous = GetPreviousNode(node) ?? throw new NotFoundException();
				previous._next = node._next;
				if (_tail == node) _tail = previous;
			}

			node.Invalidate();
			_count--;
			_version++;
		}

		private void ValidateNewNode([NotNull] SinglyLinkedListNode<T> node)
		{
			if (node._list == null) return;
			throw new InvalidOperationException();
		}

		private void ValidateNode([NotNull] SinglyLinkedListNode<T> node)
		{
			if (node._list == this) return;
			throw new InvalidOperationException();
		}
	}
}