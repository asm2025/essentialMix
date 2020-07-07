using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	public interface IKeyedHeap<TNode, TKey, TValue> : IHeap<TValue>
		where TNode : IKeyedNode<TKey, TValue>
	{
		[NotNull]
		IComparer<TKey> Comparer { get; }

		[NotNull]
		TNode MakeNode([NotNull] TValue value);
		void Add([NotNull] TNode node);
		bool Remove([NotNull] TNode node);
		TNode Find([NotNull] TValue value);
		TNode FindByKey([NotNull] TKey key);
		void DecreaseKey([NotNull] TNode node, [NotNull] TKey newKey);
	}
}