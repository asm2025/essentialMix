using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class LinkedListExtension
	{
		public static void Reverse<T>([NotNull] this LinkedList<T> thisValue)
		{
			if (thisValue.Count < 2) return;

			LinkedListNode<T> first = thisValue.First;

			while (first.Next != null)
			{
				LinkedListNode<T> next = first.Next;
				thisValue.Remove(next);
				thisValue.AddFirst(next.Value);
			}
		}

		public static T PopFirst<T>([NotNull] this LinkedList<T> thisValue)
		{
			if (thisValue.Count == 0) throw new InvalidOperationException("List is empty.");

			T result;

			if (thisValue is ICollection collection && collection.IsSynchronized)
			{
				lock (collection.SyncRoot)
				{
					result = thisValue.First.Value;
					thisValue.RemoveFirst();
					return result;
				}
			}

			lock (thisValue)
			{
				result = thisValue.First.Value;
				thisValue.RemoveFirst();
				return result;
			}
		}

		public static T PopLast<T>([NotNull] this LinkedList<T> thisValue)
		{
			if (thisValue.Count == 0) throw new InvalidOperationException("List is empty.");

			T result;

			if (thisValue is ICollection collection && collection.IsSynchronized)
			{
				lock (collection.SyncRoot)
				{
					result = thisValue.Last.Value;
					thisValue.RemoveLast();
					return result;
				}
			}

			lock (thisValue)
			{
				result = thisValue.Last.Value;
				thisValue.RemoveLast();
				return result;
			}
		}
	}
}