using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[Serializable]
[DebuggerDisplay("Count = {Count}")]
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class ReadOnlySet<T> : IReadOnlySet<T>
{
	private readonly ISet<T> _set;

	public ReadOnlySet([NotNull] ISet<T> set)
	{
		_set = set;
	}

	public IEnumerator<T> GetEnumerator() { return _set.GetEnumerator(); }

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	public int Count => _set.Count;

	public bool Contains(T item) { return _set.Contains(item); }

	public void CopyTo([NotNull] T[] array) { _set.CopyTo(array, 0); }

	public void CopyTo([NotNull] T[] array, int arrayIndex) { _set.CopyTo(array, arrayIndex); }

	public bool IsSubsetOf([NotNull] IEnumerable<T> other) { return _set.IsSubsetOf(other); }

	public bool IsSupersetOf([NotNull] IEnumerable<T> other) { return _set.IsSupersetOf(other); }

	public bool IsProperSupersetOf([NotNull] IEnumerable<T> other) { return _set.IsProperSupersetOf(other); }

	public bool IsProperSubsetOf([NotNull] IEnumerable<T> other) { return _set.IsProperSubsetOf(other); }

	public bool Overlaps([NotNull] IEnumerable<T> other) { return _set.Overlaps(other); }

	public bool SetEquals([NotNull] IEnumerable<T> other) { return _set.SetEquals(other); }
}