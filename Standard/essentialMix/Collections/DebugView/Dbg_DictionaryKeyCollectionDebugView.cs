using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Collections.DebugView
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	[DebuggerNonUserCode]
	// ReSharper disable once UnusedTypeParameter
	public sealed class Dbg_DictionaryKeyCollectionDebugView<TKey, TValue>
	{
		private readonly ICollection<TKey> _collection;

		public Dbg_DictionaryKeyCollectionDebugView([NotNull] ICollection<TKey> collection)
		{
			_collection = collection;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		[NotNull]
		public TKey[] Items
		{
			get
			{
				TKey[] items = new TKey[_collection.Count];
				_collection.CopyTo(items, 0);
				return items;
			}
		}
	}
}