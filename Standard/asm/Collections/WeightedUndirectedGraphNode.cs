using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class WeightedUndirectedGraphNodeBase<TNode, T> : WeightedGraphNode<TNode, T>
		where TNode : WeightedUndirectedGraphNodeBase<TNode, T>
	{
		protected WeightedUndirectedGraphNodeBase([NotNull] T value)
			: base(value)
		{
		}

		/// <inheritdoc />
		public override void Add(TNode to, int weight)
		{
			if (to == null) throw new ArgumentNullException(nameof(to));
			AdjacencyList[to] = weight;
			to.AdjacencyList[(TNode)this] = weight;
		}

		/// <inheritdoc />
		public override bool Remove(TNode to)
		{
			if (to == null) throw new ArgumentNullException(nameof(to));
			if (AdjacencyList.ContainsKey(to)) AdjacencyList.Remove(to);
			if (AdjacencyList.ContainsKey((TNode)this)) to.AdjacencyList.Remove((TNode)this);
			return true;
		}

		/// <inheritdoc />
		public override void Clear()
		{
			TNode self = (TNode)this;

			foreach (TNode node in AdjacencyList.Keys) 
				node.AdjacencyList.Remove(self);

			AdjacencyList.Clear();
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class WeightedUndirectedGraphNode<T> : WeightedUndirectedGraphNodeBase<WeightedUndirectedGraphNode<T>, T>
	{
		public WeightedUndirectedGraphNode([NotNull] T value)
			: base(value)
		{
		}
	}
}