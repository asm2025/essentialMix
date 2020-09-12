using JetBrains.Annotations;

namespace asm.Collections.DebugView
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	public class Dbg_RootedHeapDebugView<TNode, TKey, TValue>
		where TNode : class, IKeyedNode<TKey, TValue>
	{
		private readonly RootedHeap<TNode, TKey, TValue> _heap;

		public Dbg_RootedHeapDebugView([NotNull] RootedHeap<TNode, TKey, TValue> heap)
		{
			_heap = heap;
		}

		[NotNull]
		public TNode Head => _heap.Head;
	}
}