using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Collections;

namespace essentialMix.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><see href="https://en.wikipedia.org/wiki/Graph_(discrete_mathematics)">Undirected Graph</see></para>
	/// </summary>
	/// <inheritdoc/>
	[Serializable]
	public class UndirectedGraphList<T> : GraphList<T>
	{
		/// <inheritdoc />
		public UndirectedGraphList() 
			: this(0, null)
		{
		}

		/// <inheritdoc />
		public UndirectedGraphList(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		public UndirectedGraphList(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		public UndirectedGraphList(int capacity, IEqualityComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		protected UndirectedGraphList(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc />
		public override void AddEdge(T from, T to)
		{
			if (!ContainsKey(from)) throw new KeyNotFoundException(nameof(from) + " value is not found.");
			if (!ContainsKey(to)) throw new KeyNotFoundException(nameof(to) + " value is not found.");

			HashSet<T> fromEdges = this[from];

			if (fromEdges == null)
			{
				fromEdges = MakeContainer();
				this[from] = fromEdges;
			}

			fromEdges.Add(to);
			// short-circuit - loop edge
			if (Comparer.Equals(from, to)) return;

			HashSet<T> toEdges = this[to];

			if (toEdges == null)
			{
				toEdges = MakeContainer();
				this[to] = toEdges;
			}

			toEdges.Add(from);
		}

		/// <inheritdoc />
		public override bool RemoveEdge(T from, T to)
		{
			bool removed = false;
			if (TryGetValue(from, out HashSet<T> fromEdges) && fromEdges != null) removed |= fromEdges.Remove(to);
			if (TryGetValue(to, out HashSet<T> toEdges) && toEdges != null) removed |= toEdges.Remove(from);
			return removed;
		}

		/// <inheritdoc />
		public override int GetSize()
		{
			int degree = 0;

			foreach (KeyValuePair<T, HashSet<T>> pair in this)
			{
				if (pair.Value == null) continue;

				foreach (T edge in pair.Value)
				{
					degree++;
					if (IsLoop(pair.Key, edge)) degree++;
				}
			}

			return degree / 2;
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

				HashSet<T> edges = this[vertex];

				if (edges != null && edges.Count > 0)
				{
					foreach (T edge in edges)
					{
						if (IsLoop(parent, edge) || ignoreLoop && IsLoop(vertex, edge)) continue;

						if (visited.Contains(edge) /* cycle detected */ || FindCycleLocal(edge, vertex, visited, version, ref conflict, ref conflictSet))
						{
							if (!conflictSet)
							{
								conflictSet = true;
								conflict = edge;
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