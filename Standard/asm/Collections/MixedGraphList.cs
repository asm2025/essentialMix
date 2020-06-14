﻿using System;
using System.Collections.Generic;
using System.Linq;
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
	public abstract class MixedGraphList<TEdge, T> : GraphList<GraphVertex<T>, TEdge, T>
		where TEdge : GraphEdge<GraphVertex<T>, TEdge, T>
	{
		/// <inheritdoc />
		protected MixedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected MixedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected MixedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected MixedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public override void AddEdge(T from, T to) { AddEdge(from, to, false); }
		public virtual void AddEdge([NotNull] T from, [NotNull] T to, bool undirected)
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
			if (!undirected || Comparer.Equals(from, to)) return;

			if (!Edges.TryGetValue(to, out KeyedDictionary<T, TEdge> toEdges))
			{
				toEdges = NewEdgesContainer();
				Edges.Add(to, toEdges);
			}
			else if (toEdges.ContainsKey(from))
			{
				return;
			}

			toEdges.Add(NewEdge(from));
		}

		/// <inheritdoc />
		public override void RemoveEdge(T from, T to) { RemoveEdge(from, to, false); }
		public void RemoveEdge([NotNull] T from, [NotNull] T to, bool undirected)
		{
			if (Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges))
			{
				fromEdges.RemoveByKey(to);
				if (fromEdges.Count == 0) Edges.Remove(from);
			}

			if (!undirected) return;

			if (Edges.TryGetValue(to, out KeyedDictionary<T, TEdge> toEdges))
			{
				toEdges.RemoveByKey(to);
				if (toEdges.Count == 0) Edges.Remove(to);
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
				}
			}

			return degree;
		}

		[NotNull]
		public IEnumerable<T> TopologicalSort()
		{
			if (Count == 0) return Enumerable.Empty<T>();

			int version = _version;
			Stack<T> result = new Stack<T>(Count);
			Stack<T> stack = new Stack<T>();
			// for detecting cycles if any
			LinkedList<T> cycle = new LinkedList<T>();
			HashSet<T> visiting = new HashSet<T>(Comparer);
			// keep track of visited vertices
			HashSet<T> visited = new HashSet<T>(Comparer);

			foreach (T key in Edges.Keys)
			{
				if (version != _version) throw new VersionChangedException();
				if (visited.Contains(key)) continue;
				int deque = 0;
				stack.Push(key);

				while (stack.Count > 0)
				{
					if (version != _version) throw new VersionChangedException();

					T value = stack.Pop();
					if (visited.Contains(value)) continue;
					visiting.Add(value);
					deque++;
					cycle.AddLast(value);
					result.Push(value);

					if (Edges.TryGetValue(value, out KeyedDictionary<T, TEdge> edges))
					{
						foreach (TEdge edge in edges)
						{
							if (visited.Contains(edge.To.Value) || IsLoop(value, edge)) continue;

							// cycle detected
							if (visiting.Contains(edge.To.Value))
							{
								cycle.AddLast(edge.To.Value);

								while (!Comparer.Equals(edge.To.Value, cycle.First.Value))
									cycle.RemoveFirst();

								throw new Exception($"Cycle detected [{string.Join(", ", cycle)}]");
							}

							stack.Push(edge.To.Value);
						}

						continue;
					}

					while (deque-- > 0)
					{
						T d = cycle.Last.Value;
						cycle.RemoveLast();
						visiting.Remove(d);
						visited.Add(d);
					}
				}
			}

			return result;
		}

		/// <inheritdoc />
		protected override GraphVertex<T> NewVertex([NotNull] T value)
		{
			return new GraphVertex<T>(value);
		}
	}


	/// <inheritdoc />
	[Serializable]
	public class MixedGraphList<T> : MixedGraphList<GraphEdge<T>, T>
	{
		/// <inheritdoc />
		public MixedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public MixedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public MixedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public MixedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
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