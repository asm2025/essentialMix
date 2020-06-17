using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("{To}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphEdge<TVertex, TEdge, T>
		where TVertex : GraphVertex<TVertex, T>
		where TEdge : GraphEdge<TVertex, TEdge, T>
	{
		protected GraphEdge([NotNull] TVertex to)
		{
			To = to;
		}

		[NotNull]
		public TVertex To { get; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return To.ToString(); }
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphEdge<TEdge, T> : GraphEdge<GraphVertex<T>, TEdge, T>
		where TEdge : GraphEdge<GraphVertex<T>, TEdge, T>
	{
		protected GraphEdge([NotNull] GraphVertex<T> to)
			: base(to)
		{
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class GraphEdge<T> : GraphEdge<GraphEdge<T>, T>
	{
		public GraphEdge([NotNull] GraphVertex<T> to)
			: base(to)
		{
		}
	}
}