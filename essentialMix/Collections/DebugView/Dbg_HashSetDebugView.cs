using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Collections.DebugView;

/*
* VS IDE can't differentiate between types with the same name from different
* assembly. So we need to use different names for collection debug view for
* collections in this solution assemblies.
*/
[DebuggerNonUserCode]
public class Dbg_HashSetDebugView<T>(HashSetBase<T> set)
{
	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	[NotNull]
	public T[] Items => set.ToArray();
}