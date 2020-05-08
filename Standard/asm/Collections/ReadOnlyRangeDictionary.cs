using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[DebuggerDisplay("Count = {Count}")]
	[Serializable]
	public class ReadOnlyRangeDictionary<TKey, TValue> : IReadOnlyRangeDictionary<TKey, TValue>,
		ICollection,
		IEnumerable,
		IDeserializationCallback,
		ISerializable
		where TKey : IComparable
	{
		[NonSerialized]
		private SerializationInfo _siInfo;

		/// <inheritdoc />
		public ReadOnlyRangeDictionary()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		protected ReadOnlyRangeDictionary(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		public ReadOnlyRangeDictionary([NotNull] IDictionary<(TKey Minimum, TKey Maximum), TValue> dictionary)
			: this(0, dictionary)
		{
		}

		protected ReadOnlyRangeDictionary(int capacity, IDictionary<(TKey Minimum, TKey Maximum), TValue> dictionary)
		{
			if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			Dictionary = dictionary != null
							? new Dictionary<(TKey, TKey), TValue>(dictionary, Comparer)
							: new Dictionary<(TKey, TKey), TValue>(capacity, Comparer);
		}

		protected ReadOnlyRangeDictionary(SerializationInfo info, StreamingContext context)
		{
			_siInfo = info;
		}

		/// <inheritdoc />
		[SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		[SecurityCritical]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null) throw new ArgumentNullException(nameof(info));
			KeyValuePair<(TKey Minimum, TKey Maximum), TValue>[] array = Dictionary.ToArray();
			if (array.Length == 0) return;
			info.AddValue("Elements", array, typeof(KeyValuePair<(TKey Minimum, TKey Maximum), TValue>[]));
		}

		/// <inheritdoc />
		public virtual void OnDeserialization(object sender)
		{
			if (_siInfo == null) return;

			KeyValuePair<(TKey Minimum, TKey Maximum), TValue>[] array = (KeyValuePair<(TKey Minimum, TKey Maximum), TValue>[])_siInfo.GetValue("Elements", typeof(KeyValuePair<(TKey Minimum, TKey Maximum), TValue>[]));

			if (array != null && array.Length > 0)
			{
				Dictionary = new Dictionary<(TKey Minimum, TKey Maximum), TValue>(array.Length, Comparer);
				Dictionary.AddRange(array);
			}
			else
			{
				Dictionary = new Dictionary<(TKey Minimum, TKey Maximum), TValue>(Comparer);
			}

			_siInfo = null;
		}

		/// <inheritdoc />
		bool ICollection.IsSynchronized => DictionaryAsCollection.IsSynchronized;

		/// <inheritdoc />
		object ICollection.SyncRoot => DictionaryAsCollection.SyncRoot;

		[field: NonSerialized]
		protected Dictionary<(TKey, TKey), TValue> Dictionary { get; private set; }

		[NotNull]
		protected ICollection DictionaryAsCollection => Dictionary;

		[NotNull]
		protected ICollection<KeyValuePair<(TKey Minimum, TKey Maximum), TValue>> DictionaryAsGenericCollection => Dictionary;

		[NotNull]
		protected IDictionary DictionaryAsDictionary => Dictionary;

		[NotNull]
		[field: NonSerialized]
		public RangeDictionaryComparer<TKey> Comparer { get; } = RangeDictionaryComparer<TKey>.Default;

		/// <inheritdoc />
		public virtual TValue this[(TKey Minimum, TKey Maximum) key]
		{
			get => Dictionary[key];
			set => throw new NotSupportedException();
		}

		/// <inheritdoc />
		public virtual TValue this[TKey key]
		{
			get => Dictionary[GetKey(key)];
			set => throw new NotSupportedException();
		}

		/// <inheritdoc />
		IEnumerable<(TKey Minimum, TKey Maximum)> IReadOnlyDictionary<(TKey Minimum, TKey Maximum), TValue>.Keys => Dictionary.Keys;

		/// <inheritdoc />
		IEnumerable<TValue> IReadOnlyDictionary<(TKey Minimum, TKey Maximum), TValue>.Values => Dictionary.Values;

		/// <inheritdoc cref="Dictionary{TKey, TValue}" />
		public int Count => Dictionary.Count;

		/// <inheritdoc />
		public IEnumerator<KeyValuePair<(TKey Minimum, TKey Maximum), TValue>> GetEnumerator() { return Dictionary.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc cref="Dictionary{TKey,TValue}" />
		public bool ContainsKey((TKey Minimum, TKey Maximum) key) { return Dictionary.ContainsKey(key); }

		public bool ContainsKey(TKey key)
		{
			Dictionary<(TKey, TKey), TValue>.KeyCollection keys = Dictionary.Keys;

			lock(keys)
			{
				return keys.Any(e => Comparer.Equals(e, key));
			}
		}

		public (TKey Minimum, TKey Maximum) GetKey(TKey key)
		{
			if (!TryGetKey(key, out (TKey Minimum, TKey Maximum) tuple)) throw new KeyNotFoundException();
			return tuple;
		}

		public bool TryGetKey(TKey lookup, out (TKey Minimum, TKey Maximum) key)
		{
			bool found = false;
			Dictionary<(TKey, TKey), TValue>.KeyCollection keys = Dictionary.Keys;

			lock(keys)
			{
				key = default((TKey, TKey));

				foreach ((TKey, TKey) tuple in keys.Where(tuple => Comparer.Equals(tuple, lookup)))
				{
					key = tuple;
					found = true;
					break;
				}
			}

			return found;
		}

		/// <inheritdoc cref="Dictionary{TKey,TValue}" />
		public bool TryGetValue((TKey Minimum, TKey Maximum) key, out TValue value) { return Dictionary.TryGetValue(key, out value); }

		public bool TryGetValue(TKey key, out TValue value)
		{
			value = default(TValue);
			return TryGetKey(key, out (TKey, TKey) tuple) && Dictionary.TryGetValue(tuple, out value);
		}

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { DictionaryAsCollection.CopyTo(array, index); }
	}
}