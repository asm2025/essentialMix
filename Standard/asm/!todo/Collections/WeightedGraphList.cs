using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><see href="https://en.wikipedia.org/wiki/Graph_(discrete_mathematics)">Weighted Graph</see></para>
	/// </summary>
	/// <inheritdoc/>
	/// <typeparam name="TEdge"><inheritdoc/></typeparam>
	/// <typeparam name="TWeight">The weight of the edge.</typeparam>
	/// <typeparam name="T"><inheritdoc/></typeparam>
	[Serializable]
	public abstract class WeightedGraphList<TEdge, TWeight, T> : GraphList<GraphVertex<T>, TEdge, T>
		where TEdge : GraphWeightedEdge<GraphVertex<T>, TEdge, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		[DebuggerDisplay("{Value}")]
		private struct PathEntry
		{
			public PathEntry([NotNull] T value, TWeight priority)
			{
				Value = value;
				Priority = priority;
			}

			[NotNull]
			public readonly T Value;
			public readonly TWeight Priority;
		}

		/// <inheritdoc />
		protected WeightedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected WeightedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected WeightedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public override void AddEdge(T from, T to) { AddEdge(from, to, default(TWeight)); }

		public abstract void AddEdge([NotNull] T from, [NotNull] T to, TWeight weight);

		public abstract void SetWeight([NotNull] T from, [NotNull] T to, TWeight weight);

		[NotNull]
		public IEnumerable<T> GetShortestPath([NotNull] T from, [NotNull] T to, ShortestPathAlgorithm algorithm)
		{
			switch (algorithm)
			{
				case ShortestPathAlgorithm.Dijkstra:
					return DijkstraShortestPath(from, to);
				default:
					throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
			}
		}

		[NotNull]
		private IEnumerable<T> DijkstraShortestPath([NotNull] T from, [NotNull] T to)
		{
			// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
			if (!Vertices.ContainsKey(from)) throw new KeyNotFoundException(nameof(from) + " value is not found.");
			if (!Vertices.ContainsKey(to)) throw new KeyNotFoundException(nameof(to) + " value is not found.");

			Dictionary<T, T> history = new Dictionary<T, T>();
			Dictionary<T, TWeight> weights = new Dictionary<T, TWeight>(Comparer);
			PriorityQueue<TWeight, PathEntry> queue = new MinPriorityQueue<TWeight, PathEntry>(e => e.Priority);
			HashSet<T> visited = new HashSet<T>(Comparer);
			TWeight maxWeight = TypeHelper.MaximumOf<TWeight>();

			foreach (GraphVertex<T> vertex in Vertices) 
				weights.Add(vertex.Value, maxWeight);

			weights[from] = default(TWeight);
			queue.Add(new PathEntry(from, default(TWeight)));

			while (queue.Count > 0)
			{
				T current = queue.Remove().Value;
				visited.Add(current);
				if (!Edges.TryGetValue(current, out KeyedDictionary<T, TEdge> edges)) continue;

				foreach (TEdge edge in edges)
				{
					if (visited.Contains(edge.To)) continue;

					TWeight weight = weights[current].Add(edge.Weight);
					if (weight.CompareTo(weights[edge.To.Value]) >= 0) continue;
					weights[edge.To.Value] = weight;
					history[edge.To.Value] = Vertices[current];
					queue.Add(new PathEntry(edge.To, weight));
				}
			}

			if (!visited.Contains(to)) return Enumerable.Empty<T>();

			Stack<T> stack = new Stack<T>();

			while (history.TryGetValue(to, out T previous))
			{
				stack.Push(to);
				history.Remove(to);
				to = previous;
			}

			stack.Push(to);
			return stack;
		}
	}
}