using System.Collections.ObjectModel;
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
	internal sealed class Other_Mscorlib_KeyedCollectionDebugView<TKey, TValue>
	{
		private readonly KeyedCollection<TKey, TValue> _keyedCollection;

		public Other_Mscorlib_KeyedCollectionDebugView([NotNull] KeyedCollection<TKey, TValue> keyedCollection)
		{
			_keyedCollection = keyedCollection;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		[NotNull]
		public TValue[] Items
		{
			get
			{
				TValue[] items = new TValue[_keyedCollection.Count];
				_keyedCollection.CopyTo(items, 0);
				return items;
			}
		}
	}
}