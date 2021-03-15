using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public class ReadOnlyList<T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
	{
		private readonly IList<T> _list;

		public ReadOnlyList([NotNull] IList<T> list)
		{
			_list = list;
		}

		public IEnumerator<T> GetEnumerator() { return _list.GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public T this[int index] => _list[index];

		public int Count => _list.Count;

		public bool Contains(T item) { return _list.Contains(item); }

		public void CopyTo([NotNull] T[] array) { _list.CopyTo(array, 0); }

		public void CopyTo([NotNull] T[] array, int arrayIndex) { _list.CopyTo(array, arrayIndex); }
	}
}