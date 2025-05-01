using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[DebuggerDisplay("{From} => {Edge}")]
public class EdgeEntry<T, TEdge>([NotNull] T from, [NotNull] TEdge edge)
{
	[NotNull]
	public readonly T From = from;
	[NotNull]
	public readonly TEdge Edge = edge;
}