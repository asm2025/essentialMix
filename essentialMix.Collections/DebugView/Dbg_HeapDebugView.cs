using JetBrains.Annotations;

namespace essentialMix.Collections.DebugView;

/*
* VS IDE can't differentiate between types with the same name from different
* assembly. So we need to use different names for collection debug view for
* collections in this solution assemblies.
*/
public class Dbg_HeapDebugView<TNode, T>([NotNull] LinkedHeapBase<TNode, T> heap)
	where TNode : class, ITreeNode<TNode, T>
{
	private readonly LinkedHeapBase<TNode, T> _heap = heap;

	[NotNull]
	public TNode Head => _heap.Head;
}

/*
* VS IDE can't differentiate between types with the same name from different
* assembly. So we need to use different names for collection debug view for
* collections in this solution assemblies.
*/
public class Dbg_HeapDebugView<TNode, TKey, TValue>([NotNull] LinkedHeap<TNode, TKey, TValue> heap)
	where TNode : class, ITreeNode<TNode, TKey, TValue>
{
	private readonly LinkedHeap<TNode, TKey, TValue> _heap = heap;

	[NotNull]
	public TNode Head => _heap.Head;
}
