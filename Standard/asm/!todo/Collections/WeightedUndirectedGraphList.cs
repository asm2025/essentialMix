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

			if (!TryGetValue(from, out KeyedCollection<T, GraphEdge<T, TWeight>> fromEdges) || fromEdges == null)
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

			if (!TryGetValue(to, out KeyedCollection<T, GraphEdge<T, TWeight>> toEdges) || toEdges == null)
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

				if (TryGetValue(vertex, out KeyedCollection<T, GraphEdge<T, TWeight>> edges) && edges != null)
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
	}
}