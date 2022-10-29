﻿using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Collections.DebugView;

/*
* VS IDE can't differentiate between types with the same name from different
* assembly. So we need to use different names for collection debug view for
* collections in this solution assemblies.
*/
[DebuggerNonUserCode]
public class Dbg_HashSetDebugView<T>
{
	private readonly HashSetBase<T> _set;

	public Dbg_HashSetDebugView(HashSetBase<T> set)
	{
		_set = set;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	[NotNull]
	public T[] Items => _set.ToArray();
}