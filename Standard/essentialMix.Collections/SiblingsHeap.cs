using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public abstract class SiblingsHeap<TNode, T> : Heap<TNode, T>
		where TNode : class, ISiblingNode<TNode, T>
	{
		/// <inheritdoc />
		protected SiblingsHeap()
			: this((IComparer<T>)null)
		{
		}

		protected SiblingsHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		protected SiblingsHeap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		protected SiblingsHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		public virtual bool Equals(SiblingsHeap<TNode, T> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (GetType() != other.GetType() 
				|| Count != other.Count 
				|| !Comparer.Equals(other.Comparer)) return false;
			if (Count == 0) return true;

			using (IEnumerator<T> thisEnumerator = GetEnumerator())
			{
				using (IEnumerator<T> otherEnumerator = other.GetEnumerator())
				{
					bool thisMoved = thisEnumerator.MoveNext();
					bool otherMoved = otherEnumerator.MoveNext();

					while (thisMoved && otherMoved)
					{
						if (Comparer.Compare(thisEnumerator.Current, otherEnumerator.Current) != 0) return false;
						thisMoved = thisEnumerator.MoveNext();
						otherMoved = otherEnumerator.MoveNext();
					}

					if (thisMoved ^ otherMoved) return false;
				}
			}

			return true;
		}
	}

	[Serializable]
	public abstract class SiblingsHeap<TNode, TKey, TValue> : KeyedHeap<TNode, TKey, TValue>
		where TNode : class, ISiblingNode<TNode, TKey, TValue>
	{
		/// <inheritdoc />
		protected SiblingsHeap()
			: this((IComparer<TKey>)null)
		{
		}

		protected SiblingsHeap(IComparer<TKey> comparer)
			: base(comparer)
		{
		}

		protected SiblingsHeap([NotNull] IEnumerable<TValue> enumerable)
			: this(enumerable, null)
		{
		}

		protected SiblingsHeap([NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: base(enumerable, comparer)
		{
		}

		public virtual bool Equals(SiblingsHeap<TNode, TKey, TValue> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (GetType() != other.GetType() 
				|| Count != other.Count 
				|| !ValueComparer.Equals(other.ValueComparer)) return false;
			if (Count == 0) return true;

			using (IEnumerator<TValue> thisEnumerator = GetEnumerator())
			{
				using (IEnumerator<TValue> otherEnumerator = other.GetEnumerator())
				{
					bool thisMoved = thisEnumerator.MoveNext();
					bool otherMoved = otherEnumerator.MoveNext();

					while (thisMoved && otherMoved)
					{
						if (!ValueComparer.Equals(thisEnumerator.Current, otherEnumerator.Current)) return false;
						thisMoved = thisEnumerator.MoveNext();
						otherMoved = otherEnumerator.MoveNext();
					}

					if (thisMoved ^ otherMoved) return false;
				}
			}

			return true;
		}
	}

	public static class SiblingsHeapExtension
	{
		public static void WriteTo<TNode, T>([NotNull] this SiblingsHeap<TNode, T> thisValue, [NotNull] TextWriter writer)
			where TNode : class, ISiblingNode<TNode, T>
		{
			if (thisValue.Head == null) return;

			StringBuilder indent = new StringBuilder();
			Stack<(TNode Node, int Level)> stack = new Stack<(TNode Node, int Level)>(1);
			stack.Push((thisValue.Head, 0));

			while (stack.Count > 0)
			{
				(TNode node, int level) = stack.Pop();
				int n = Constants.INDENT * level;

				if (indent.Length > n) indent.Length = n;
				else if (indent.Length < n) indent.Append(' ', n - indent.Length);

				writer.Write(indent);

				if (node == null)
				{
					writer.WriteLine(Constants.NULL);
					continue;
				}

				writer.WriteLine(node.ToString(level));
				if (node.Sibling != null) stack.Push((node.Sibling, level));
				if (node.Child != null) stack.Push((node.Child, level + 1));
			}
		}

		public static void WriteTo<TNode, TKey, TValue>([NotNull] this SiblingsHeap<TNode, TKey, TValue> thisValue, [NotNull] TextWriter writer)
			where TNode : class, ISiblingNode<TNode, TKey, TValue>
		{
			if (thisValue.Head == null) return;

			StringBuilder indent = new StringBuilder();
			Stack<(TNode Node, int Level)> stack = new Stack<(TNode Node, int Level)>(1);
			stack.Push((thisValue.Head, 0));

			while (stack.Count > 0)
			{
				(TNode node, int level) = stack.Pop();
				int n = Constants.INDENT * level;

				if (indent.Length > n) indent.Length = n;
				else if (indent.Length < n) indent.Append(' ', n - indent.Length);

				writer.Write(indent);

				if (node == null)
				{
					writer.WriteLine(Constants.NULL);
					continue;
				}

				writer.WriteLine(node.ToString(level));
				if (node.Sibling != null) stack.Push((node.Sibling, level));
				if (node.Child != null) stack.Push((node.Child, level + 1));
			}
		}
	}
}