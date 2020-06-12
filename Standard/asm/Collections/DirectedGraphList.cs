using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><see href="https://en.wikipedia.org/wiki/Directed_graph">Directed Graph</see></para>
	/// </summary>
	/// <inheritdoc/>
	[Serializable]
	public abstract class DirectedGraphList<TEdge, T> : GraphList<GraphVertex<T>, TEdge, T>
		where TEdge : GraphEdge<GraphVertex<T>, TEdge, T>
	{
		/// <inheritdoc />
		protected DirectedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected DirectedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected DirectedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected DirectedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public override void AddEdge([NotNull] T from, [NotNull] T to)
		{
			if (!Vertices.ContainsKey(from)) throw new KeyNotFoundException(nameof(from) + " value is not found.");
			if (!Vertices.ContainsKey(to)) throw new KeyNotFoundException(nameof(to) + " value is not found.");

			if (!Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges))
			{
				fromEdges = NewEdgesContainer();
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
		public override void RemoveEdges(T value)
		{
			Edges.Remove(value);
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

		public int InDegree([NotNull] T value)
		{
			int degree = 0;

			foreach (KeyValuePair<T, KeyedDictionary<T, TEdge>> pair in Edges)
			{
				foreach (TEdge edge in pair.Value)
				{
					if (!Comparer.Equals(value, edge.To.Value)) continue;
					degree++;
					// loop edge
					if (Comparer.Equals(pair.Key, edge.To.Value)) degree++;
				}
			}

			return degree;
		}

		public int OutDegree([NotNull] T value)
		{
			if (!Edges.TryGetValue(value, out KeyedDictionary<T, TEdge> edges)) return 0;
			int degree = edges.Count;
			// if edges has a loop edge (edge to the same vertex), add 1.
			if (edges.ContainsKey(value)) degree++;
			return degree;
		}

		// todo
		[NotNull]
		public IEnumerable<T> TopologicalSort()
		{
			return Enumerable.Empty<T>();
		}

		protected override GraphVertex<T> NewVertex([NotNull] T value)
		{
			return new GraphVertex<T>(value);
		}
	}
	
	/// <inheritdoc/>
	[Serializable]
	public class DirectedGraphList<T> : DirectedGraphList<GraphEdge<T>, T>
	{
		/// <inheritdoc />
		public DirectedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public DirectedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public DirectedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public DirectedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		protected override GraphEdge<T> NewEdge([NotNull] T value)
		{
			if (!Vertices.TryGetValue(value, out GraphVertex<T> vertex)) throw new KeyNotFoundException();
			return new GraphEdge<T>(vertex);
		}
	}
}