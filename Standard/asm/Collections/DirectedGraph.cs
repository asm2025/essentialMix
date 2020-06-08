using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Collections
{
	public abstract class DirectedGraph<TNode, T> : Graph<TNode, T>
		where TNode : DirectedGraphNodeBase<TNode, T>
	{
		/// <inheritdoc />
		protected DirectedGraph()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected DirectedGraph(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected DirectedGraph([NotNull] IEnumerable<TNode> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected DirectedGraph([NotNull] IEnumerable<TNode> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		protected DirectedGraph(SerializationInfo info, StreamingContext context)
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