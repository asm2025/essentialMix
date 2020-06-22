using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Collections
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	//[DebuggerNonUserCode]
	//internal sealed class asm_RedBlackTreeDebugView<T>
	//{
	//	private readonly RedBlackTree<T> _tree;

	//	public asm_RedBlackTreeDebugView([NotNull] RedBlackTree<T> tree)
	//	{
	//		_tree = tree;
	//	}

	//	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	//	[NotNull]
	//	public RedBlackNode<T> Root => _tree.Root;
	//}
}