using System;
using System.ComponentModel.DataAnnotations;

namespace asm.Patterns.Collections
{
	[Flags]
	public enum TraverseMethodFlags
	{
		None = 0,
		[Display(Name = "Breadth First (Level Order)")]
		LevelOrder = 1, // Root-Left-Right (Queue)
		[Display(Name = "Depth First - PreOrder")]
		PreOrder = 1 << 1, // Root-Left-Right (Stack)
		[Display(Name = "Depth First - InOrder")]
		InOrder = 1 << 2, // Left-Root-Right (Stack)
		[Display(Name = "Depth First - PostOrder")]
		PostOrder = 1 << 3, // Left-Right-Root (Stack)
	}
}