using System.ComponentModel;

namespace asm.Collections
{
	public interface IProtectable
	{
		[Browsable(false)]
		bool IsProtected { get; set; }
	}
}