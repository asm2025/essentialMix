using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("->{To} :{Weight}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphWeightedEdge<TNode, TEdge, T> : GraphEdge<TNode, TEdge, T>
		where TNode : GraphNode<TNode, T>
		where TEdge : GraphWeightedEdge<TNode, TEdge, T>
	{
		protected GraphWeightedEdge([NotNull] TNode to)
			: base(to)
		{
		}

		public int Weight { get; set; }
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphWeightedEdge<TEdge, T> : GraphWeightedEdge<GraphNode<T>, TEdge, T>
		where TEdge : GraphWeightedEdge<GraphNode<T>, TEdge, T>
	{
		protected GraphWeightedEdge([NotNull] GraphNode<T> to)
			: base(to)
		{
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class GraphWeightedEdge<T> : GraphWeightedEdge<GraphWeightedEdge<T>, T>
	{
		public GraphWeightedEdge([NotNull] GraphNode<T> to)
			: base(to)
		{
		}
	}
}