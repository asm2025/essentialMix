using System;
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
				fromEdges = MakeContainer();
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
				toEdges = MakeContainer();
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
			if (Count == 0) return null;
			return algorithm switch
			{
				SpanningTreeAlgorithm.Prim => PrimSpanningTree(),
				SpanningTreeAlgorithm.Kruskal => KruskalSpanningTree(),
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
			/*
			 * Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
			 * The implementation in the course uses more storage than it should by duplicating the 'from' vertex in the edge object.
			 * It's only needed temporarily to keep track of the 'from' or there might be a better solution coming later. We still
			 * need to have a list of vertices because there might be some not-connected vertices in a forest.
			 * Eliminating the duplicated entries is required to keep the graph as lean as possible.
			 *
			 * Udemy - Mastering Data Structures & Algorithms using C and C++ - Abdul Bari
			 * Correction to 'Code With Mosh': We should select the first minimum edge not the first connected vertex
			 *
			 * Amazing how one course is just not enough to implement the algorithm properly!
			 *
			 * OK, this is better
			 * http://keithschwarz.com/interesting/code/?dir=fibonacci-heap
			 */
			if (Count == 0) return null;

			BinomialHeap<TWeight, EdgeEntry<T, GraphEdge<T, TWeight>>> queue = new MinBinomialHeap<TWeight, EdgeEntry<T, GraphEdge<T, TWeight>>>(e => e.Edge.Weight);
			Dictionary<T, BinomialNode<TWeight, EdgeEntry<T, GraphEdge<T, TWeight>>>> entries = new Dictionary<T, BinomialNode<TWeight, EdgeEntry<T, GraphEdge<T, TWeight>>>>(Comparer);
			WeightedUndirectedGraphList<T, TWeight> result = new WeightedUndirectedGraphList<T, TWeight>(Comparer);

			T minFrom = default(T);
			GraphEdge<T, TWeight> minEdge = null;
			KeyedCollection<T, GraphEdge<T, TWeight>> minConnectedEdges = null;

			// find the first minimum weight edge
			foreach (T vertex in Keys)
			{
				KeyedCollection<T, GraphEdge<T, TWeight>> edges = this[vertex];
				if (edges == null || edges.Count == 0) continue;
			
				foreach (GraphEdge<T, TWeight> edge in edges)
				{
					if (minEdge != null && minEdge.Weight.CompareTo(edge.Weight) < 1) continue;
					minFrom = vertex;
					minEdge = edge;
					minConnectedEdges = edges;
				}
			}

			if (minEdge == null) return null;
			// add the starting node
			result.Add(minFrom);
			AddOutgoingEdges(minFrom, minConnectedEdges, queue, result, entries);

			while (result.Count < Keys.Count && queue.Count > 0)
			{
				BinomialNode<TWeight, EdgeEntry<T, GraphEdge<T, TWeight>>> edgeEntry = queue.ExtractValue();

				if (!result.ContainsKey(edgeEntry.Value.Edge.To))
				{
					// this avoids to have a cycle
					result.Add(edgeEntry.Value.Edge.To);
					result.AddEdge(edgeEntry.Value.From, edgeEntry.Value.Edge.To, edgeEntry.Key);
				}

				// doing it this way guarantees the vertices are always connected
				KeyedCollection<T, GraphEdge<T, TWeight>> edges = this[edgeEntry.Value.Edge.To];
				if (edges == null || edges.Count == 0) continue;
				AddOutgoingEdges(edgeEntry.Value.Edge.To, edges, queue, result, entries);
			}

			return result;

			static void AddOutgoingEdges(T from, KeyedCollection<T, GraphEdge<T, TWeight>> edges
				, BinomialHeap<TWeight, EdgeEntry<T, GraphEdge<T, TWeight>>> queue
				, WeightedUndirectedGraphList<T, TWeight> result
				, Dictionary<T, BinomialNode<TWeight, EdgeEntry<T, GraphEdge<T, TWeight>>>> entries)
			{
				// Start off by scanning over all edges emanating from our node.
				foreach (GraphEdge<T, TWeight> edge in edges)
				{
					/*
					 * There are four possibilities for this edge:
					 * 1. This endpoint has already been added to the graph.
					 * 2. This endpoint is not in the graph and has never been in the queue.
					 * 3. This endpoint is in the graph, but this is a better edge.
					 * 4. This endpoint is in the graph, but there is a better edge to it.
					 */
					if (result.ContainsKey(edge.To)) continue; // case 1

					if (!entries.ContainsKey(edge.To))
					{
						// case 2
						BinomialNode<TWeight, EdgeEntry<T, GraphEdge<T, TWeight>>> node = queue.MakeNode(new EdgeEntry<T, GraphEdge<T, TWeight>>(from, edge));
						queue.Add(node);
						entries.Add(edge.To, node);
					}
					else if (entries[edge.To].Key.CompareTo(edge.Weight) > 0)
					{
						// case 3
						queue.DecreaseKey(entries[edge.To], edge.Weight);
					}

					// case 4: do nothing
				}
			}
		}

		private WeightedUndirectedGraphList<T, TWeight> KruskalSpanningTree()
		{
			// Udemy - Mastering Data Structures & Algorithms using C and C++ - Abdul Bari
			// https://www.geeksforgeeks.org/kruskals-minimum-spanning-tree-algorithm-greedy-algo-2/
			if (Count == 0) return null;

			BinomialHeap<TWeight, EdgeEntry<T, GraphEdge<T, TWeight>>> queue = new MinBinomialHeap<TWeight, EdgeEntry<T, GraphEdge<T, TWeight>>>(e => e.Edge.Weight);

			// add all the edges to the queue
			foreach (T vertex in Keys)
			{
				KeyedCollection<T, GraphEdge<T, TWeight>> edges = this[vertex];
				if (edges == null || edges.Count == 0) continue;

				foreach (GraphEdge<T, TWeight> edge in edges)
					queue.Add(new EdgeEntry<T, GraphEdge<T, TWeight>>(vertex, edge));
			}

			if (queue.Count == 0) return null;

			DisjointSet<T> disjointSet = new DisjointSet<T>(Keys, Comparer);
			WeightedUndirectedGraphList<T, TWeight> result = new WeightedUndirectedGraphList<T, TWeight>(Comparer);

			while (result.Count < Keys.Count && queue.Count > 0)
			{
				BinomialNode<TWeight, EdgeEntry<T, GraphEdge<T, TWeight>>> edgeEntry = queue.ExtractValue();
				if (disjointSet.IsConnected(edgeEntry.Value.From, edgeEntry.Value.Edge.To)) continue;
				disjointSet.Union(edgeEntry.Value.From, edgeEntry.Value.Edge.To);
				if (!result.ContainsKey(edgeEntry.Value.From)) result.Add(edgeEntry.Value.From);
				if (!result.ContainsKey(edgeEntry.Value.Edge.To)) result.Add(edgeEntry.Value.Edge.To);
				result.AddEdge(edgeEntry.Value.From, edgeEntry.Value.Edge.To, edgeEntry.Key);
			}

			return result;
		}
	}
}