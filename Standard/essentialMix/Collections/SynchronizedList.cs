using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Threading;

namespace essentialMix.Collections
{
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public sealed class SynchronizedList<T> : IList<Lockable<T>>, ICollection<Lockable<T>>, IEnumerable<Lockable<T>>, IEnumerable, IList, ICollection, IReadOnlyList<Lockable<T>>, IReadOnlyCollection<Lockable<T>>
		where T : class
	{
		private readonly object _lock = new object();
		private readonly List<Lockable<T>> _list = new List<Lockable<T>>();
		private readonly IList _iList;

		public SynchronizedList()
		{
			_iList = _list;
		}

		public SynchronizedList([NotNull] IEnumerable<Lockable<T>> collection)
		{
			_list.AddRange(collection);
			_iList = _list;
		}

		public Lockable<T> this[int index]
		{
			get
			{
				lock(_lock)
				{
					return _list[index];
				}
			}
			set
			{
				lock(_lock)
				{
					_list[index] = value;
				}
			}
		}

		object IList.this[int index]
		{
			get => this[index];
			set
			{
				if (!(value is Lockable<T> lockable)) throw new InvalidCastException();
				this[index] = lockable;
			}
		}

		public int Count
		{
			get
			{
				lock(_lock)
				{
					return _list.Count;
				}
			}
		}

		int ICollection.Count => Count;

		int IReadOnlyCollection<Lockable<T>>.Count => Count;

		public bool IsReadOnly => _iList.IsReadOnly;

		bool ICollection<Lockable<T>>.IsReadOnly => IsReadOnly;

		public bool IsFixedSize => _iList.IsFixedSize;

		public object SyncRoot => _iList.SyncRoot;

		public bool IsSynchronized => _iList.IsSynchronized;

		public int Add(object value)
		{
			if (!(value is Lockable<T> lockable)) throw new InvalidCastException();
			Add(lockable);
			return Count - 1;
		}

		public void Add(Lockable<T> item)
		{
			lock(_lock)
			{
				_list.Add(item);
			}
		}

		public void Insert(int index, [NotNull] object value)
		{
			if (!(value is Lockable<T> lockable)) throw new InvalidCastException();
			Insert(index, lockable);
		}
		public void Insert(int index, Lockable<T> item)
		{
			lock(_lock)
			{
				_list.Insert(index, item);
			}
		}

		public void AddRange([NotNull] IEnumerable<Lockable<T>> collection)
		{
			lock(_lock)
			{
				_list.AddRange(collection);
			}
		}

		public void InsertRange(int index, [NotNull] IEnumerable<Lockable<T>> collection)
		{
			lock(_lock)
			{
				_list.InsertRange(index, collection);
			}
		}

		public void Remove([NotNull] object value)
		{
			if (!(value is Lockable<T> lockable)) throw new InvalidCastException();
			Remove(lockable);
		}

		public bool Remove(Lockable<T> item)
		{
			lock(_lock)
			{
				int index = _list.IndexOf(item);
				if (index < 0) return false;
				_list.RemoveAt(index);
				return true;
			}
		}

		public void RemoveAt(int index)
		{
			lock(_lock)
			{
				if (!index.InRangeRx(0, _list.Count)) throw new ArgumentOutOfRangeException(nameof(index));
				_list.RemoveAt(index);
			}
		}

		public void RemoveRange(int index, int count)
		{
			lock(_lock)
			{
				_list.RemoveRange(index, count);
			}
		}

		public int RemoveAll([NotNull] Predicate<Lockable<T>> match)
		{
			lock(_lock)
			{
				return _list.RemoveAll(match);
			}
		}

		public void Clear()
		{
			lock(_lock)
			{
				_list.Clear();
			}
		}

		public void Reverse(int index, int count)
		{
			lock(_lock)
			{
				_list.Reverse(index, count);
			}
		}

		public void Sort(int index, int count, IComparer<Lockable<T>> comparer)
		{
			lock(_lock)
			{
				_list.Sort(index, count, comparer);
			}
		}

		public void Sort([NotNull] Comparison<Lockable<T>> comparison)
		{
			lock(_lock)
			{
				_list.Sort(comparison);
			}
		}

		public int IndexOf([NotNull] object value)
		{
			if (!(value is Lockable<T> lockable)) throw new InvalidCastException();
			return IndexOf(lockable);
		}
		public int IndexOf(Lockable<T> item)
		{
			lock(_lock)
			{
				return _list.IndexOf(item);
			}
		}

		public bool Contains(object value)
		{
			if (!(value is Lockable<T> lockable)) throw new InvalidCastException();
			return Contains(lockable);
		}
		public bool Contains(Lockable<T> item)
		{
			lock(_lock)
			{
				return _list.Contains(item);
			}
		}

		public void CopyTo(Lockable<T>[] array, int arrayIndex)
		{
			lock(_lock)
			{
				_list.CopyTo(array, arrayIndex);
			}
		}
		public void CopyTo(Array array, int index) { _iList.CopyTo(array, index); }

		public IEnumerator<Lockable<T>> GetEnumerator()
		{
			lock(_lock)
			{
				return _list.GetEnumerator();
			}
		}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}