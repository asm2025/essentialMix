using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class DirectedGraphNodeBase<TNode, T> : GraphNode<TNode, T>
		where TNode : DirectedGraphNodeBase<TNode, T>
	{
		protected DirectedGraphNodeBase([NotNull] T value)
			: base(value)
		{
		}

		/// <inheritdoc />
		public override void Add(TNode to)
		{
			if (to == null) throw new ArgumentNullException(nameof(to));
			AdjacencyList.Add(to);
		}

		/// <inheritdoc />
		public override bool Remove(TNode to)
		{
			if (to == null) throw new ArgumentNullException(nameof(to));
			return AdjacencyList.Remove(to);
		}

		/// <inheritdoc />
		public override void Clear()
		{
			AdjacencyList.Clear();
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class DirectedGraphNode<T> : DirectedGraphNodeBase<DirectedGraphNode<T>, T>
	{
		public DirectedGraphNode([NotNull] T value)
			: base(value)
		{
		}
	}
}