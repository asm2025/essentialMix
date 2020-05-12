using System.ComponentModel.DataAnnotations;

namespace asm.Patterns.Collections
{
	/// <summary>
	/// <para>
	/// Breadth First (BFS) (LevelOrder): FCIADGJBEHK => Root-Left-Right (Queue)
	/// </para>
	/// <para>
	/// Depth First (DFS) [PreOrder]:   FCABDEIGHJK => Root-Left-Right (Stack)
	/// </para>
	/// <para>
	/// Depth First (DFS) [InOrder]:    ABCDEFGHIJK => Left-Root-Right (Stack)
	/// </para>
	/// <para>
	/// Depth First (DFS) [PostOrder]:  BAEDCHGKJIF => Left-Right-Root (Stack)
	/// </para>
	/// <para>
	/// If the problem is to search for a value that is more likely is closer to
	/// the root, BFS would should be preferred but if the target node is closer
	/// to a leaf, we would prefer DFS (more likely PostOrder or depending on the
	/// situation).
	/// </para>
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