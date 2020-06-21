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
	internal sealed class asm_Mscorlib_DictionaryKeyCollectionDebugView<TKey, TValue>
	{
		private readonly ICollection<TKey> _collection;

		public asm_Mscorlib_DictionaryKeyCollectionDebugView([NotNull] ICollection<TKey> collection)
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