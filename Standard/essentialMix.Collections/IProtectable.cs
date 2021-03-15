using System.ComponentModel;

namespace essentialMix.Collections
{
	public interface IProtectable
	{
		[Browsable(false)]
		bool IsProtected { get; set; }
	}
}