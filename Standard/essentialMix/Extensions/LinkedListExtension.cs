using System;
using System.Collections;
using System.Collections.Generic;
using essentialMix.Exceptions.Collections;
using JetBrains.Annotations;

namespace essentialMix.Extensions
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
			if (thisValue.Count == 0) throw new CollectionIsEmptyException();

			T result;

			if (thisValue is ICollection { IsSynchronized: true } collection)
			{
				lock(collection.SyncRoot)
				{
					result = thisValue.First.Value;
					thisValue.RemoveFirst();
					return result;
				}
			}

			lock(thisValue)
			{
				result = thisValue.First.Value;
				thisValue.RemoveFirst();
				return result;
			}
		}

		public static T PopLast<T>([NotNull] this LinkedList<T> thisValue)
		{
			if (thisValue.Count == 0) throw new CollectionIsEmptyException();

			T result;

			if (thisValue is ICollection { IsSynchronized: true } collection)
			{
				lock(collection.SyncRoot)
				{
					result = thisValue.Last.Value;
					thisValue.RemoveLast();
					return result;
				}
			}

			lock(thisValue)
			{
				result = thisValue.Last.Value;
				thisValue.RemoveLast();
				return result;
			}
		}

		public static void RemoveAll<T>([NotNull] this LinkedList<T> thisValue, [NotNull] Predicate<T> predicate)
		{
			if (thisValue.Count == 0) return;

			LinkedListNode<T> next = thisValue.First;

			while (next != null)
			{
				if (predicate(next.Value))
				{
					LinkedListNode<T> remove = next;
					next = next.Next;
					thisValue.Remove(remove);
				}
				else
				{
					next = next.Next;
				}
			}
		}
	}
}