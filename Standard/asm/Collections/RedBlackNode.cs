using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class RedBlackNode<T> : LinkedBinaryNode<RedBlackNode<T>, T>
	{
		private RedBlackNode<T> _parent;

		internal RedBlackNode(T value)
			: base(value)
		{
		}
		
		public RedBlackNode<T> Parent
		{
			get => _parent;
			internal set
			{
				if (_parent == value) return;

				// reset old parent
				if (_parent != null)
				{
					/*
					* The comparison with this and parent.left/.right is essential because the node
					* could have moved to another parent. Don't use IsLeft or IsRight here.
					*/
					if (_parent.Left == this) _parent.Left = null;
					else if (_parent.Right == this) _parent.Right = null;
				}

				_parent = value;
			}
		}

		public override RedBlackNode<T> Left
		{
			get => base.Left;
			internal set
			{
				if (base.Left == value) return;
				// reset old left
				if (base.Left?._parent == this) base.Left._parent = null;
				base.Left = value;
				if (base.Left == null) return;
				base.Left._parent = this;
			}
		}

		public override RedBlackNode<T> Right
		{
			get => base.Right;
			internal set
			{
				if (base.Right == value) return;
				// reset old right
				if (base.Right?._parent == this) base.Right._parent = null;
				base.Right = value;
				if (base.Right == null) return;
				base.Right._parent = this;
			}
		}

		/// <summary>
		/// True means Red and False = no color or Black
		/// </summary>
		public bool Color { get; internal set; } = true;

		public bool IsRoot => _parent == null;

		public bool IsLeft => _parent?.Left == this;

		public bool IsRight => _parent?.Right == this;

		public bool HasRedParent => Parent != null && Parent.Color;

		public bool HasRedLeft => Left != null && Left.Color;

		public bool HasRedRight => Right != null && Right.Color;

		/// <inheritdoc />
		protected internal override string ToString(int depth, bool diagnostic)
		{
			return diagnostic
						? $"{Value} {(Color ? 'R' : 'B')}"
						: Convert.ToString(Value);
		}

		[ItemNotNull]
		public IEnumerable<RedBlackNode<T>> Ancestors()
		{
			RedBlackNode<T> node = _parent;

			while (node != null)
			{
				yield return node;
				node = node._parent;
			}
		}

		public RedBlackNode<T> Uncle()
		{
			// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
			return _parent?._parent == null
						? null // no parent or grand parent
						: _parent.IsLeft
							? _parent._parent.Right // uncle on the right
							: _parent._parent.Left;
		}

		public RedBlackNode<T> Sibling()
		{
			// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
			return _parent == null
						? null // no parent
						: IsLeft
							? _parent.Right // sibling on the right
							: _parent.Left;
		}

		public void SwapColor([NotNull] RedBlackNode<T> other)
		{
			bool tmp = other.Color;
			other.Color = Color;
			Color = tmp;
		}
	}
}