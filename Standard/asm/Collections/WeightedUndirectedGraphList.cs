﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using asm.Exceptions.Collections;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para>Weighted Undirected Graph</para>
	/// </summary>
	/// <inheritdoc/>
	[Serializable]
	public class WeightedUndirectedGraphList<T, TWeight> : WeightedGraphList<T, TWeight>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		/// <inheritdoc />
		public WeightedUndirectedGraphList()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		public WeightedUndirectedGraphList(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		public WeightedUndirectedGraphList(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		public WeightedUndirectedGraphList(int capacity, IEqualityComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedUndirectedGraphList(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc />
		public override void AddEdge(T from, T to, TWeight weight)
		{
			if (!ContainsKey(from)) throw new KeyNotFoundException(nameof(from) + " value is not found.");
			if (!ContainsKey(to)) throw new KeyNotFoundException(nameof(to) + " value is not found.");

			KeyedCollection<T, GraphEdge<T, TWeight>> fromEdges = this[from];

			if (fromEdges == null)
			{
				fromEdges = NewEdgesContainer();
				this[from] = fromEdges;
			}

			if (fromEdges.TryGetValue(to, out GraphEdge<T, TWeight> fromEdge))
			{
				if (fromEdge.Weight.CompareTo(weight) > 0) fromEdge.Weight = weight;
			}
			else
			{
				fromEdge = new GraphEdge<T, TWeight>(to, weight);
				fromEdges.Add(fromEdge);
			}

			// short-circuit - loop edge
			if (Comparer.Equals(from, to)) return;

			KeyedCollection<T, GraphEdge<T, TWeight>> toEdges = this[to];

			if (toEdges == null)
			{
				toEdges = NewEdgesContainer();
				this[to] = toEdges;
			}

			if (toEdges.TryGetValue(from, out GraphEdge<T, TWeight> toEdge))
			{
				if (toEdge.Weight.CompareTo(weight) > 0) toEdge.Weight = weight;
			}
			else
			{
				toEdge = new GraphEdge<T, TWeight>(from, weight);
				toEdges.Add(toEdge);
			}
		}

		/// <inheritdoc />
		public override bool RemoveEdge(T from, T to)
		{
			bool removed = false;
			if (TryGetValue(from, out KeyedCollection<T, GraphEdge<T, TWeight>> fromEdges) && fromEdges != null) removed |= fromEdges.Remove(to);
			if (TryGetValue(to, out KeyedCollection<T, GraphEdge<T, TWeight>> toEdges) && toEdges != null) removed |= toEdges.Remove(from);
			return removed;
		}

		public override void SetWeight(T from, T to, TWeight weight)
		{
			if (TryGetValue(from, out KeyedCollection<T, GraphEdge<T, TWeight>> fromEdges)
				&& fromEdges != null && fromEdges.TryGetValue(to, out GraphEdge<T, TWeight> fromEdge))
			{
				fromEdge.Weight = weight;
			}

			if (TryGetValue(to, out KeyedCollection<T, GraphEdge<T, TWeight>> toEdges)
				&& toEdges != null && toEdges.TryGetValue(from, out GraphEdge<T, TWeight> toEdge))
			{
				toEdge.Weight = weight;
			}
		}

		/// <inheritdoc />
		public override int GetSize()
		{
			int sum = 0;

			foreach (KeyValuePair<T, KeyedCollection<T, GraphEdge<T, TWeight>>> pair in this)
			{
				if (pair.Value == null) continue;

				foreach (GraphEdge<T, TWeight> edge in pair.Value.Values)
				{
					sum++;
					if (IsLoop(pair.Key, edge)) sum++;
				}
			}

			return sum / 2;
		}

		public WeightedUndirectedGraphList<T, TWeight> GetMinimumSpanningTree(SpanningTreeAlgorithm algorithm)
		{
			return algorithm switch
			{
				SpanningTreeAlgorithm.Prim => PrimSpanningTree(),
				SpanningTreeAlgorithm.Kruskal => KruskalSpanningTree(),
				SpanningTreeAlgorithm.Boruvka => BoruvkaSpanningTree(),
				_ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
			};
		}

		/// <inheritdoc />
		protected override IEnumerable<T> FindCycle(ICollection<T> vertices, bool ignoreLoop = false)
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
			if (vertices.Count == 0) return null;
			
			bool conflictIsSet = false;
			T conflictVertex = default(T);
			HashSet<T> visitedSet = new HashSet<T>(Comparer);
			int graphVersion = _version;

			foreach (T vertex in vertices)
			{
				if (graphVersion != _version) throw new VersionChangedException();
				if (visitedSet.Contains(vertex) || !FindCycleLocal(vertex, vertex, visitedSet, graphVersion, ref conflictVertex, ref conflictIsSet)) continue;
				return visitedSet.Append(conflictVertex);
			}

			return null;

			bool FindCycleLocal(T vertex, T parent, HashSet<T> visited, int version, ref T conflict, ref bool conflictSet)
			{
				if (version != _version) throw new VersionChangedException();
				if (visited.Contains(vertex)) return false;
				visited.Add(vertex);

				KeyedCollection<T, GraphEdge<T, TWeight>> edges = this[vertex];

				if (edges != null && edges.Count > 0)
				{
					foreach (GraphEdge<T, TWeight> edge in edges.Values.OrderBy(e => e.Weight))
					{
						if (IsLoop(parent, edge) || ignoreLoop && IsLoop(vertex, edge)) continue;

						if (visited.Contains(edge.To) /* cycle detected */ || FindCycleLocal(edge.To, vertex, visited, version, ref conflict, ref conflictSet))
						{
							if (!conflictSet)
							{
								conflictSet = true;
								conflict = edge.To;
							}

							return true;
						}
					}
				}

				return false;
			}
		}

		private WeightedUndirectedGraphList<T, TWeight> PrimSpanningTree()
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
			/*
			 * The original implementation uses more storage than it should by duplicating the 'from' vertex in the edge object.
			 * It's only needed temporarily to keep track of the 'from' or there might be a better solution coming later. We still
			 * need to have a list of vertices because there might be some not-connected vertices in a forest.
			 * Eliminating the duplicated entries is required to keep the graph as lean as possible.
			 */
			if (Count == 0) return null;
			// find the first connected vertex
			if (!GetFirstConnectedVertex(out T vertex, out KeyedCollection<T, GraphEdge<T, TWeight>> edges)) return null;

			WeightedUndirectedGraphList<T, TWeight> result = new WeightedUndirectedGraphList<T, TWeight>(Comparer);
			PriorityQueue<TWeight, (T From, GraphEdge<T, TWeight> Edge)> queue = new MinPriorityQueue<TWeight, (T From, GraphEdge<T, TWeight> Edge)>(e => e.Edge.Weight);
			result.Add(vertex);

			// add the edges to the queue
			foreach (GraphEdge<T, TWeight> edge in edges) 
				queue.Add((vertex, edge));

			while (result.Count < Keys.Count && queue.Count > 0)
			{
				(T From, GraphEdge<T, TWeight> Edge) tuple = queue.Remove();
				if (result.ContainsKey(tuple.Edge.To)) continue;
				result.Add(tuple.Edge.To);
				result.AddEdge(tuple.From, tuple.Edge.To, tuple.Edge.Weight);
				edges = this[tuple.Edge.To];
				if (edges == null || edges.Count == 0) continue;

				foreach (GraphEdge<T, TWeight> edge in edges) 
					queue.Add((tuple.Edge.To, edge));
			}

			return result;
		}

		private WeightedUndirectedGraphList<T, TWeight> KruskalSpanningTree()
		{
			//if (Count == 0) return Enumerable.Empty<T>();

			//PriorityQueue<TWeight, GraphEdge<T, TWeight>> queue = new MinPriorityQueue<TWeight, GraphEdge<T, TWeight>>(e => e.Weight);
			//T vertex = Keys.First();
			//queue.Add();
			return null;
		}

		private WeightedUndirectedGraphList<T, TWeight> BoruvkaSpanningTree()
		{
			//if (Count == 0) return Enumerable.Empty<T>();

			//PriorityQueue<TWeight, GraphEdge<T, TWeight>> queue = new MinPriorityQueue<TWeight, GraphEdge<T, TWeight>>(e => e.Weight);
			//T vertex = Keys.First();
			//queue.Add();
			return null;
		}
	}
}