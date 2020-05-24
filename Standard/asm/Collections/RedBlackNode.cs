using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class RedBlackNode<T> : LinkedBinaryNode<RedBlackNode<T>, T>
	{
		internal RedBlackNode(T value)
			: base(value)
		{
		}

		/// <summary>
		/// True means Red and False = no color or Black
		/// </summary>
		public bool Color { get; internal set; } = true;

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

		public RedBlackNode<T> Uncle()
		{
			// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
			return Parent?.Parent == null
						? null // no parent or grand parent
						: Parent.IsLeft
							? Parent.Parent.Right // uncle on the right
							: Parent.Parent.Left;
		}

		public RedBlackNode<T> Sibling()
		{
			// https://www.geeksforgeeks.org/red-black-tree-set-3-delete-2/
			return Parent == null
						? null // no parent
						: IsLeft
							? Parent.Right // sibling on the right
							: Parent.Left;
		}

		public void SwapColor([NotNull] RedBlackNode<T> other)
		{
			bool tmp = other.Color;
			other.Color = Color;
			Color = tmp;
		}
	}
}