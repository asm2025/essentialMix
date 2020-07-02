using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	public class MinBinomialHeap<T> : BinomialHeap<T>
	{
		/// <inheritdoc />
		public MinBinomialHeap()
			: this((IComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap([NotNull] BinomialNode<T> head)
			: this(head, null)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap([NotNull] BinomialNode<T> head, IComparer<T> comparer)
			: base(head, comparer)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		/// <inheritdoc />
		public MinBinomialHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		public override T Value()
		{
			if (Head == null) return default(T);

			BinomialNode<T> minimum = Head;

			foreach (BinomialNode<T> node in Head.Siblings())
			{
				if (Comparer.IsLessThan(node.Value, minimum.Value)) minimum = node;
			}

			return minimum.Value;
		}

		/// <inheritdoc />
		public override T ExtractValue()
		{
			if (Head == null) return default(T);

			BinomialNode<T> minimum = Head
							, minPrev = null
							, next = minimum.Sibling
							, nextPrev = minimum;

			while (next != null)
			{
				if (Comparer.IsLessThan(next.Value, minimum.Value))
				{
					minimum = next;
					minPrev = nextPrev;
				}

				nextPrev = next;
				next = next.Sibling;
			}

			RemoveRoot(minimum, minPrev);
			return minimum.Value;
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
						if (Comparer.IsLessThan(current.Value, next.Value))
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
			}

			_version++;
			return newHead;
		}

		/// <inheritdoc />
		[NotNull]
		protected override BinomialHeap<T> NewHeap(BinomialNode<T> head) { return new MinBinomialHeap<T>(head); }

		/// <inheritdoc />
		[NotNull]
		protected override BinomialNode<T> BubbleUp(Stack<BinomialNode<T>> stack, BinomialNode<T> node, bool toRoot = false)
		{
			BinomialNode<T> parent;

			while (stack.Count > 0 && (parent = stack.Pop()) != null && (toRoot || Comparer.IsLessThan(node.Value, parent.Value)))
			{
				node.Swap(parent);
				node = parent;
			}

			return node;
		}
	}
}