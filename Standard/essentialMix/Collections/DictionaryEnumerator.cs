using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public class DictionaryEnumerator : Enumerator, IDictionaryEnumerator, IEnumerator<DictionaryEntry>
	{
		public DictionaryEnumerator([NotNull] IDictionary dictionary)
			: base(dictionary)
		{
		}

		protected DictionaryEnumerator([NotNull] IEnumerable enumerable)
			: base(enumerable)
		{
		}

		public virtual DictionaryEntry Entry => Impl.Entry;

		public new DictionaryEntry Current => Entry;

		public object Key => Entry.Key;

		public object Value => Entry.Value;

		protected new IDictionaryEnumerator Impl => (IDictionaryEnumerator)base.Impl;
	}

	public class DictionaryEnumerator<TKey, TValue> : DictionaryEnumerator, IEnumerator<KeyValuePair<TKey, TValue>>
	{
		public DictionaryEnumerator([NotNull] IDictionary<TKey, TValue> dictionary)
			: base(dictionary)
		{
		}

		public DictionaryEnumerator([NotNull] IReadOnlyDictionary<TKey, TValue> dictionary)
			: base(dictionary)
		{
		}

		public DictionaryEnumerator([NotNull] IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
			: base(enumerable)
		{
		}

		public new virtual KeyValuePair<TKey, TValue> Entry => Impl.Current;

		public new KeyValuePair<TKey, TValue> Current => Entry;

		public new TKey Key => Entry.Key;

		public new TValue Value => Entry.Value;

		protected new IEnumerator<KeyValuePair<TKey, TValue>> Impl => (IEnumerator<KeyValuePair<TKey, TValue>>)base.Impl;
	}
}