using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Sorting
{
	[DebuggerDisplay("{Name} {Type}")]
	public struct SortField
	{
		/// <inheritdoc />
		public SortField([NotNull] string name)
			: this(name, SortType.Ascending)
		{
		}

		public SortField(string name, SortType type)
		{
			Name = name;
			Type = type;
		}

		public string Name { get; set; }
		public SortType Type { get; set; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return $"{Name} {Type}"; }
	}
}