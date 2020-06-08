using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class UndirectedGraphNodeBase<TNode, T> : GraphNode<TNode, T>
		where TNode : UndirectedGraphNodeBase<TNode, T>
	{
		protected UndirectedGraphNodeBase([NotNull] T value)
			: base(value)
		{
		}

		/// <inheritdoc />
		public override void Add(TNode to)
		{
			if (to == null) throw new ArgumentNullException(nameof(to));
			AdjacencyList.Add(to);
			to.AdjacencyList.Add((TNode)this);
		}

		/// <inheritdoc />
		public override bool Remove(TNode to)
		{
			if (to == null) throw new ArgumentNullException(nameof(to));
			AdjacencyList.Remove(to);
			to.AdjacencyList.Remove((TNode)this);
			return true;
		}

		/// <inheritdoc />
		public override void Clear()
		{
			TNode self = (TNode)this;

			foreach (TNode node in AdjacencyList) 
				node.AdjacencyList.Remove(self);

			AdjacencyList.Clear();
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class UndirectedGraphNode<T> : UndirectedGraphNodeBase<UndirectedGraphNode<T>, T>
	{
		public UndirectedGraphNode([NotNull] T value)
			: base(value)
		{
		}
	}
}