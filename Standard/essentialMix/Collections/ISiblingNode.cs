using System.Collections.Generic;

namespace essentialMix.Collections
{
	public interface ISiblingNode<TNode, T> : ITreeNode<TNode, T>
		where TNode : ISiblingNode<TNode, T>
	{
		TNode Child { get;  }
		TNode Sibling { get;  }
		bool IsLeaf { get;  }
		IEnumerable<TNode> Children();
		IEnumerable<TNode> Siblings();
	}

	public interface ISiblingNode<TNode, TKey, TValue> : ITreeNode<TNode, TKey, TValue>
		where TNode : ISiblingNode<TNode, TKey, TValue>
	{
		TNode Child { get;  }
		TNode Sibling { get;  }
		bool IsLeaf { get;  }
		IEnumerable<TNode> Children();
		IEnumerable<TNode> Siblings();
	}
}