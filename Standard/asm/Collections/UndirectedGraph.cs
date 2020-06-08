using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Collections
{
	public abstract class UndirectedGraph<TNode, T> : Graph<TNode, T>
		where TNode : UndirectedGraphNodeBase<TNode, T>
	{
		/// <inheritdoc />
		protected UndirectedGraph()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected UndirectedGraph(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected UndirectedGraph([NotNull] IEnumerable<TNode> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected UndirectedGraph([NotNull] IEnumerable<TNode> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		protected UndirectedGraph(SerializationInfo info, StreamingContext context)
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