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
	/// <para><see href="https://en.wikipedia.org/wiki/Mixed_graph">Mixed Graph</see></para>
	/// </summary>
	/// <inheritdoc/>
	[Serializable]
	public class MixedGraphList<T> : GraphList<T>
	{
		/// <inheritdoc />
		public MixedGraphList()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		public MixedGraphList(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		public MixedGraphList(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		public MixedGraphList(int capacity, IEqualityComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		protected MixedGraphList(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc />
		public override void AddEdge(T from, T to) { AddEdge(from, to, false); }
		public virtual void AddEdge([NotNull] T from, [NotNull] T to, bool undirected)
		{
			if (!ContainsKey(from)) throw new KeyNotFoundException(nameof(from) + " value is not found.");
			if (!ContainsKey(to)) throw new KeyNotFoundException(nameof(to) + " value is not found.");

			HashSet<T> fromEdges = this[from];

			if (fromEdges == null)
			{
				fromEdges = NewEdgesContainer();
				this[from] = fromEdges;
			}

			fromEdges.Add(to);
			// short-circuit - loop edge
			if (!undirected || Comparer.Equals(from, to)) return;

			HashSet<T> toEdges = this[to];

			if (toEdges == null)
			{
				toEdges = NewEdgesContainer();
				this[to] = toEdges;
			}

			toEdges.Add(from);
		}

		/// <inheritdoc />
		public override bool RemoveEdge(T from, T to) { return RemoveEdge(from, to, false); }
		public bool RemoveEdge([NotNull] T from, [NotNull] T to, bool undirected)
		{
			bool removed = false;
			if (TryGetValue(from, out HashSet<T> fromEdges) && fromEdges != null) removed |= fromEdges.Remove(to);
			if (!undirected) return removed;
			if (TryGetValue(to, out HashSet<T> toEdges) && toEdges != null) removed |= toEdges.Remove(to);
			return removed;
		}

		public int InDegree([NotNull] T value)
		{
			int degree = 0;

			foreach (KeyValuePair<T, HashSet<T>> pair in this)
			{
				if (pair.Value != null && pair.Value.Contains(value)) degree++;
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

				HashSet<T> edges = this[vertex];

				if (edges != null && edges.Count > 0)
				{
					foreach (T edge in edges)
					{
						if (IsLoop(parent, edge) || IsLoop(vertex, edge)) continue;

						if (visited.Contains(edge) /* cycle detected */ || !TopologicalSortLocal(edge, vertex, visited, result, version, ref conflict, ref conflictSet))
						{
							if (!conflictSet)
							{
								conflictSet = true;
								conflict = edge;
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