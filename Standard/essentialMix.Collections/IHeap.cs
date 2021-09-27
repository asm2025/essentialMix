using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public interface IHeap<T> : ICollection<T>
	{
		void Add([NotNull] IEnumerable<T> values);
	
		[NotNull]
		T Value();
		
		[NotNull]
		T ExtractValue();
		
		[NotNull]
		T ElementAt(int k);
	}

	public interface IHeap<TNode, T> : IHeap<T>
		where TNode : ITreeNode<T>
	{
		[NotNull]
		IComparer<T> Comparer { get; }

		[NotNull]
		TNode MakeNode(T value);

		[NotNull]
		TNode Add([NotNull] TNode node);
		
		bool Remove([NotNull] TNode node);
		
		TNode Find(T value);
		
		void DecreaseKey([NotNull] TNode node, [NotNull] T newKey);
		
		[NotNull]
		new TNode ExtractValue();
	}
}