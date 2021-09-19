using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public class KeyedCollection<TKey, TValue> : KeyedCollectionBase<TKey, TValue>
	{
		[NotNull]
		private readonly Func<TValue, TKey> _getKey;

		public KeyedCollection([NotNull] Func<TValue, TKey> getKey)
			: this(getKey, (IEqualityComparer<TKey>)null)
		{
		}

		public KeyedCollection([NotNull] Func<TValue, TKey> getKey, IEqualityComparer<TKey> comparer)
			: base(comparer)
		{
			_getKey = getKey;
		}

		public KeyedCollection([NotNull] Func<TValue, TKey> getKey, [NotNull] IEnumerable<TValue> collection)
			: this(getKey, collection, null)
		{
		}

		public KeyedCollection([NotNull] Func<TValue, TKey> getKey, [NotNull] IEnumerable<TValue> collection, IEqualityComparer<TKey> comparer)
			: base(collection, comparer)
		{
			_getKey = getKey;
		}

		/// <inheritdoc />
		protected sealed override TKey GetKeyForItem(TValue item) { return _getKey(item); }
	}
}