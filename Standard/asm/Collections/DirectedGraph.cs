using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><see href="https://en.wikipedia.org/wiki/Directed_graph">Directed Graph</see></para>
	/// </summary>
	/// <inheritdoc/>
	[Serializable]
	public abstract class DirectedGraph<TEdge, T> : Graph<GraphNode<T>, TEdge, T>
		where TEdge : GraphEdge<GraphNode<T>, TEdge, T>
	{
		/// <inheritdoc />
		protected DirectedGraph()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected DirectedGraph(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected DirectedGraph([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected DirectedGraph([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public override void AddEdge([NotNull] T from, [NotNull] T to)
		{
			if (!Nodes.ContainsKey(from)) throw new KeyNotFoundException(nameof(from) + " value is not found.");
			if (!Nodes.ContainsKey(to)) throw new KeyNotFoundException(nameof(to) + " value is not found.");

			if (!Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges))
			{
				fromEdges = NewEdges();
				Edges.Add(from, fromEdges);
			}

			fromEdges.Add(NewEdge(to));
		}

		/// <inheritdoc />
		public override void RemoveEdge(T from, T to)
		{
			if (!Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges)) return;
			fromEdges.RemoveByKey(to);
			if (fromEdges.Count == 0) Edges.Remove(from);
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

			foreach (KeyValuePair<T, KeyedDictionary<T, TEdge>> pair in Edges)
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
	}
	
	/// <inheritdoc/>
	[Serializable]
	public class DirectedGraph<T> : DirectedGraph<GraphEdge<T>, T>
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

		protected override GraphEdge<T> NewEdge([NotNull] T value)
		{
			if (!Nodes.TryGetValue(value, out GraphNode<T> node)) throw new KeyNotFoundException();
			return new GraphEdge<T>(node);
		}
	}
}