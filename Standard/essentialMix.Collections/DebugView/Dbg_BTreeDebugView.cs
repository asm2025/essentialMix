using JetBrains.Annotations;

namespace essentialMix.Collections.DebugView;

/*
* VS IDE can't differentiate between types with the same name from different
* assembly. So we need to use different names for collection debug view for
* collections in this solution assemblies.
*/
public class Dbg_BTreeDebugView<TBlock, TNode, T>
	where TBlock : class, ITreeBlock<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	private readonly IBTreeBase<TBlock, TNode, T> _tree;

	public Dbg_BTreeDebugView([NotNull] IBTreeBase<TBlock, TNode, T> tree)
	{
		_tree = tree;
	}

	[NotNull]
	public TBlock Root => _tree.Root;
}

/*
* VS IDE can't differentiate between types with the same name from different
* assembly. So we need to use different names for collection debug view for
* collections in this solution assemblies.
*/
public class Dbg_BTreeDebugView<TBlock, TNode, TKey, TValue>
	where TBlock : ITreeBlock<TBlock, TNode, TKey, TValue>
	where TNode : ITreeNode<TNode, TKey, TValue>
{
	private readonly IBTree<TBlock, TNode, TKey, TValue> _tree;

	public Dbg_BTreeDebugView([NotNull] IBTree<TBlock, TNode, TKey, TValue> tree)
	{
		_tree = tree;
	}

	[NotNull]
	public TBlock Root => _tree.Root;
}
