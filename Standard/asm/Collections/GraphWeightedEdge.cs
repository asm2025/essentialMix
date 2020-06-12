using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("->{To} :{Weight}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphWeightedEdge<TVertex, TEdge, TWeight, T> : GraphEdge<TVertex, TEdge, T>
		where TVertex : GraphVertex<TVertex, T>
		where TEdge : GraphWeightedEdge<TVertex, TEdge, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		protected GraphWeightedEdge([NotNull] TVertex to)
			: base(to)
		{
		}

		public TWeight Weight { get; set; }

		/// <inheritdoc />
		public override string ToString() { return $"{To} :{Weight}"; }
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphWeightedEdge<TEdge, TWeight, T> : GraphWeightedEdge<GraphVertex<T>, TEdge, TWeight, T>
		where TEdge : GraphWeightedEdge<GraphVertex<T>, TEdge, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		protected GraphWeightedEdge([NotNull] GraphVertex<T> to)
			: base(to)
		{
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class GraphWeightedEdge<TWeight, T> : GraphWeightedEdge<GraphWeightedEdge<TWeight, T>, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		public GraphWeightedEdge([NotNull] GraphVertex<T> to)
			: base(to)
		{
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class GraphWeightedEdge<T> : GraphWeightedEdge<GraphWeightedEdge<T>, int, T>
	{
		public GraphWeightedEdge([NotNull] GraphVertex<T> to)
			: base(to)
		{
		}
	}
}