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
	/// <para><see href="https://en.wikipedia.org/wiki/Directed_graph">Directed Graph</see></para>
	/// </summary>
	/// <inheritdoc/>
	[Serializable]
	public class DirectedGraphList<T> : GraphList<T>
	{
		/// <inheritdoc />
		public DirectedGraphList()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		public DirectedGraphList(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		public DirectedGraphList(IEqualityComparer<T> comparer)
			: this(0, comparer)
		{
		}

		/// <inheritdoc />
		public DirectedGraphList(int capacity, IEqualityComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		/// <inheritdoc />
		protected DirectedGraphList(SerializationInfo info, StreamingContext context)
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
		}

		/// <inheritdoc />
		public override bool RemoveEdge(T from, T to) { return TryGetValue(from, out HashSet<T> fromEdges) && fromEdges != null && fromEdges.Remove(to); }

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

				HashSet<T> edges = this[vertex];

				if (edges != null && edges.Count > 0)
				{
					foreach (T edge in edges)
					{
						if (visited.Contains(edge) || IsLoop(vertex, edge)) continue;

						if (visiting.Contains(edge) /* cycle detected */ || !TopologicalSortLocal(edge, visiting, visited, result, version, ref conflict, ref conflictSet))
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

				HashSet<T> edges = this[vertex];

				if (edges != null && edges.Count > 0)
				{
					foreach (T edge in edges)
					{
						if (visited.Contains(edge) || ignoreLoop && IsLoop(vertex, edge)) continue;

						if (visiting.Contains(edge) /* cycle detected */ || FindCycleLocal(edge, visiting, visited, version, ref conflict, ref conflictSet))
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

				visiting.Remove(vertex);
				visited.Add(vertex);
				return false;
			}
		}
	}
}