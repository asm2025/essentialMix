using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using asm.Exceptions.Collections;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><see href="https://en.wikipedia.org/wiki/Mixed_graph">Weighted Mixed Graph</see></para>
	/// </summary>
	/// <inheritdoc/>
	[Serializable]
	public class WeightedMixedGraphList<T, TWeight> : WeightedGraphList<T, TWeight>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		/// <inheritdoc />
		public WeightedMixedGraphList() 
			: this(0, null)
		{
		}

		/// <inheritdoc />
		public WeightedMixedGraphList(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		public WeightedMixedGraphList(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		public WeightedMixedGraphList(int capacity, IEqualityComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedMixedGraphList(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc />
		public override void AddEdge(T from, T to) { AddEdge(from, to, default(TWeight), false); }
		/// <inheritdoc />
		public override void AddEdge(T from, T to, TWeight weight) { AddEdge(from, to, weight, false); }
		public void AddEdge([NotNull] T from, [NotNull] T to, bool undirected) { AddEdge(from, to, default(TWeight), undirected); }
		public void AddEdge([NotNull] T from, [NotNull] T to, TWeight weight, bool undirected)
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
			if (!undirected || Comparer.Equals(from, to)) return;

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
		public override bool RemoveEdge(T from, T to) { return RemoveEdge(from, to, false); }
		public bool RemoveEdge([NotNull] T from, [NotNull] T to, bool undirected)
		{
			bool removed = false;
			if (TryGetValue(from, out KeyedCollection<T, GraphEdge<T, TWeight>> fromEdges) && fromEdges != null) removed |= fromEdges.Remove(to);
			if (!undirected) return removed;
			if (TryGetValue(to, out KeyedCollection<T, GraphEdge<T, TWeight>> toEdges) & toEdges != null) removed |= toEdges.Remove(to);
			return removed;
		}

		/// <inheritdoc />
		public override void SetWeight(T from, T to, TWeight weight) { SetWeight(from, to, weight, false); }
		public void SetWeight([NotNull] T from, [NotNull] T to, TWeight weight, bool undirected)
		{
			if (TryGetValue(from, out KeyedCollection<T, GraphEdge<T, TWeight>> fromEdges)
				&& fromEdges != null && fromEdges.TryGetValue(to, out GraphEdge<T, TWeight> fromEdge))
			{
				fromEdge.Weight = weight;
			}

			if (undirected && TryGetValue(to, out KeyedCollection<T, GraphEdge<T, TWeight>> toEdges)
				&& toEdges != null && toEdges.TryGetValue(from, out GraphEdge<T, TWeight> toEdge))
			{
				toEdge.Weight = weight;
			}
		}

		public int InDegree([NotNull] T value)
		{
			int degree = 0;

			foreach (KeyValuePair<T, KeyedCollection<T, GraphEdge<T, TWeight>>> pair in this)
			{
				if (pair.Value == null) continue;

				foreach (GraphEdge<T, TWeight> edge in pair.Value)
				{
					if (!Comparer.Equals(value, edge.To)) continue;
					degree++;
				}
			}

			return degree;
		}

		[NotNull]
		public IEnumerable<T> TopologicalSort()
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
			if (Count == 0) return Enumerable.Empty<T>();

			// for detecting cycles
			bool conflictIsSet = false;
			T conflictVertex = default(T);
			HashSet<T> visitedSet = new HashSet<T>(Comparer);
			Stack<T> resultStack = new Stack<T>(Count);
			int graphVersion = _version;

			foreach (T vertex in Keys)
			{
				if (graphVersion != _version) throw new VersionChangedException();
				if (visitedSet.Contains(vertex) || TopologicalSortLocal(vertex, vertex, visitedSet, resultStack, graphVersion, ref conflictVertex, ref conflictIsSet)) continue;
				// cycle detected
				throw new Exception($"Cycle detected [{string.Join(", ", visitedSet.Append(conflictVertex))}]");
			}

			return resultStack;

			bool TopologicalSortLocal(T vertex, T parent, HashSet<T> visited, Stack<T> result, int version, ref T conflict, ref bool conflictSet)
			{
				if (version != _version) throw new VersionChangedException();
				if (visited.Contains(vertex)) return true;
				visited.Add(vertex);
				result.Push(vertex);

				if (TryGetValue(vertex, out KeyedCollection<T, GraphEdge<T, TWeight>> edges) && edges != null)
				{
					foreach (GraphEdge<T, TWeight> edge in edges.Values.OrderBy(e => e.Weight))
					{
						if (IsLoop(parent, edge) || IsLoop(vertex, edge)) continue;

						if (visited.Contains(edge.To) /* cycle detected */ || !TopologicalSortLocal(edge.To, vertex, visited, result, version, ref conflict, ref conflictSet))
						{
							if (!conflictSet)
							{
								conflictSet = true;
								conflict = edge.To;
							}

							return false;
						}
					}
				}

				return true;
			}
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