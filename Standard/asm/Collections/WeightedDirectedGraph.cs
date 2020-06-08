using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Collections
{
	public abstract class WeightedDirectedGraph<TNode, T> : WeightedGraph<TNode, T>
		where TNode : WeightedDirectedGraphNodeBase<TNode, T>
	{
		/// <inheritdoc />
		protected WeightedDirectedGraph()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected WeightedDirectedGraph(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedDirectedGraph([NotNull] IEnumerable<TNode> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected WeightedDirectedGraph([NotNull] IEnumerable<TNode> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedDirectedGraph(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc />
		protected override void Insert(T key, TNode value, bool add)
		{
			//TODO
		}

		/// <inheritdoc />
		public override bool RemoveByKey(T key)
		{
			//TODO
			return true;
		}

		/// <inheritdoc />
		public override void Clear()
		{
			//TODO
		}
	}
}