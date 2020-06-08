using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Collections
{
	public abstract class WeightedGraph<TNode, T> : GraphBase<TNode, T>
		where TNode : WeightedGraphNode<TNode, T>
	{
		/// <inheritdoc />
		protected WeightedGraph()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected WeightedGraph(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedGraph([NotNull] IEnumerable<TNode> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected WeightedGraph([NotNull] IEnumerable<TNode> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedGraph(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}