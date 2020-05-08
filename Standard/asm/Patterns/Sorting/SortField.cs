using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Patterns.Sorting
{
	[DebuggerDisplay("{Name} {Type}")]
	public struct SortField
	{
		public string Name { get; set; }
		public SortType Type { get; set; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return $"{Name} {Type}"; }
	}
}