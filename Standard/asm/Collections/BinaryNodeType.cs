using JetBrains.Annotations;

namespace asm.Collections
{
	public enum BinaryNodeType : byte
	{
		None,
		Root,
		Left,
		Right
	}

	public static class BinaryNodeTypeExtension
	{
		public static BinaryNodeType NodeType<TNode, T>([NotNull] this LinkedBinaryNode<TNode, T> thisValue)
			where TNode : LinkedBinaryNode<TNode, T>
		{
			return thisValue.IsRoot
						? BinaryNodeType.Root
						: thisValue.IsLeft
							? BinaryNodeType.Left
							: thisValue.IsRight
								? BinaryNodeType.Right
								: BinaryNodeType.None;
		}

		public static BinaryNodeType NodeType<TNode, T>([NotNull] this LinkedBinaryNode<TNode, T> thisValue, LinkedBinaryNode<TNode, T> parent)
			where TNode : LinkedBinaryNode<TNode, T>
		{
			return parent == null
						? BinaryNodeType.Root
						: ReferenceEquals(parent.Left, thisValue)
							? BinaryNodeType.Left
							: ReferenceEquals(parent.Right, thisValue)
								? BinaryNodeType.Right
								: BinaryNodeType.None;
		}
	}
}