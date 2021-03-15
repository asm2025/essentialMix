using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[DebuggerDisplay("{From} => {Edge}")]
	public class EdgeEntry<T, TEdge>
	{
		public EdgeEntry([NotNull] T from, [NotNull] TEdge edge)
		{
			From = from;
			Edge = edge;
		}

		[NotNull]
		public readonly T From;
		[NotNull]
		public readonly TEdge Edge;
	}
}