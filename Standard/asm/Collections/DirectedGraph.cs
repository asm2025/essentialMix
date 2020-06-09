using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	public class DirectedGraph<T> : Graph<GraphNode<T>, GraphEdge<T>, T>
	{
		/// <inheritdoc />
		public DirectedGraph()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public DirectedGraph(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public DirectedGraph([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public DirectedGraph([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
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

		protected override GraphNode<T> NewNode([NotNull] T value)
		{
			return new GraphNode<T>(value);
		}

		protected override GraphEdge<T> NewEdge([NotNull] T value)
		{
			if (!Nodes.TryGetValue(value, out GraphNode<T> node)) throw new KeyNotFoundException();
			return new GraphEdge<T>(node);
		}
	}
}