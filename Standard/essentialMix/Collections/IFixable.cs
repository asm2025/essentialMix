using System.ComponentModel;

namespace essentialMix.Collections
{
	public interface IFixable
	{
		[Browsable(false)]
		bool IsFixed { get; }
	}
}