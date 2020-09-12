using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Collections.DebugView
{
	/*
	 * VS IDE can't differentiate between types with the same name from different
	 * assembly. So we need to use different names for collection debug view for
	 * collections in this solution assemblies.
	 */
	[DebuggerNonUserCode]
	public sealed class Dbg_DictionaryDebugView<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> _dictionary;

		public Dbg_DictionaryDebugView([NotNull] IDictionary<TKey, TValue> dictionary)
		{
			_dictionary = dictionary;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		[NotNull]
		public KeyValuePair<TKey, TValue>[] Items
		{
			get
			{
				KeyValuePair<TKey, TValue>[] items = new KeyValuePair<TKey, TValue>[_dictionary.Count];
				_dictionary.CopyTo(items, 0);
				return items;
			}
		}
	}
}