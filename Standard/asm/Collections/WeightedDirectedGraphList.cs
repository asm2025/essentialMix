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
			// detect cycles
			ICollection<T> cycle = FindCycle();
			if (cycle != null && cycle.Count > 0) throw new Exception($"Cycle detected [{string.Join(", ", cycle)}]");

			Stack<T> result = new Stack<T>(Count);
			Stack<T> stack = new Stack<T>();
			HashSet<T> visited = new HashSet<T>(Comparer);

			foreach (T key in Edges.Keys)
			{
				stack.Push(key);

				while (stack.Count > 0)
				{
					if (version != _version) throw new VersionChangedException();

					T value = stack.Pop();
					if (visited.Contains(value)) continue;

					if (!Edges.TryGetValue(value, out KeyedDictionary<T, TEdge> edges))
					{
						result.Push(value);
						visited.Add(value);
						continue;
					}

					foreach (TEdge edge in edges.Values.OrderBy(e => e.Weight))
					{
						if (visited.Contains(edge.To.Value)) continue;
						stack.Push(edge.To.Value);
					}

					result.Push(value);
					visited.Add(value);
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