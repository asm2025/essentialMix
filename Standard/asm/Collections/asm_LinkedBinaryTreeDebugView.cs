using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Collections
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	[DebuggerNonUserCode]
	internal sealed class asm_LinkedBinaryTreeDebugView<TNode, T>
		where TNode : LinkedBinaryNode<TNode, T>
	{
		private readonly LinkedBinaryTree<TNode, T> _tree;

		public asm_LinkedBinaryTreeDebugView([NotNull] LinkedBinaryTree<TNode, T> tree)
		{
			_tree = tree;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		[NotNull]
		public TNode Root => _tree.Root;
	}

	[DebuggerNonUserCode]
	internal sealed class asm_LinkedBinaryTreeDebugView<T>
	{
		private readonly LinkedBinaryTree<T> _tree;

		public asm_LinkedBinaryTreeDebugView([NotNull] LinkedBinaryTree<T> tree)
		{
			_tree = tree;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		[NotNull]
		public LinkedBinaryNode<T> Root => _tree.Root;
	}
}