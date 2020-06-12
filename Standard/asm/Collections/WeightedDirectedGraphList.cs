using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para><see href="https://en.wikipedia.org/wiki/Graph_(discrete_mathematics)">Weighted Directed Graph</see></para>
	/// </summary>
	/// <inheritdoc/>
	/// <typeparam name="TEdge"><inheritdoc/></typeparam>
	/// <typeparam name="TWeight">The weight of the edge.</typeparam>
	/// <typeparam name="T"><inheritdoc/></typeparam>
	[Serializable]
	public abstract class WeightedDirectedGraphList<TEdge, TWeight, T> : DirectedGraphList<TEdge, T>
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
		public override void AddEdge(T from, T to) { AddEdge(from, to, default(TWeight)); }
		public void AddEdge([NotNull] T from, [NotNull] T to, TWeight weight)
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

		public void SetWeight([NotNull] T from, [NotNull] T to, TWeight weight)
		{
			if (!Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> fromEdges)
				|| !fromEdges.TryGetValue(to, out TEdge fromEdge))
			{
				throw new KeyNotFoundException(nameof(from) + " value is not found.");
			}

			fromEdge.Weight = weight;
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