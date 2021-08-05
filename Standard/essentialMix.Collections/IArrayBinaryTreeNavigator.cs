using System.Collections.Generic;

namespace essentialMix.Collections
{
	public interface IArrayBinaryTreeNavigator<out T>
	{
		int Index { get; set; }
		T Value { get; }
		int ParentIndex { get; }
		int LeftIndex { get; }
		int RightIndex { get; }
		bool IsRoot { get; }
		bool IsLeft { get; }
		bool IsRight { get; }
		bool IsLeaf { get; }
		bool IsNode { get; }
		bool HasOneChild { get; }
		bool IsFull { get; }
		bool ParentIsRoot { get; }
		bool ParentIsLeft { get; }
		bool ParentIsRight { get; }

		string ToString();
		string ToString(int level);
		IEnumerable<int> Ancestors();
		int LeftMost();
		int RightMost();
		void Swap(int other);
		void Invalidate();
	}
}