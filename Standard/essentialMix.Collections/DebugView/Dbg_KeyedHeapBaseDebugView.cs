using JetBrains.Annotations;

namespace essentialMix.Collections.DebugView
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	public class Dbg_KeyedHeapBaseDebugView<TNode, TKey, TValue>
		where TNode : class, IKeyedNode<TKey, TValue>
	{
		private readonly KeyedHeap<TNode, TKey, TValue> _heap;

		public Dbg_KeyedHeapBaseDebugView([NotNull] KeyedHeap<TNode, TKey, TValue> heap)
		{
			_heap = heap;
		}

		[NotNull]
		public TNode Head => _heap.Head;
	}
}