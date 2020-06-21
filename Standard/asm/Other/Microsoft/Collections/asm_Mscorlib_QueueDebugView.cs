using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Other.Microsoft.Collections
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	internal sealed class asm_Mscorlib_QueueDebugView<T>
	{
		private readonly Queue<T> _queue;

		public asm_Mscorlib_QueueDebugView([NotNull] Queue<T> queue)
		{
			_queue = queue;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		[NotNull]
		public T[] Items => _queue.ToArray();
	}
}