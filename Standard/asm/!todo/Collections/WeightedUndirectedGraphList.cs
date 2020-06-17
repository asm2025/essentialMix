using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para>Weighted Undirected Graph</para>
	/// </summary>
	/// <inheritdoc/>
	[Serializable]
	public abstract class WeightedUndirectedGraphList<TEdge, TWeight, T> : WeightedGraphList<TEdge, TWeight, T>
		where TEdge : GraphWeightedEdge<GraphVertex<T>, TEdge, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		/// <inheritdoc />
		protected WeightedUndirectedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected WeightedUndirectedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedUndirectedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected WeightedUndirectedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
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

			// short-circuit - loop edge
			if (Comparer.Equals(from, to)) return;

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
		public override void RemoveEdge(T from, T to)
		{
			if (Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges))
			{
				fromEdges.RemoveByKey(to);
				if (fromEdges.Count == 0) Edges.Remove(from);
			}

			if (Edges.TryGetValue(to, out KeyedDictionary<T, TEdge> toEdges))
			{
				toEdges.RemoveByKey(from);
				if (toEdges.Count == 0) Edges.Remove(to);
			}
		}

		public override void SetWeight(T from, T to, TWeight weight)
		{
			if (Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges)
				&& fromEdges.TryGetValue(to, out TEdge fromEdge))
			{
				fromEdge.Weight = weight;
			}

			if (Edges.TryGetValue(to, out KeyedDictionary<T, TEdge> toEdges)
				&& toEdges.TryGetValue(from, out TEdge toEdge))
			{
				toEdge.Weight = weight;
			}
		}

		/// <inheritdoc />
		public override int GetSize()
		{
			int sum = 0;

			foreach (KeyValuePair<T, KeyedDictionary<T, TEdge>> pair in Edges)
			{
				foreach (TEdge edge in pair.Value.Values)
				{
					if (IsLoop(pair.Key, edge)) sum += 2;
					else sum++;
				}
			}

			return sum / 2;
		}

		/// <inheritdoc />
		protected override GraphVertex<T> NewVertex([NotNull] T value)
		{
			return new GraphVertex<T>(value);
		}
	}

	/// <inheritdoc/>
	[Serializable]
	public class WeightedUndirectedGraphList<TWeight, T> : WeightedUndirectedGraphList<GraphWeightedEdge<TWeight, T>, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		/// <inheritdoc />
		public WeightedUndirectedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public WeightedUndirectedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public WeightedUndirectedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public WeightedUndirectedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
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
	public class WeightedUndirectedGraphList<T> : WeightedUndirectedGraphList<GraphWeightedEdge<T>, int, T>
	{
		/// <inheritdoc />
		public WeightedUndirectedGraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		public WeightedUndirectedGraphList(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public WeightedUndirectedGraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public WeightedUndirectedGraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
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