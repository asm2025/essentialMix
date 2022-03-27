using JetBrains.Annotations;

namespace essentialMix.Collections.DebugView;

/*
* VS IDE can't differentiate between types with the same name from different
* assembly. So we need to use different names for collection debug view for
* collections in this solution assemblies.
*/
public class Dbg_BTreeDebugView<T>
{
	private readonly BTree<T> _tree;

	public Dbg_BTreeDebugView([NotNull] BTree<T> tree)
	{
		_tree = tree;
	}

	[NotNull]
	public BTree<T>.Node Root => _tree.Root;
}
