using System.Diagnostics;
using Other.Microsoft.Collections;
using JetBrains.Annotations;

namespace asm.Collections.DebugView
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	[DebuggerNonUserCode]
	public class Dbg_HashSetDebugView<T>
	{
		private readonly HashSetBase<T> set;

		public Dbg_HashSetDebugView(HashSetBase<T> set)
		{
			this.set = set;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		[NotNull]
		public T[] Items => set.ToArray();
	}
}