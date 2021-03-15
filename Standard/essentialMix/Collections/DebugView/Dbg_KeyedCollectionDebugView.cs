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
	public sealed class Dbg_KeyedCollectionDebugView<TKey, TValue>
	{
		private readonly System.Collections.ObjectModel.KeyedCollection<TKey, TValue> _keyedCollection;

		public Dbg_KeyedCollectionDebugView([NotNull] System.Collections.ObjectModel.KeyedCollection<TKey, TValue> keyedCollection)
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