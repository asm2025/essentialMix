using System.Diagnostics;

using JetBrains.Annotations;

namespace asm.Other.Microsoft.Collections
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	[DebuggerNonUserCode]
	internal class Other_Mscorlib_HashSetDebugView<T>
	{
		private readonly HashSetBase<T> set;

		public Other_Mscorlib_HashSetDebugView(HashSetBase<T> set)
		{
			this.set = set;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		[NotNull]
		public T[] Items => set.ToArray();
	}
}