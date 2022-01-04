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
// ReSharper disable once UnusedTypeParameter
public sealed class Dbg_DictionaryValueCollectionDebugView<TKey, TValue>
{
	private readonly ICollection<TValue> _collection;

	public Dbg_DictionaryValueCollectionDebugView([NotNull] ICollection<TValue> collection)
	{
		_collection = collection;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	[NotNull]
	public TValue[] Items
	{
		get
		{
			TValue[] items = new TValue[_collection.Count];
			_collection.CopyTo(items, 0);
			return items;
		}
	}
}