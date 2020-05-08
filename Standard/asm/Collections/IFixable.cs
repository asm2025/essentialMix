using System.ComponentModel;

namespace asm.Collections
{
	public interface IFixable
	{
		[Browsable(false)]
		bool IsFixed { get; }
	}
}