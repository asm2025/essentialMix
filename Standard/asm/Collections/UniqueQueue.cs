using System;
using System.Collections.Generic;

namespace asm.Collections
{
	[Serializable]
	public class UniqueQueue<T> : Queue<T>
	{
		private readonly HashSet<T> _hash;

		public UniqueQueue()
		{
			_hash = new HashSet<T>();
		}

		public UniqueQueue(int capacity)
			: base(capacity)
		{
			_hash = new HashSet<T>(EqualityComparer<T>.Default);
		}

		public UniqueQueue(IEqualityComparer<T> comparer)
		{
			_hash = new HashSet<T>(comparer);
		}

		public new void Enqueue(T item)
		{
			if (_hash.Contains(item)) return;
			_hash.Add(item);
			base.Enqueue(item);
		}

		public new T Dequeue()
		{
			T item = base.Dequeue();
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