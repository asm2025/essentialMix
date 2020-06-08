using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public abstract class WeightedDirectedGraphNodeBase<TNode, T> : WeightedGraphNode<TNode, T>
		where TNode : WeightedDirectedGraphNodeBase<TNode, T>
	{
		protected WeightedDirectedGraphNodeBase([NotNull] T value)
			: base(value)
		{
		}

		/// <inheritdoc />
		public override void Add(TNode to, int weight)
		{
			AdjacencyList.Add(to, weight);
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
	public class WeightedDirectedGraphNode<T> : WeightedDirectedGraphNodeBase<WeightedDirectedGraphNode<T>, T>
	{
		public WeightedDirectedGraphNode([NotNull] T value)
			: base(value)
		{
		}
	}
}