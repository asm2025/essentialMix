using System.Collections.Generic;

namespace asm.Data.Patterns.Parameters
{
	public interface IIncludeSettings
	{
		IList<string> Include { get; set; }
	}
}