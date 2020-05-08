using System.ComponentModel;

namespace asm.Collections
{
	public interface ILimited
	{
		[Browsable(false)]
		int Limit { get; set; }
		[Browsable(false)]
		bool LimitReached { get; }
		[Browsable(false)]
		LimitType LimitReachedBehavior { get; set; }

		void Refresh();
	}
}