namespace essentialMix.Collections
{
	public interface ILinkedNode<out TNode, TKey, TValue> : IKeyedNode<TKey, TValue>
		where TNode : ILinkedNode<TNode, TKey, TValue>
	{
		TNode Left { get;  }
		TNode Right { get;  }
		bool IsLeaf { get;  }

	}
}