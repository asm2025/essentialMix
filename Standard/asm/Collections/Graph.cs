using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Collections
{
	public abstract class Graph<TNode, T> : GraphBase<TNode, T>
		where TNode : GraphNode<TNode, T>
	{
		/// <inheritdoc />
		protected Graph()
			: this((IEqualityComparer<T>)null)
		{
		}

		/// <inheritdoc />
		protected Graph(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		protected Graph([NotNull] IEnumerable<TNode> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected Graph([NotNull] IEnumerable<TNode> collection, IEqualityComparer<T> comparer)
			: base(collection, comparer)
		{
		}

		/// <inheritdoc />
		protected Graph(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}