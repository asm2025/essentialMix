using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class WeightedUndirectedGraph<T> : WeightedGraph<GraphNode<T>, GraphWeightedEdge<T>, T>
	{
		/// <inheritdoc />
		protected WeightedUndirectedGraph()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected WeightedUndirectedGraph(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedUndirectedGraph([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected WeightedUndirectedGraph([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public sealed override int Size => Edges.Count / 2;

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