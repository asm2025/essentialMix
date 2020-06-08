using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Collections
{
	public abstract class WeightedUndirectedGraph<TNode, T> : WeightedGraph<TNode, T>
		where TNode : WeightedUndirectedGraphNodeBase<TNode, T>
	{
		/// <inheritdoc />
		protected WeightedUndirectedGraph()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected WeightedUndirectedGraph(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedUndirectedGraph([NotNull] IEnumerable<TNode> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected WeightedUndirectedGraph([NotNull] IEnumerable<TNode> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		protected WeightedUndirectedGraph(SerializationInfo info, StreamingContext context)
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