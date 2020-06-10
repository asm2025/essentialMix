using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("->{To} :{Weight}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphWeightedEdge<TNode, TEdge, TWeight, T> : GraphEdge<TNode, TEdge, T>
		where TNode : GraphNode<TNode, T>
		where TEdge : GraphWeightedEdge<TNode, TEdge, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		protected GraphWeightedEdge([NotNull] TNode to)
			: base(to)
		{
		}

		public TWeight Weight { get; set; }

		/// <inheritdoc />
		public override string ToString() { return $"{To} :{Weight}"; }
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphWeightedEdge<TEdge, TWeight, T> : GraphWeightedEdge<GraphNode<T>, TEdge, TWeight, T>
		where TEdge : GraphWeightedEdge<GraphNode<T>, TEdge, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		protected GraphWeightedEdge([NotNull] GraphNode<T> to)
			: base(to)
		{
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class GraphWeightedEdge<TWeight, T> : GraphWeightedEdge<GraphWeightedEdge<TWeight, T>, TWeight, T>
		where TWeight : struct, IComparable<TWeight>, IComparable, IEquatable<TWeight>, IConvertible, IFormattable
	{
		public GraphWeightedEdge([NotNull] GraphNode<T> to)
			: base(to)
		{
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class GraphWeightedEdge<T> : GraphWeightedEdge<GraphWeightedEdge<T>, int, T>
	{
		public GraphWeightedEdge([NotNull] GraphNode<T> to)
			: base(to)
		{
		}
	}
}