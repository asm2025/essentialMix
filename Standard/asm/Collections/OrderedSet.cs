using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using JetBrains.Annotations;
using asm.Extensions;

namespace asm.Collections
{
	[Serializable]
	public class OrderedSet<T> : ISet<T>, ICollection<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, ISerializable, IDeserializationCallback
	{
		private int _capacity;
		private IDictionary<T, LinkedListNode<T>> _dictionary;
		private LinkedList<T> _linkedList;
		
		[NonSerialized]
		private SerializationInfo _siInfo;

		/// <inheritdoc />
		public OrderedSet()
			: this(0, EqualityComparer<T>.Default)
		{
		}

		/// <inheritdoc />
		public OrderedSet(int capacity)
			: this(capacity, EqualityComparer<T>.Default)
		{
		}

		/// <inheritdoc />
		public OrderedSet(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		public OrderedSet([NotNull] IEnumerable<T> collection)
			: this(collection, EqualityComparer<T>.Default)
		{
		}

		/// <inheritdoc />
		public OrderedSet([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
			foreach (T item in collection) 
				Add(item);
		}

		public OrderedSet(int capacity, IEqualityComparer<T> comparer)
		{
			_capacity = capacity.NotBelow(0);
			Comparer = comparer ?? EqualityComparer<T>.Default;
			_dictionary = new Dictionary<T, LinkedListNode<T>>(_capacity, Comparer);
			_linkedList = new LinkedList<T>();
		}

		protected OrderedSet(SerializationInfo info, StreamingContext context)
		{
			_siInfo = info;
		}

		/// <inheritdoc />
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		[SecurityCritical]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Capacity", _capacity);
			info.AddValue("Comparer", Comparer, typeof(IEqualityComparer<T>));

			T[] array;

			if (_dictionary.Count > 0)
			{
				array = new T[_dictionary.Count];
				_linkedList.CopyTo(array, 0);
			}
			else
			{
				array = null;
			}

			info.AddValue("Elements", array, typeof(T[]));
		}

		/// <inheritdoc />
		void IDeserializationCallback.OnDeserialization(object sender)
		{
			if (_siInfo == null) return;
			_capacity = _siInfo.GetInt32("Capacity");
			Comparer = (IEqualityComparer<T>)_siInfo.GetValue("Comparer", typeof(IEqualityComparer<T>));
			_dictionary = new Dictionary<T, LinkedListNode<T>>(_capacity, Comparer);
			_linkedList = new LinkedList<T>();

			T[] array = (T[])_siInfo.GetValue("Elements", typeof(T[]));
			
			if (array != null && array.Length > 0)
			{
				foreach (T item in array)
					Add(item);
			}

			_siInfo = null;
		}

		public IEqualityComparer<T> Comparer { get; private set; }

		/// <inheritdoc />
		public bool IsReadOnly => _dictionary.IsReadOnly;

		/// <inheritdoc />
		public int Count => _linkedList.Count;

		/// <inheritdoc />
		int IReadOnlyCollection<T>.Count => Count;

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return _linkedList.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public bool Add(T item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));
			if (_dictionary.ContainsKey(item)) return false;
			
			LinkedListNode<T> node =_linkedList.AddLast(item);
			_dictionary.Add(item, node);
			return true;
		}

		/// <inheritdoc />
		void ICollection<T>.Add(T item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));
			Add(item);
		}

		/// <inheritdoc />
		public bool Remove(T item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			bool found = _dictionary.TryGetValue(item, out LinkedListNode<T> node);
			if (!found) return false;
			_dictionary.Remove(item);
			_linkedList.Remove(node);
			return true;
		}

		public int RemoveWhere([NotNull] Predicate<T> match)
		{
			int n = 0;

			foreach (T item in this.Where(e => match(e)))
				Remove(item);

			return n;
		}

		/// <inheritdoc />
		public void Clear()
		{
			_dictionary.Clear();
			_linkedList.Clear();
		}

		/// <inheritdoc />
		public bool Contains(T item) { return item != null && _dictionary.ContainsKey(item); }

		public void CopyTo([NotNull] T[] array)
		{
			_linkedList.CopyTo(array, 0);
		}

		/// <inheritdoc />
		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			_linkedList.CopyTo(array, arrayIndex);
		}

		public void CopyTo([NotNull] T[] array, int arrayIndex, int count)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			Count.ValidateRange(arrayIndex, ref count);
			array.Length.ValidateRange(arrayIndex, ref count);
			if (count == 0 || Count == 0) return;

			int i = arrayIndex;

			foreach (T item in _linkedList.Skip(arrayIndex).Take(count))
			{
				array[i] = item;
				++i;
			}
		}

		/// <inheritdoc />
		public void UnionWith(IEnumerable<T> other)
		{
			if (other == null) throw new ArgumentNullException(nameof(other));

			foreach (T item in other) 
				Add(item);
		}

		/// <inheritdoc />
		public void IntersectWith(IEnumerable<T> other)
		{
			if (other == null) throw new ArgumentNullException(nameof(other));
			if (Count == 0 || Equals(other, this)) return;
			if (!(other is ICollection<T> collection)) return;
			if (!AreComparersEqual(other)) return;

			T[] nonExist = _dictionary.Keys.Where(e => !collection.Contains(e)).ToArray();
			if (nonExist.Length == 0) return;

			foreach (T item in nonExist)
				Remove(item);
		}

		/// <inheritdoc />
		public void ExceptWith(IEnumerable<T> other)
		{
			if (other == null) throw new ArgumentNullException(nameof(other));
			if (Count == 0) return;

			if (Equals(this, other))
			{
				Clear();
				return;
			}

			foreach (T item in other)
				Remove(item);
		}

		/// <inheritdoc />
		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			if (other == null) throw new ArgumentNullException(nameof(other));

			if (Count == 0)
			{
				UnionWith(other);
				return;
			}

			if (Equals(this, other))
			{
				Clear();
				return;
			}

			if (!AreComparersEqual(other) || !(other is ICollection<T> collection)) return;

			foreach (T item in collection)
			{
				if (Remove(item)) continue;
				Add(item);
			}
		}

		/// <inheritdoc />
		public bool IsSubsetOf(IEnumerable<T> other)
		{
			if (other == null) throw new ArgumentNullException(nameof(other));
			if (Count == 0 || Equals(other, this)) return true;

			if (AreComparersEqual(other, out ISet<T> set))
			{
				if (Count > set.Count) return false;

				foreach (T item in this)
				{
					if (!set.Contains(item)) return false;
				}

				return true;
			}

			if (!(other is IReadOnlyCollection<T> readOnlyCollection)) return false;
			(int Unique, int Unfound) uniqueAndUnfound = CheckUniqueAndUnfoundElements(readOnlyCollection, false);
			return uniqueAndUnfound.Unique == Count;
		}

		/// <inheritdoc />
		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			if (other == null) throw new ArgumentNullException(nameof(other));
			if (Equals(other, this)) return false;

			if (other is ICollection<T> collection)
			{
				if (collection.Count == 0) return false;
				if (Count == 0) return collection.Count > 0;

				if (AreComparersEqual(other, out ISet<T> set))
				{
					if (Count >= set.Count) return false;

					foreach (T item in this)
					{
						if (!set.Contains(item)) return false;
					}

					return true;
				}
			}

			if (!(other is IReadOnlyCollection<T> readOnlyCollection)) return false;
			(int Unique, int Unfound) uniqueAndUnfound = CheckUniqueAndUnfoundElements(readOnlyCollection, false);
			return uniqueAndUnfound.Unique == Count && uniqueAndUnfound.Unfound > 0;
		}

		/// <inheritdoc />
		public bool IsSupersetOf(IEnumerable<T> other)
		{
			if (other == null) throw new ArgumentNullException(nameof(other));
			if (Equals(other, this)) return true;

			if (other is ICollection<T> collection)
			{
				if (collection.Count == 0) return true;
				if (AreComparersEqual(other, out ISet<T> set) && set.Count > Count) return false;
			}

			return other is IReadOnlyCollection<T> readOnlyCollection && readOnlyCollection.All(Contains);
		}

		/// <inheritdoc />
		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			if (other == null) throw new ArgumentNullException(nameof(other));
			if (Count == 0 || Equals(other, this)) return false;

			if (other is ICollection<T> collection)
			{
				if (collection.Count == 0) return true;
				if (Count == 0) return collection.Count > 0;
				if (AreComparersEqual(other, out ISet<T> set)) return set.Count < Count && set.All(Contains);
			}

			if (!(other is IReadOnlyCollection<T> readOnlyCollection)) return false;
			(int Unique, int Unfound) uniqueAndUnfound = CheckUniqueAndUnfoundElements(readOnlyCollection, true);
			return uniqueAndUnfound.Unique < Count;
		}

		/// <inheritdoc />
		public bool Overlaps(IEnumerable<T> other)
		{
			if (other == null) throw new ArgumentNullException(nameof(other));
			if (Count == 0) return false;
			if (Equals(other, this)) return true;
			return other is IReadOnlyCollection<T> readOnlyCollection && readOnlyCollection.Any(Contains);
		}

		/// <inheritdoc />
		public bool SetEquals(IEnumerable<T> other)
		{
			if (other == null) throw new ArgumentNullException(nameof(other));
			if (Equals(other, this)) return true;

			if (AreComparersEqual(other, out ISet<T> set))
			{
				if (Count != set.Count) return false;

				foreach (T item in this)
				{
					if (!set.Contains(item)) return false;
				}

				return true;
			}

			if (!(other is IReadOnlyCollection<T> readOnlyCollection) || readOnlyCollection.Count != Count) return false;
			(int Unique, int Unfound) uniqueAndUnfound = CheckUniqueAndUnfoundElements(readOnlyCollection, true);
			return uniqueAndUnfound.Unique == Count && uniqueAndUnfound.Unfound == 0;
		}

		private bool AreComparersEqual(IEnumerable<T> other) { return AreComparersEqual(other, out _); }

		private bool AreComparersEqual(IEnumerable<T> other, out ISet<T> set)
		{
			switch (other)
			{
				case HashSet<T> hashSet:
					set = hashSet;
					return hashSet.Comparer.Equals(Comparer);
				case SortedSet<T> sortedSet:
					set = sortedSet;
					return sortedSet.Comparer.Equals(Comparer);
				case OrderedSet<T> orderedSet:
					set = orderedSet;
					return orderedSet.Comparer.Equals(Comparer);
				default:
					set = null;
					return false;
			}
		}

		private (int Unique, int Unfound) CheckUniqueAndUnfoundElements([NotNull] IReadOnlyCollection<T> other, bool returnIfUnfound)
		{
			int unique = 0, notFound = 0;

			if (Count == 0)
			{
				notFound = other.HasMoreThan()
							? 1
							: 0;
				return (unique, notFound);
			}

			foreach (T item in other)
			{
				if (Contains(item)) 
					unique++;
				else
				{
					notFound++;
					if (returnIfUnfound) break;
				}
			}

			return (unique, notFound);
		}
	}
}