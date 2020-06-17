using System;
using System.Collections.Generic;
using System.Linq;
using asm.Exceptions.Collections;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para>Weighted Directed Graph</para>
	/// </summary>
	/// <inheritdoc/>
	[Serializable]
	public abstract class WeightedDirectedGraphList<TEdge, TWeight, T> : WeightedGraphList<TEdge, TWeight, T>
		where TEdge : GraphWeightedEdge<GraphVertex<T>, TEdge, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		/// <inheritdoc />
		protected WeightedDirectedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected WeightedDirectedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedDirectedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected WeightedDirectedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public override void AddEdge(T from, T to, TWeight weight)
		{
			if (!Vertices.ContainsKey(from)) throw new KeyNotFoundException(nameof(from) + " value is not found.");
			if (!Vertices.ContainsKey(to)) throw new KeyNotFoundException(nameof(to) + " value is not found.");

			if (!Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges))
			{
				fromEdges = NewEdgesContainer();
				Edges.Add(from, fromEdges);
			}

			if (fromEdges.TryGetValue(to, out TEdge fromEdge))
			{
				if (fromEdge.Weight.CompareTo(weight) > 0) fromEdge.Weight = weight;
			}
			else
			{
				fromEdge = NewEdge(to);
				fromEdge.Weight = weight;
				fromEdges.Add(fromEdge);
			}
		}

		/// <inheritdoc />
		public override void RemoveEdge(T from, T to)
		{
			if (!Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges)) return;
			fromEdges.RemoveByKey(to);
			if (fromEdges.Count == 0) Edges.Remove(from);
		}

		/// <inheritdoc />
		public override void SetWeight(T from, T to, TWeight weight)
		{
			if (Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges)
				&& fromEdges.TryGetValue(to, out TEdge fromEdge))
			{
				fromEdge.Weight = weight;
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
						foreach (TEdge edge in edges.Values.OrderBy(e => e.Weight))
						{
							if (visited.Contains(edge.To.Value)) continue;

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

	/// <inheritdoc/>
	[Serializable]
	public class WeightedDirectedGraphList<TWeight, T> : WeightedDirectedGraphList<GraphWeightedEdge<TWeight, T>, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		/// <inheritdoc />
		public WeightedDirectedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public WeightedDirectedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public WeightedDirectedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public WeightedDirectedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		protected override GraphWeightedEdge<TWeight, T> NewEdge([NotNull] T value)
		{
			if (!Vertices.TryGetValue(value, out GraphVertex<T> vertex)) throw new KeyNotFoundException();
			return new GraphWeightedEdge<TWeight, T>(vertex);
		}
	}

	/// <inheritdoc/>
	[Serializable]
	public class WeightedDirectedGraphList<T> : WeightedDirectedGraphList<GraphWeightedEdge<T>, int, T>
	{
		/// <inheritdoc />
		public WeightedDirectedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public WeightedDirectedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public WeightedDirectedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public WeightedDirectedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		protected override GraphWeightedEdge<T> NewEdge([NotNull] T value)
		{
			if (!Vertices.TryGetValue(value, out GraphVertex<T> vertex)) throw new KeyNotFoundException();
			return new GraphWeightedEdge<T>(vertex);
		}
	}
}