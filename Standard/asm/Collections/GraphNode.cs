using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("{Value}")]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class GraphNode<TNode, T>
		where TNode : GraphNode<TNode, T>
	{
		protected GraphNode([NotNull] T value)
		{
			Value = value;
		}

		[NotNull]
		public T Value { get; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		public static implicit operator T([NotNull] GraphNode<TNode, T> node) { return node.Value; }
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class GraphNode<T> : GraphNode<GraphNode<T>, T>
	{
		public GraphNode([NotNull] T value)
			: base(value)
		{
		}
	}
}