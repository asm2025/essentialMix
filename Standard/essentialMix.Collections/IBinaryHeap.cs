using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public interface IBinaryHeapBase<TNode, T> : IHeap<T>
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
		
		[NotNull]
		TNode ExtractNode();
	}

	public interface IBinaryHeap<TNode, T> : IBinaryHeapBase<TNode, T>
		where TNode : ITreeNode<TNode, T>
	{
		void DecreaseKey([NotNull] TNode node, [NotNull] T newValue);
		void DecreaseKey(int index, [NotNull] T newValue);
	}

	public interface IBinaryHeap<TNode, TKey, TValue> : IBinaryHeapBase<TNode, TValue>
		where TNode : ITreeNode<TNode, TKey, TValue>
	{
		[NotNull]
		IComparer<TKey> KeyComparer { get; }
		
		int IndexOfKey([NotNull] TKey key);
		TNode FindByKey([NotNull] TKey key);
		
		void DecreaseKey([NotNull] TNode node, [NotNull] TKey newKey);
		void DecreaseKey(int index, [NotNull] TKey newKey);
	}
}