using System.Collections.Generic;

namespace asm.Data.Patterns.Parameters
{
	public interface IFilterSettings
	{
		string FilterExpression { get; set; }
		ICollection<string> FilterReferences { get; set; }
		ICollection<string> FilterImports { get; set; }
	}
}