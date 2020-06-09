using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public abstract class WeightedGraph<TNode, TEdge, T> : Graph<TNode, TEdge, T>
		where TNode : GraphNode<TNode, T>
		where TEdge : GraphWeightedEdge<TNode, TEdge, T>
	{
		/// <inheritdoc />
		protected WeightedGraph()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected WeightedGraph(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedGraph([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected WeightedGraph([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}
	}
}