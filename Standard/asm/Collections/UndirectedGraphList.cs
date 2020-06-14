using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><see href="https://en.wikipedia.org/wiki/Graph_(discrete_mathematics)">Undirected Graph</see></para>
	/// </summary>
	/// <inheritdoc/>
	[Serializable]
	public abstract class UndirectedGraphList<TEdge, T> : GraphList<GraphVertex<T>, TEdge, T>
		where TEdge : GraphEdge<GraphVertex<T>, TEdge, T>
	{
		/// <inheritdoc />
		protected UndirectedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected UndirectedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected UndirectedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected UndirectedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public override void AddEdge(T from, T to)
		{
			if (!Vertices.ContainsKey(from)) throw new KeyNotFoundException(nameof(from) + " value is not found.");
			if (!Vertices.ContainsKey(to)) throw new KeyNotFoundException(nameof(to) + " value is not found.");

			if (!Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges))
			{
				fromEdges = NewEdgesContainer();
				Edges.Add(from, fromEdges);
			}

			fromEdges.Add(NewEdge(to));
			// short-circuit - loop edge
			if (Comparer.Equals(from, to)) return;

			if (!Edges.TryGetValue(to, out KeyedDictionary<T, TEdge> toEdges))
			{
				toEdges = NewEdgesContainer();
				Edges.Add(to, toEdges);
			}

			toEdges.Add(NewEdge(from));
		}

		/// <inheritdoc />
		public override void RemoveEdge(T from, T to)
		{
			if (Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges))
			{
				fromEdges.RemoveByKey(to);
				if (fromEdges.Count == 0) Edges.Remove(from);
			}

			if (Edges.TryGetValue(to, out KeyedDictionary<T, TEdge> toEdges))
			{
				toEdges.RemoveByKey(from);
				if (toEdges.Count == 0) Edges.Remove(to);
			}
		}

		/// <inheritdoc />
		public override int GetSize()
		{
			int sum = 0;

			foreach (KeyValuePair<T, KeyedDictionary<T, TEdge>> pair in Edges)
			{
				foreach (TEdge edge in pair.Value.Values)
				{
					if (IsLoop(pair.Key, edge)) sum += 2;
					else sum++;
				}
			}

			return sum / 2;
		}

		/// <inheritdoc />
		protected override GraphVertex<T> NewVertex([NotNull] T value)
		{
			return new GraphVertex<T>(value);
		}
	}


	/// <inheritdoc />
	[Serializable]
	public class UndirectedGraphList<T> : UndirectedGraphList<GraphEdge<T>, T>
	{
		/// <inheritdoc />
		public UndirectedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public UndirectedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public UndirectedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public UndirectedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		protected override GraphEdge<T> NewEdge([NotNull] T value)
		{
			if (!Vertices.TryGetValue(value, out GraphVertex<T> vertex)) throw new KeyNotFoundException();
			return new GraphEdge<T>(vertex);
		}
	}
}