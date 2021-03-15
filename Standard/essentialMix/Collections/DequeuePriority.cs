using System.ComponentModel.DataAnnotations;

namespace essentialMix.Collections
{
	public enum DequeuePriority
	{
		[Display(Name = "First-in-first-out")]
		FIFO,
		[Display(Name = "Last-in-first-out")]
		LIFO
	}
}