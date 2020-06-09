using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("->{To}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphEdge<TNode, TEdge, T>
		where TNode : GraphNode<TNode, T>
		where TEdge : GraphEdge<TNode, TEdge, T>
	{
		protected GraphEdge([NotNull] TNode to)
		{
			To = to;
		}

		[NotNull]
		public TNode To { get; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return To.ToString(); }
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphEdge<TNode, T> : GraphEdge<TNode, GraphEdge<TNode, T>, T>
		where TNode : GraphNode<TNode, T>
	{
		protected GraphEdge([NotNull] TNode to)
			: base(to)
		{
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class GraphEdge<T> : GraphEdge<GraphNode<T>, GraphEdge<T>, T>
	{
		public GraphEdge([NotNull] GraphNode<T> to)
			: base(to)
		{
		}
	}
}