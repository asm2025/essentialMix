using System;
using System.Collections;
using System.Collections.Generic;

namespace asm.Collections
{
	[Serializable]
	public class UniqueStack<T> : Stack<T>, IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
	{
		private readonly HashSet<T> _hash;

		public UniqueStack()
		{
			_hash = new HashSet<T>();
		}

		public UniqueStack(int capacity)
			: base(capacity)
		{
			_hash = new HashSet<T>(EqualityComparer<T>.Default);
		}

		public UniqueStack(IEqualityComparer<T> comparer)
		{
			_hash = new HashSet<T>(comparer);
		}

		public new void Push(T item)
		{
			if (_hash.Contains(item)) return;
			_hash.Add(item);
			base.Push(item);
		}

		public new T Pop()
		{
			T item = base.Pop();
			_hash.Remove(item);
			return item;
		}

		public new void Clear()
		{
			_hash.Clear();
			base.Clear();
		}
	}
}