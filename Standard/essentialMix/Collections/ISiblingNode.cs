using System.Collections.Generic;

namespace essentialMix.Collections
{
	public interface ISiblingNode<out TNode, T> : ITreeNode<T>
		where TNode : ISiblingNode<TNode, T>
	{
		TNode Child { get;  }
		TNode Sibling { get;  }
		bool IsLeaf { get;  }
		IEnumerable<TNode> Siblings();
	}

	public interface ISiblingNode<out TNode, TKey, TValue> : IKeyedNode<TKey, TValue>
		where TNode : ISiblingNode<TNode, TKey, TValue>
	{
		TNode Child { get;  }
		TNode Sibling { get;  }
		bool IsLeaf { get;  }
		IEnumerable<TNode> Siblings();
	}
}