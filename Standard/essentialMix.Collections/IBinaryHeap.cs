using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public interface IBinaryHeap<TNode, T> : IHeap<T>
		where TNode : ITreeNode<TNode, T>
	{
		[NotNull]
		IComparer<T> Comparer { get; }

		[NotNull]
		TNode MakeNode(T value);

		[NotNull]
		TNode Add([NotNull] TNode node);
		
		bool Remove([NotNull] TNode node);
		
		TNode Find(T value);
		
		void DecreaseKey([NotNull] TNode node, [NotNull] T newValue);
		
		[NotNull]
		TNode ExtractNode();
	}

	public interface IBinaryHeap<TNode, in TKey, TValue> : IHeap<TValue>
		where TNode : ITreeNode<TNode, TKey, TValue>
	{
		[NotNull]
		IComparer<TKey> Comparer { get; }

		[NotNull]
		TNode MakeNode(TValue value);

		[NotNull]
		TNode Add([NotNull] TNode node);
		
		bool Remove([NotNull] TNode node);
		
		TNode Find(TValue value);
		
		TNode FindByKey([NotNull] TKey key);
		
		void DecreaseKey([NotNull] TNode node, [NotNull] TKey newKey);
		
		[NotNull]
		TNode ExtractNode();
	}
}