using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Sorting;

[DebuggerDisplay("{Name} {Type}")]
public struct SortField(string name, SortType type)
{
	/// <inheritdoc />
	public SortField([NotNull] string name)
		: this(name, SortType.Ascending)
	{
	}

	public string Name { get; set; } = name;
	public SortType Type { get; set; } = type;

	/// <inheritdoc />
	[NotNull]
	public override string ToString() { return $"{Name} {Type}"; }
}