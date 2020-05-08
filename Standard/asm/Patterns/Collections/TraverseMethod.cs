using System.ComponentModel.DataAnnotations;

namespace asm.Patterns.Collections
{
	/// <summary>
	/// How to Pick One?
	/// 1. Extra Space required for Level Order Traversal is O(w) 
	/// where w is maximum width of Binary Tree. In level order 
	/// traversal, queue one by one stores nodes of different level.
	/// 2. Extra Space required for Depth First Traversals is O(h) 
	/// where h is maximum height of Binary Tree. In Depth First 
	/// Traversals, stack (or function call stack) stores all 
	/// ancestors of a node.
	/// 3. Depth First Traversals are typically recursive and recursive 
	/// code requires function call overheads.
	/// 4. The most important points is, BFS starts visiting nodes from 
	/// root while DFS starts visiting nodes from leaves. So if our problem 
	/// is to search something that is more likely is closer to root, we 
	/// would prefer BFS. And if the target node is close to a leaf, we 
	/// would prefer DFS.
	/// </summary>
	public enum TraverseMethod
	{
		[Display(Name = "Breadth First (Level Order)")]
		LevelOrder, // Root-Left-Right (Queue)
		[Display(Name = "Depth First - PreOrder")]
		PreOrder, // Root-Left-Right (Stack)
		[Display(Name = "Depth First - InOrder")]
		InOrder, // Left-Root-Right (Stack)
		[Display(Name = "Depth First - PostOrder")]
		PostOrder, // Left-Right-Root (Stack)
	}
}