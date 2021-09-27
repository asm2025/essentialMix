using JetBrains.Annotations;

namespace essentialMix.Collections.DebugView
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	public class Dbg_HeapBaseDebugView<TNode, T>
		where TNode : class, ITreeNode<T>
	{
		private readonly Heap<TNode, T> _heap;

		public Dbg_HeapBaseDebugView([NotNull] Heap<TNode, T> heap)
		{
			_heap = heap;
		}

		[NotNull]
		public TNode Head => _heap.Head;
	}
}