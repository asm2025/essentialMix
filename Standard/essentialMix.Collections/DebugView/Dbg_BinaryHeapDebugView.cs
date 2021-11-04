using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Collections.DebugView
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	public class Dbg_BinaryHeapDebugView<TNode, TValue>
		where TNode : class, ITreeNode<TNode, TValue>
	{
		private readonly BinaryHeap<TNode, TValue> _heap;

		public Dbg_BinaryHeapDebugView([NotNull] BinaryHeap<TNode, TValue> heap)
		{
			_heap = heap;
		}

		[NotNull]
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public TNode[] Nodes
		{
			get
			{
				TNode[] nodes = new TNode[_heap.Count];
				_heap.CopyTo(nodes, 0);
				return nodes;
			}
		}
	}

	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	public class Dbg_BinaryHeapDebugView<TNode, TKey, TValue>
		where TNode : class, ITreeNode<TNode, TKey, TValue>
	{
		private readonly BinaryHeap<TNode, TKey, TValue> _heap;

		public Dbg_BinaryHeapDebugView([NotNull] BinaryHeap<TNode, TKey, TValue> heap)
		{
			_heap = heap;
		}

		[NotNull]
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public TNode[] Nodes
		{
			get
			{
				TNode[] nodes = new TNode[_heap.Count];
				_heap.CopyTo(nodes, 0);
				return nodes;
			}
		}
	}
}