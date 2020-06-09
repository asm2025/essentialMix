using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class UndirectedGraph<T> : Graph<GraphNode<T>, GraphEdge<T>, T>
	{
		/// <inheritdoc />
		public UndirectedGraph()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public UndirectedGraph(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public UndirectedGraph([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public UndirectedGraph([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public sealed override int Size => Edges.Count / 2;

		/// <inheritdoc />
		public override void AddEdge([NotNull] T from, [NotNull] T to)
		{
			if (!Nodes.ContainsKey(from)) throw new KeyNotFoundException(nameof(from) + " value is not found.");
			if (!Nodes.ContainsKey(to)) throw new KeyNotFoundException(nameof(to) + " value is not found.");

			if (!Edges.TryGetValue(from, out KeyedDictionary<T, GraphEdge<T>> fromEdges))
			{
				fromEdges = NewEdges();
				Edges.Add(from, fromEdges);
			}

			fromEdges.Add(NewEdge(to));

			if (!Edges.TryGetValue(to, out KeyedDictionary<T, GraphEdge<T>> toEdges))
			{
				toEdges = NewEdges();
				Edges.Add(to, toEdges);
			}

			toEdges.Add(NewEdge(from));
		}

		/// <inheritdoc />
		public override void RemoveEdge(T from, T to)
		{
			if (Edges.TryGetValue(from, out KeyedDictionary<T, GraphEdge<T>> fromEdges))
			{
				fromEdges.RemoveByKey(to);
				if (fromEdges.Count == 0) Edges.Remove(from);
			}

			if (Edges.TryGetValue(to, out KeyedDictionary<T, GraphEdge<T>> toEdges))
			{
				toEdges.RemoveByKey(from);
				if (toEdges.Count == 0) Edges.Remove(to);
			}
		}

		/// <inheritdoc />
		public override void RemoveEdges(T from)
		{
			Edges.Remove(from);
		}

		/// <inheritdoc />
		public override void RemoveAllEdges(T value)
		{
			Edges.Remove(value);
			if (Edges.Count == 0) return;

			List<T> empty = new List<T>();

			foreach (KeyValuePair<T, KeyedDictionary<T, GraphEdge<T>>> pair in Edges)
			{
				pair.Value.RemoveByKey(value);
				if (pair.Value.Count == 0) empty.Add(pair.Key);
			}

			if (empty.Count == 0) return;

			foreach (T key in empty)
			{
				Edges.Remove(key);
			}
		}

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