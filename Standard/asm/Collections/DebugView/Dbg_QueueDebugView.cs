using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Collections.DebugView
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	[DebuggerNonUserCode]
	public sealed class Dbg_QueueDebugView<T>
	{
		private readonly Queue<T> _queue;

		public Dbg_QueueDebugView([NotNull] Queue<T> queue)
		{
			_queue = queue;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		[NotNull]
		public T[] Items => _queue.ToArray();
	}
}