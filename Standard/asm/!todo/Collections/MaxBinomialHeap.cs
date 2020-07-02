using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	public class MaxBinomialHeap<T> : BinomialHeap<T>
	{
		/// <inheritdoc />
		public MaxBinomialHeap()
			: this((IComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap([NotNull] BinomialNode<T> head)
			: this(head, null)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap([NotNull] BinomialNode<T> head, IComparer<T> comparer)
			: base(head, comparer)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		/// <inheritdoc />
		public MaxBinomialHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		public override T Value()
		{
			if (Head == null) return default(T);

			BinomialNode<T> maximum = Head;

			foreach (BinomialNode<T> node in Head.Siblings())
			{
				if (Comparer.IsGreaterThan(node.Value, maximum.Value)) maximum = node;
			}

			return maximum.Value;
		}

		/// <inheritdoc />
		public override T ExtractValue()
		{
			if (Head == null) return default(T);

			BinomialNode<T> maximum = Head
							, maxPrev = null
							, next = maximum.Sibling
							, nextPrev = maximum;

			while (next != null)
			{
				if (Comparer.IsGreaterThan(next.Value, maximum.Value))
				{
					maximum = next;
					maxPrev = nextPrev;
				}

				nextPrev = next;
				next = next.Sibling;
			}

			RemoveRoot(maximum, maxPrev);
			return maximum.Value;
		}

		/// <inheritdoc />
		public override BinomialNode<T> Union(BinomialHeap<T> heap)
		{
			BinomialNode<T> newHead = Merge(heap);
			Head = null;
			heap.Head = null;
			
			if (newHead != null)
			{
				BinomialNode<T> prev = null, current = newHead, next = newHead.Sibling;

				while (next != null)
				{
					if (current.Degree != next.Degree || next.Sibling != null && next.Sibling.Degree == current.Degree)
					{
						prev = current;
						current = next;
					}
					else
					{
						if (Comparer.IsGreaterThan(current.Value, next.Value))
						{
							current.Sibling = next.Sibling;
							current.Link(next);
						}
						else
						{
							if (prev == null) newHead = next;
							else prev.Sibling = next;

							next.Link(current);
							current = next;
						}
					}

					next = current.Sibling;
				}

				return newHead;
			}

			_version++;
			return null;
		}

		/// <inheritdoc />
		[NotNull]
		protected override BinomialHeap<T> NewHeap(BinomialNode<T> head) { return new MaxBinomialHeap<T>(head); }

		/// <inheritdoc />
		[NotNull]
		protected override BinomialNode<T> BubbleUp(Stack<BinomialNode<T>> stack, BinomialNode<T> node, bool toRoot = false)
		{
			BinomialNode<T> parent;

			while (stack.Count > 0 && (parent = stack.Pop()) != null && (toRoot || Comparer.IsGreaterThan(node.Value, parent.Value)))
			{
				node.Swap(parent);
				node = parent;
			}

			return node;
		}
	}
}