using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using essentialMix.Collections.DebugView;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
[Serializable]
public class ReadOnlyBoundList<T>([NotNull] IReadOnlyBoundList<T> list) : IReadOnlyBoundList<T>
{
	private IReadOnlyBoundList<T> _list = list;

	public ReadOnlyBoundList([NotNull] IBoundList<T> list)
		: this(list as IReadOnlyBoundList<T> ?? throw new ArgumentException($"Argument does not implement {typeof(IReadOnlyBoundList<T>)}.", nameof(list)))
	{
	}

	public int Capacity => _list.Capacity;

	public int Limit => _list.Limit;

	public int Count => _list.Count;

	/// <inheritdoc />
	public T this[int index] => _list[index];

	public IEnumerator<T> GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}