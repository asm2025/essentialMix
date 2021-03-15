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
	public sealed class Dbg_CollectionDebugView<T>
	{
		private readonly ICollection<T> _collection;

		public Dbg_CollectionDebugView([NotNull] ICollection<T> collection)
		{
			_collection = collection;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		[NotNull]
		public T[] Items
		{
			get
			{
				T[] items = new T[_collection.Count];
				_collection.CopyTo(items, 0);
				return items;
			}
		}
	}
}