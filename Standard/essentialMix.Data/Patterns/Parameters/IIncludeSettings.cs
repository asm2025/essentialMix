using System.Collections.Generic;

namespace essentialMix.Data.Patterns.Parameters
{
	public interface IIncludeSettings
	{
		IList<string> Include { get; set; }
	}
}