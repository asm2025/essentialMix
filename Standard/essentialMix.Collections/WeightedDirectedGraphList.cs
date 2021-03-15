using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using essentialMix.Exceptions.Collections;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para>Weighted Directed Graph</para>
	/// </summary>
	/// <inheritdoc/>
	[Serializable]
	public class WeightedDirectedGraphList<T, TWeight> : WeightedGraphList<T, TWeight>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		/// <inheritdoc />
		public WeightedDirectedGraphList()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		public WeightedDirectedGraphList(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		public WeightedDirectedGraphList(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		public WeightedDirectedGraphList(int capacity, IEqualityComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedDirectedGraphList(SerializationInfo info, StreamingContext context)
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
		}

		/// <inheritdoc />
		public override bool RemoveEdge(T from, T to)
		{
			return TryGetValue(from, out KeyedCollection<T, GraphEdge<T, TWeight>> fromEdges) && fromEdges != null && fromEdges.Remove(to);
		}

		/// <inheritdoc />
		public override void SetWeight(T from, T to, TWeight weight)
		{
			if (TryGetValue(from, out KeyedCollection<T, GraphEdge<T, TWeight>> fromEdges)
				&& fromEdges != null && fromEdges.TryGetValue(to, out GraphEdge<T, TWeight> fromEdge))
			{
				fromEdge.Weight = weight;
			}
		}

		public int InDegree([NotNull] T value)
		{
			int degree = 0;

			foreach (KeyValuePair<T, KeyedCollection<T, GraphEdge<T, TWeight>>> pair in this)
			{
				if (pair.Value != null && pair.Value.ContainsKey(value)) degree++;
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
			HashSet<T> visitingSet = new HashSet<T>(Comparer);
			HashSet<T> visitedSet = new HashSet<T>(Comparer);
			Stack<T> resultStack = new Stack<T>(Count);
			int graphVersion = _version;

			foreach (T vertex in Keys)
			{
				if (graphVersion != _version) throw new VersionChangedException();
				if (TopologicalSortLocal(vertex, visitingSet, visitedSet, resultStack, graphVersion, ref conflictVertex, ref conflictIsSet)) continue;
				// cycle detected
				throw new Exception($"Cycle detected [{string.Join(", ", visitingSet.Append(conflictVertex))}]");
			}

			return resultStack.Reverse();

			bool TopologicalSortLocal(T vertex, HashSet<T> visiting, HashSet<T> visited, Stack<T> result, int version, ref T conflict, ref bool conflictSet)
			{
				if (version != _version) throw new VersionChangedException();
				if (visited.Contains(vertex)) return true;
				visiting.Add(vertex);

				KeyedCollection<T, GraphEdge<T, TWeight>> edges = this[vertex];

				if (edges != null && edges.Count > 0)
				{
					foreach (GraphEdge<T, TWeight> edge in edges.Values.OrderBy(e => e.Weight))
					{
						if (visited.Contains(edge.To) || IsLoop(vertex, edge)) continue;

						if (visiting.Contains(edge.To) /* cycle detected */ || !TopologicalSortLocal(edge.To, visiting, visited, result, version, ref conflict, ref conflictSet))
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

				visiting.Remove(vertex);
				visited.Add(vertex);
				result.Push(vertex);
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
			HashSet<T> visitingSet = new HashSet<T>(Comparer);
			HashSet<T> visitedSet = new HashSet<T>(Comparer);
			int graphVersion = _version;

			foreach (T vertex in vertices)
			{
				if (graphVersion != _version) throw new VersionChangedException();
				if (!FindCycleLocal(vertex, visitingSet, visitedSet, graphVersion, ref conflictVertex, ref conflictIsSet)) continue;
				return visitingSet.Append(conflictVertex);
			}

			return null;

			bool FindCycleLocal(T vertex, HashSet<T> visiting, HashSet<T> visited, int version, ref T conflict, ref bool conflictSet)
			{
				if (version != _version) throw new VersionChangedException();
				if (visited.Contains(vertex)) return false;
				visiting.Add(vertex);

				KeyedCollection<T, GraphEdge<T, TWeight>> edges = this[vertex];

				if (edges != null && edges.Count > 0)
				{
					foreach (GraphEdge<T, TWeight> edge in edges.Values.OrderBy(e => e.Weight))
					{
						if (visited.Contains(edge.To) || ignoreLoop && IsLoop(vertex, edge)) continue;

						if (visiting.Contains(edge.To) /* cycle detected */ || FindCycleLocal(edge.To, visiting, visited, version, ref conflict, ref conflictSet))
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

				visiting.Remove(vertex);
				visited.Add(vertex);
				return false;
			}
		}
	}
}