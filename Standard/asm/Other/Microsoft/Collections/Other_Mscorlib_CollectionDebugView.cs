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
	[DebuggerNonUserCode]
	internal sealed class Other_Mscorlib_CollectionDebugView<T>
	{
		private readonly ICollection<T> _collection;

		public Other_Mscorlib_CollectionDebugView([NotNull] ICollection<T> collection)
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