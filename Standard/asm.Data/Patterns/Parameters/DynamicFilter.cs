using System.Diagnostics;

namespace asm.Data.Patterns.Parameters
{
	/// <summary>
	/// https://github.com/StefH/System.Linq.Dynamic.Core/wiki/Dynamic-Expressions
	/// </summary>
	[DebuggerDisplay("{Expression}")]
	public struct DynamicFilter
	{
		public string Expression { get; set; }
		public object[] Arguments { get; set; }
	}
}