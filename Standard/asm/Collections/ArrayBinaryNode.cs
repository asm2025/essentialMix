using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// A node metadata used to navigate the tree and print it
	/// </summary>
	[DebuggerDisplay("{Value}")]
	public struct ArrayBinaryNode<T>
	{
		[NotNull]
		private readonly ArrayBinaryTree<T> _tree;

		private int _index;

		/// <inheritdoc />
		public ArrayBinaryNode([NotNull] ArrayBinaryTree<T> tree)
			: this(tree, -1)
		{
		}

		public ArrayBinaryNode([NotNull] ArrayBinaryTree<T> tree, int index)
			: this()
		{
			_tree = tree;
			Index = index;
		}

		public int Index
		{
			get => _index;
			set
			{
				_index = value;

				if (_index < 0)
				{
					Invalidate();
					return;
				}

				ParentIndex = _tree.ParentIndex(_index);
				LeftIndex = _tree.LeftIndex(_index);
				RightIndex = _tree.RightIndex(_index);
			}
		}

		public T Value => _tree.Items[Index];

		public int ParentIndex { get; private set; }

		public int LeftIndex { get; private set; }

		public int RightIndex { get; private set; }

		public bool IsRoot => Index == 0;
		public bool IsLeft => Index > 0 && Index % 2 != 0;
		public bool IsRight => Index > 0 && Index % 2 == 0;
		public bool IsLeaf => LeftIndex < 0 && RightIndex < 0;
		public bool IsNode => LeftIndex > 0 && RightIndex > 0;
		public bool HasOneChild => (LeftIndex > 0) ^ (RightIndex > 0);
		public bool IsFull => !HasOneChild;

		public bool ParentIsRoot => ParentIndex == 0;
		public bool ParentIsLeft => ParentIndex > 0 && ParentIndex % 2 != 0;
		public bool ParentIsRight => ParentIndex > 0 && ParentIndex % 2 == 0;

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		internal string ToString(int depth)
		{
			return $"{Value} :D{depth}";
		}

		public IEnumerable<ArrayBinaryNode<T>> Ancestors()
		{
			if (ParentIndex <= 0) yield break;

			ArrayBinaryNode<T> node = new ArrayBinaryNode<T>(_tree, ParentIndex);

			while (node.Index > -1)
			{
				yield return node;
				node.Index = node.ParentIndex;
			}
		}

		public int LeftMost()
		{
			int index = Index, next = _tree.LeftIndex(index);

			while (next > -1)
			{
				index = next;
				next = _tree.LeftIndex(next);
			}

			return index;
		}

		public int RightMost()
		{
			int index = Index, next = _tree.RightIndex(index);

			while (next > -1)
			{
				index = next;
				next = _tree.RightIndex(next);
			}

			return index;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public void Swap(int other)
		{
			T tmp = _tree.Items[other];
			_tree.Items[other] = Value;
			_tree.Items[Index] = tmp;
		}

		private void Invalidate()
		{
			_index = ParentIndex = LeftIndex = RightIndex = -1;
		}

		public static implicit operator T(ArrayBinaryNode<T> node) { return node.Value; }
	}
}