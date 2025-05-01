using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Collections.DebugView;

/*
* VS IDE can't differentiate between types with the same name from different
* assembly. So we need to use different names for collection debug view for
* collections in this solution assemblies.
*/
[DebuggerNonUserCode]
public sealed class Dbg_StackDebugView<T>([NotNull] Stack<T> stack)
{
	private readonly Stack<T> _stack = stack;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	[NotNull]
	public T[] Items => _stack.ToArray();
}