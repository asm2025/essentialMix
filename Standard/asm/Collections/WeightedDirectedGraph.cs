using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class WeightedDirectedGraph<T> : WeightedGraph<GraphNode<T>, GraphWeightedEdge<T>, T>
	{
		/// <inheritdoc />
		protected WeightedDirectedGraph()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected WeightedDirectedGraph(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedDirectedGraph([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected WeightedDirectedGraph([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public sealed override int Size => Edges.Count;

		/// <inheritdoc />
		public override void AddEdge(T from, T to) {  }

		/// <inheritdoc />
		public override void RemoveEdge(T from, T to) {  }

		/// <inheritdoc />
		public override void RemoveEdges(T from) {  }

		/// <inheritdoc />
		public override void RemoveAllEdges(T value) {  }

		/// <inheritdoc />
		protected override GraphNode<T> NewNode([NotNull] T value) { return new GraphNode<T>(value); }

		/// <inheritdoc />
		protected override GraphWeightedEdge<T> NewEdge([NotNull] T value)
		{
			if (!Nodes.TryGetValue(value, out GraphNode<T> node)) throw new KeyNotFoundException();
			return new GraphWeightedEdge<T>(node);
		}
	}
}