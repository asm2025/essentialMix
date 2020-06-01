using System;
using JetBrains.Annotations;

namespace asm.Collections
{
	public enum BinaryNodeType : byte
	{
		Root,
		Left,
		Right
	}

	public static class BinaryNodeTypeExtension
	{
		public static BinaryNodeType NodeType<TNode, T>([NotNull] this LinkedBinaryNode<TNode, T> thisValue, LinkedBinaryNode<TNode, T> parent)
			where TNode : LinkedBinaryNode<TNode, T>
		{
			if (parent == null) return BinaryNodeType.Root;
			if (ReferenceEquals(parent.Left, thisValue)) return BinaryNodeType.Left;
			if (ReferenceEquals(parent.Right, thisValue)) return BinaryNodeType.Right;
			throw new ArgumentException("Node has unknown relationship to the parent.", nameof(parent));
		}
	}
}