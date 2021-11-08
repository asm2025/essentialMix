using JetBrains.Annotations;

namespace essentialMix.Collections.DebugView
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	public class Dbg_HeapDebugView<TNode, T>
		where TNode : class, ITreeNode<TNode, T>
	{
		private readonly LinkedHeap<TNode, T> _heap;

		public Dbg_HeapDebugView([NotNull] LinkedHeap<TNode, T> heap)
		{
			_heap = heap;
		}

		[NotNull]
		public TNode Head => _heap.Head;
	}

	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	public class Dbg_HeapDebugView<TNode, TKey, TValue>
		where TNode : class, ITreeNode<TNode, TKey, TValue>
	{
		private readonly LinkedHeap<TNode, TKey, TValue> _heap;

		public Dbg_HeapDebugView([NotNull] LinkedHeap<TNode, TKey, TValue> heap)
		{
			_heap = heap;
		}

		[NotNull]
		public TNode Head => _heap.Head;
	}
}