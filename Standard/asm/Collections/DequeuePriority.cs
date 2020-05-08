using System.ComponentModel.DataAnnotations;

namespace asm.Collections
{
	public enum DequeuePriority
	{
		[Display(Name = "First-in-first-out")]
		FIFO,
		[Display(Name = "Last-in-first-out")]
		LIFO
	}
}