using System;
using System.Collections.Generic;
using System.Linq;
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
	public abstract class WeightedMixedGraphList<TEdge, TWeight, T> : WeightedGraphList<TEdge, TWeight, T>
		where TEdge : GraphWeightedEdge<GraphVertex<T>, TEdge, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		/// <inheritdoc />
		protected WeightedMixedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected WeightedMixedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedMixedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected WeightedMixedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		public override void AddEdge(T from, T to) { AddEdge(from, to, default(TWeight), false); }
		/// <inheritdoc />
		public override void AddEdge(T from, T to, TWeight weight) { AddEdge(from, to, weight, false); }
		public void AddEdge([NotNull] T from, [NotNull] T to, bool undirected) { AddEdge(from, to, default(TWeight), undirected); }
		public void AddEdge([NotNull] T from, [NotNull] T to, TWeight weight, bool undirected)
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

			// short-circuit - loop edge
			if (!undirected || Comparer.Equals(from, to)) return;

			if (!Edges.TryGetValue(to, out KeyedDictionary<T, TEdge> toEdges))
			{
				toEdges = NewEdgesContainer();
				Edges.Add(to, toEdges);
			}

			if (toEdges.TryGetValue(to, out TEdge toEdge))
			{
				if (toEdge.Weight.CompareTo(weight) > 0) toEdge.Weight = weight;
			}
			else
			{
				toEdge = NewEdge(from);
				toEdge.Weight = weight;
				toEdges.Add(toEdge);
			}
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

		/// <inheritdoc />
		public override void SetWeight(T from, T to, TWeight weight) { SetWeight(from, to, weight, false); }
		public void SetWeight([NotNull] T from, [NotNull] T to, TWeight weight, bool undirected)
		{
			if (Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges)
				&& fromEdges.TryGetValue(to, out TEdge fromEdge))
			{
				fromEdge.Weight = weight;
			}

			if (undirected && Edges.TryGetValue(to, out KeyedDictionary<T, TEdge> toEdges)
				&& toEdges.TryGetValue(from, out TEdge toEdge))
			{
				toEdge.Weight = weight;
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
			ICollection<T> cycle = FindCycle(true);
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
						if (visited.Contains(edge.To.Value) || IsLoop(value, edge)) continue;
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
	public class WeightedMixedGraphList<TWeight, T> : WeightedMixedGraphList<GraphWeightedEdge<TWeight, T>, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		/// <inheritdoc />
		public WeightedMixedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public WeightedMixedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public WeightedMixedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public WeightedMixedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
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
	public class WeightedMixedGraphList<T> : WeightedMixedGraphList<GraphWeightedEdge<T>, int, T>
	{
		/// <inheritdoc />
		public WeightedMixedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public WeightedMixedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public WeightedMixedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public WeightedMixedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
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