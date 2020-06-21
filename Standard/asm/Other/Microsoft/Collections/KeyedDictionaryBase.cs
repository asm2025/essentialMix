using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using asm.Exceptions.Collections;
using asm.Extensions;
using JetBrains.Annotations;
using asmMath = asm.Numeric.Math;

namespace asm.Other.Microsoft.Collections
{
	// based on https://github.com/microsoft/referencesource/blob/master/mscorlib/system/collections/generic/dictionary.cs
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(asm_Mscorlib_DictionaryDebugView<,>))]
	[Serializable]
	[ComVisible(false)]
	public abstract class KeyedDictionaryBase<TKey, TValue> : ICollection<TValue>, ICollection, IReadOnlyDictionary<TKey, TValue>, ISerializable, IDeserializationCallback
	{
		protected struct Entry
		{
			public int HashCode; // Lower 31 bits of hash code, -1 if unused
			public int Next; // Index of next entry, -1 if last
			public TKey Key; // Key of entry
			public TValue Value; // Value of entry
		}

		[DebuggerDisplay("Count = {Count}")]
		[DebuggerTypeProxy(typeof(asm_Mscorlib_DictionaryKeyCollectionDebugView<,>))]
		[Serializable]
		public sealed class KeyCollection : ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
		{
			/// <inheritdoc cref="IEnumerator{T}" />
			[Serializable]
			// ReSharper disable once MemberHidesStaticFromOuterClass
			public struct Enumerator : IEnumerator<TKey>, IEnumerator
			{
				private KeyedDictionaryBase<TKey, TValue> _dictionary;
				private int _index;
				private int _version;

				internal Enumerator([NotNull] KeyedDictionaryBase<TKey, TValue> dictionary)
				{
					_dictionary = dictionary;
					_version = dictionary._version;
					_index = 0;
					Current = default(TKey);
				}

				public TKey Current { get; private set; }

				object IEnumerator.Current
				{
					get
					{
						if (_index == 0 || _index == _dictionary._count + 1) throw new InvalidOperationException();
						return Current;
					}
				}

				public void Dispose() { }

				public bool MoveNext()
				{
					if (_version != _dictionary._version) throw new VersionChangedException();

					while ((uint)_index < (uint)_dictionary._count)
					{
						if (_dictionary._entries[_index].HashCode >= 0)
						{
							Current = _dictionary._entries[_index].Key;
							_index++;
							return true;
						}

						_index++;
					}

					_index = _dictionary._count + 1;
					Current = default(TKey);
					return false;
				}

				void IEnumerator.Reset()
				{
					if (_version != _dictionary._version) throw new VersionChangedException();
					_index = 0;
					Current = default(TKey);
				}
			}

			private readonly KeyedDictionaryBase<TKey, TValue> _dictionary;

			public KeyCollection([NotNull] KeyedDictionaryBase<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
			}

			bool ICollection.IsSynchronized => false;

			object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

			public int Count => _dictionary.Count;

			bool ICollection<TKey>.IsReadOnly => true;

			IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(_dictionary); }

			IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() { return new Enumerator(_dictionary); }

			public Enumerator GetEnumerator() { return new Enumerator(_dictionary); }

			void ICollection<TKey>.Add(TKey item) { throw new NotSupportedException(); }

			bool ICollection<TKey>.Remove(TKey item) { throw new NotSupportedException(); }

			void ICollection<TKey>.Clear() { throw new NotSupportedException(); }

			bool ICollection<TKey>.Contains(TKey item) { return !ReferenceEquals(item, null) && _dictionary.ContainsKey(item); }

			void ICollection.CopyTo(Array array, int index)
			{
				if (array.Rank != 1) throw new RankException();
				if (array.GetLowerBound(0) != 0) throw new ArgumentException("Lower bound is larger than zero.");
				if (!index.InRangeRx(0, array.Length)) throw new ArgumentOutOfRangeException(nameof(index));
				if (array.Length - index < _dictionary.Count) throw new ArgumentException("Array is too small to hold all the values.", nameof(array));

				if (array is TKey[] keys)
				{
					CopyTo(keys, index);
				}
				else
				{
					object[] objects = array as object[] ?? throw new ArrayTypeMismatchException();
					int count = _dictionary._count;
					Entry[] entries = _dictionary._entries;

					for (int i = 0; i < count; i++)
					{
						if (entries[i].HashCode < 0) continue;
						objects[index++] = entries[i].Key;
					}
				}
			}

			public void CopyTo(TKey[] array, int index)
			{
				if (!index.InRangeRx(0, array.Length)) throw new ArgumentOutOfRangeException(nameof(index));
				if (array.Length - index < _dictionary.Count) throw new ArgumentException("Array is too small to hold all the values.", nameof(array));

				int count = _dictionary._count;
				Entry[] entries = _dictionary._entries;

				for (int i = 0; i < count; i++)
				{
					if (entries[i].HashCode < 0) continue;
					array[index++] = entries[i].Key;
				}
			}
		}

		[DebuggerDisplay("Count = {Count}")]
		[DebuggerTypeProxy(typeof(asm_Mscorlib_DictionaryValueCollectionDebugView<,>))]
		[Serializable]
		public sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
		{
			/// <inheritdoc cref="IEnumerator{T}" />
			[Serializable]
			// ReSharper disable once MemberHidesStaticFromOuterClass
			public struct Enumerator : IEnumerator<TValue>, IEnumerator
			{
				private KeyedDictionaryBase<TKey, TValue> _dictionary;
				private int _index;
				private int _version;

				internal Enumerator([NotNull] KeyedDictionaryBase<TKey, TValue> dictionary)
				{
					_dictionary = dictionary;
					_version = dictionary._version;
					_index = 0;
					Current = default(TValue);
				}

				public TValue Current { get; private set; }

				object IEnumerator.Current
				{
					get
					{
						if (_index == 0 || _index == _dictionary._count + 1) throw new InvalidOperationException();
						return Current;
					}
				}

				public void Dispose() { }

				public bool MoveNext()
				{
					if (_version != _dictionary._version) throw new InvalidOperationException();

					while ((uint)_index < (uint)_dictionary._count)
					{
						if (_dictionary._entries[_index].HashCode >= 0)
						{
							Current = _dictionary._entries[_index].Value;
							_index++;
							return true;
						}

						_index++;
					}

					_index = _dictionary._count + 1;
					Current = default(TValue);
					return false;
				}

				void IEnumerator.Reset()
				{
					if (_version != _dictionary._version) throw new VersionChangedException();
					_index = 0;
					Current = default(TValue);
				}
			}

			private KeyedDictionaryBase<TKey, TValue> _dictionary;

			public ValueCollection([NotNull] KeyedDictionaryBase<TKey, TValue> dictionary)
			{
				_dictionary = dictionary;
			}

			bool ICollection.IsSynchronized => false;

			object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

			public int Count => _dictionary.Count;

			bool ICollection<TValue>.IsReadOnly => true;

			IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(_dictionary); }

			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() { return new Enumerator(_dictionary); }

			public Enumerator GetEnumerator() { return new Enumerator(_dictionary); }

			void ICollection<TValue>.Add(TValue item) { throw new NotSupportedException(); }

			bool ICollection<TValue>.Remove(TValue item) { throw new NotSupportedException(); }

			void ICollection<TValue>.Clear() { throw new NotSupportedException(); }

			bool ICollection<TValue>.Contains(TValue item) { return _dictionary.Contains(item); }

			public void CopyTo(TValue[] array, int index)
			{
				if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(nameof(index));
				if (array.Length - index < _dictionary.Count) throw new ArgumentException("Array is too small to hold all the values.", nameof(array));

				int count = _dictionary._count;
				Entry[] entries = _dictionary._entries;

				for (int i = 0; i < count; i++)
				{
					if (entries[i].HashCode < 0) continue;
					array[index++] = entries[i].Value;
				}
			}

			void ICollection.CopyTo(Array array, int index)
			{
				if (array.Rank != 1) throw new RankException();
				if (array.GetLowerBound(0) != 0) throw new ArgumentException("Lower bound is larger than zero.");
				if (!index.InRangeRx(0, array.Length)) throw new ArgumentOutOfRangeException(nameof(index));
				if (array.Length - index < _dictionary.Count) throw new ArgumentException("Array is too small to hold all the values.", nameof(array));

				if (array is TValue[] values) CopyTo(values, index);
				else
				{
					object[] objects = array as object[] ?? throw new ArrayTypeMismatchException();
					int count = _dictionary._count;
					Entry[] entries = _dictionary._entries;

					for (int i = 0; i < count; i++)
					{
						if (entries[i].HashCode < 0) continue;
						objects[index++] = entries[i].Value;
					}
				}
			}
		}

		[Serializable]
		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
		{
			internal const int DICT_ENTRY = 1;
			internal const int KEY_VALUE_PAIR = 2;

			private readonly KeyedDictionaryBase<TKey, TValue> _dictionary;
			private readonly int _version;

			private int _index;
			private KeyValuePair<TKey, TValue> _current;
			private int _getEnumeratorRetType; // What should Enumerator.Current return?

			internal Enumerator([NotNull] KeyedDictionaryBase<TKey, TValue> dictionary, int getEnumeratorRetType)
			{
				_dictionary = dictionary;
				_version = dictionary._version;
				_index = 0;
				_getEnumeratorRetType = getEnumeratorRetType;
				_current = new KeyValuePair<TKey, TValue>();
			}

			DictionaryEntry IDictionaryEnumerator.Entry
			{
				get
				{
					if (_index == 0 || _index == _dictionary._count + 1) throw new InvalidOperationException();
					return new DictionaryEntry(_current.Key, _current.Value);
				}
			}

			object IDictionaryEnumerator.Key
			{
				get
				{
					if (_index == 0 || _index == _dictionary._count + 1) throw new InvalidOperationException();
					return _current.Key;
				}
			}

			object IDictionaryEnumerator.Value
			{
				get
				{
					if (_index == 0 || _index == _dictionary._count + 1) throw new InvalidOperationException();
					return _current.Value;
				}
			}

			public KeyValuePair<TKey, TValue> Current => _current;

			[NotNull]
			object IEnumerator.Current
			{
				get
				{
					if (_index == 0 || _index == _dictionary._count + 1) throw new InvalidOperationException();
					return _getEnumeratorRetType == DICT_ENTRY
								? (object)new DictionaryEntry(_current.Key, _current.Value)
								: new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
				}
			}

			public void Dispose() { }

			public bool MoveNext()
			{
				if (_version != _dictionary._version) throw new VersionChangedException();

				// Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
				// dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
				while ((uint)_index < (uint)_dictionary._count)
				{
					if (_dictionary._entries[_index].HashCode >= 0)
					{
						_current = new KeyValuePair<TKey, TValue>(_dictionary._entries[_index].Key, _dictionary._entries[_index].Value);
						_index++;
						return true;
					}

					_index++;
				}

				_index = _dictionary._count + 1;
				_current = new KeyValuePair<TKey, TValue>();
				return false;
			}

			void IEnumerator.Reset()
			{
				if (_version != _dictionary._version) throw new VersionChangedException();
				_index = 0;
				_current = new KeyValuePair<TKey, TValue>();
			}
		}

		// constants for serialization
		private const string HASH_SIZE_NAME = "HashSize"; // Must save buckets.Length
		private const string KEY_VALUE_PAIRS_NAME = "KeyValuePairs";

		private int[] _buckets;
		private Entry[] _entries;
		private int _count;
		private int _version;
		private int _freeList;
		private int _freeCount;
		private KeyCollection _keys;
		private ValueCollection _values;
		private object _syncRoot;

		protected KeyedDictionaryBase()
			: this(0, null)
		{
		}

		protected KeyedDictionaryBase(int capacity)
			: this(capacity, null)
		{
		}

		protected KeyedDictionaryBase(IEqualityComparer<TKey> comparer)
			: this(0, comparer)
		{
		}

		protected KeyedDictionaryBase(int capacity, IEqualityComparer<TKey> comparer)
		{
			if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			if (capacity > 0) Initialize(capacity);
			Comparer = comparer ?? EqualityComparer<TKey>.Default;
		}

		protected KeyedDictionaryBase([NotNull] IEnumerable<TValue> collection)
			: this(collection, null)
		{
		}

		protected KeyedDictionaryBase([NotNull] IEnumerable<TValue> collection, IEqualityComparer<TKey> comparer)
			: this(comparer)
		{
			foreach (TValue item in collection) 
				Add(item);
		}

		protected KeyedDictionaryBase(SerializationInfo info, StreamingContext context)
		{
			//We can't do anything with the keys and values until the entire graph has been deserialized
			//and we have a reasonable estimate that GetHashCode is not going to fail. For the time being,
			//we'll just cache this. The graph is not valid until OnDeserialization has been called.
			HashCodeHelper.SerializationInfoTable.Add(this, info);
		}

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		public TValue this[[NotNull] TKey key]
		{
			get
			{
				int i = FindEntry(key);
				if (i < 0) throw new KeyNotFoundException();
				return _entries[i].Value;
			}
			set
			{
				int i = FindEntry(key);
				if (i < 0) throw new KeyNotFoundException();
				TKey k = GetKeyForItem(_entries[i].Value);
				if (!Comparer.Equals(k, key)) throw new InvalidOperationException();
				Insert(k, value, false);
			}
		}

		public int Count => _count - _freeCount;

		public IEqualityComparer<TKey> Comparer { get; private set; }

		public bool IsReadOnly => false;

		public bool IsFixedSize => false;

		[NotNull]
		public KeyCollection Keys => _keys ??= new KeyCollection(this);

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

		[NotNull]
		public ValueCollection Values => _values ??= new ValueCollection(this);

		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

		public virtual void OnDeserialization(object sender)
		{
			HashCodeHelper.SerializationInfoTable.TryGetValue(this, out SerializationInfo siInfo);

			if (siInfo == null)
			{
				// It might be necessary to call OnDeserialization from a container if the container object also implements
				// OnDeserialization. However, remoting will call OnDeserialization again.
				// We can return immediately if this function is called twice. 
				// Note we set remove the serialization info from the table at the end of this method.
				return;
			}

			int realVersion = siInfo.GetInt32(nameof(_version));
			int hashSize = siInfo.GetInt32(HASH_SIZE_NAME);
			Comparer = (IEqualityComparer<TKey>)siInfo.GetValue(nameof(Comparer), typeof(IEqualityComparer<TKey>));

			if (hashSize != 0)
			{
				_buckets = new int[hashSize];

				for (int i = 0; i < _buckets.Length; i++) _buckets[i] = -1;

				_entries = new Entry[hashSize];
				_freeList = -1;

				KeyValuePair<TKey, TValue>[] array = (KeyValuePair<TKey, TValue>[])siInfo.GetValue(KEY_VALUE_PAIRS_NAME, typeof(KeyValuePair<TKey, TValue>[]));
				if (array == null) throw new SerializationException("Missing keys.");

				foreach (KeyValuePair<TKey, TValue> pair in array)
				{
					if (ReferenceEquals(pair.Key, null)) throw new SerializationException("Null key found");
					Insert(pair.Key, pair.Value, true);
				}
			}
			else
			{
				_buckets = null;
			}

			_version = realVersion;
			HashCodeHelper.SerializationInfoTable.Remove(this);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		[SecurityCritical]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(_version), _version);
			info.AddValue(nameof(Comparer), Comparer, typeof(IEqualityComparer<TKey>));
			info.AddValue(HASH_SIZE_NAME, _buckets?.Length ?? 0); //This is the length of the bucket array.
			if (_buckets == null) return;
			KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[Count];
			CopyTo(array, 0);
			info.AddValue(KEY_VALUE_PAIRS_NAME, array, typeof(KeyValuePair<TKey, TValue>[]));
		}

		public IEnumerator<TValue> GetEnumerator() { return Values.GetEnumerator(); }

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() { return new Enumerator(this, Enumerator.KEY_VALUE_PAIR); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public void Add(TValue value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
			Insert(GetKeyForItem(value), value, true);
		}

		public bool Remove(TValue value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
			return RemoveByKey(GetKeyForItem(value));
		}

		public virtual bool RemoveByKey([NotNull] TKey key)
		{
			if (_buckets == null) return false;

			int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
			int bucket = hashCode % _buckets.Length;
			int last = -1;

			for (int i = _buckets[bucket]; i >= 0; last = i, i = _entries[i].Next)
			{
				if (_entries[i].HashCode != hashCode || !Comparer.Equals(_entries[i].Key, key)) continue;

				if (last < 0) _buckets[bucket] = _entries[i].Next;
				else _entries[last].Next = _entries[i].Next;

				Entry entry = _entries[i];
				entry.HashCode = -1;
				entry.Next = _freeList;
				entry.Key = default(TKey);
				_entries[i] = entry;
				_freeList = i;
				_freeCount++;
				_version++;
				entry.Value = default(TValue);
				return true;
			}

			return false;
		}

		public virtual void Clear()
		{
			if (_count == 0) return;

			for (int i = 0; i < _buckets.Length; i++) 
				_buckets[i] = -1;

			Array.Clear(_entries, 0, _count);
			_freeList = -1;
			_count = 0;
			_freeCount = 0;
			_version++;
		}

		public bool ContainsKey([NotNull] TKey key) { return FindEntry(key) >= 0; }

		public bool Contains(TValue value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
			int i = FindEntry(GetKeyForItem(value));
			return i >= 0 && EqualityComparer<TValue>.Default.Equals(_entries[i].Value, value);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			int i = FindEntry(key);

			if (i >= 0)
			{
				value = _entries[i].Value;
				return true;
			}

			value = default(TValue);
			return false;
		}

		public void CopyTo(TValue[] array, int index) { Values.CopyTo(array, index); }

		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Lower bound is larger than zero.");
			if (!index.InRangeRx(0, array.Length)) throw new ArgumentOutOfRangeException(nameof(index));
			if (array.Length - index < Count) throw new ArgumentException("Array is too small to hold all the values.", nameof(array));

			switch (array)
			{
				case KeyValuePair<TKey, TValue>[] pairs:
					CopyTo(pairs, index);
					break;
				case DictionaryEntry[] dictionaryEntries:
				{
					DictionaryEntry[] dictEntryArray = dictionaryEntries;
					Entry[] entries = _entries;

					for (int i = 0; i < _count; i++)
					{
						if (entries[i].HashCode < 0) continue;
						dictEntryArray[index++] = new DictionaryEntry(entries[i].Key, entries[i].Value);
					}

					break;
				}
				default:
				{
					if (!(array is object[] objects)) throw new ArrayTypeMismatchException();

					int count = _count;
					Entry[] entries = _entries;

					for (int i = 0; i < count; i++)
					{
						if (entries[i].HashCode < 0) continue;
						objects[index++] = new KeyValuePair<TKey, TValue>(entries[i].Key, entries[i].Value);
					}

					break;
				}
			}
		}

		protected virtual void Insert([NotNull] TKey key, TValue value, bool add)
		{
			if (_buckets == null) Initialize(0);
			
			int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;
			int targetBucket = hashCode % _buckets.Length;

			for (int i = _buckets[targetBucket]; i >= 0; i = _entries[i].Next)
			{
				if (_entries[i].HashCode != hashCode || !Comparer.Equals(_entries[i].Key, key)) continue;
				if (add) throw new DuplicateKeyException();
				_entries[i].Value = value;
				_version++;
				return;
			}

			int index;

			if (_freeCount > 0)
			{
				index = _freeList;
				_freeList = _entries[index].Next;
				_freeCount--;
			}
			else
			{
				if (_count == _entries.Length)
				{
					Resize();
					targetBucket = hashCode % _buckets.Length;
				}

				index = _count;
				_count++;
			}

			_entries[index].HashCode = hashCode;
			_entries[index].Next = _buckets[targetBucket];
			_entries[index].Key = key;
			_entries[index].Value = value;
			_buckets[targetBucket] = index;
			_version++;
		}

		[NotNull]
		protected abstract TKey GetKeyForItem([NotNull] TValue item);

		// This is a convenience method for the internal callers that were converted from using Hashtable.
		// Many were combining key doesn't exist and key exists but null value (for non-value types) checks.
		// This allows them to continue getting that behavior with minimal code delta. This is basically
		// TryGetValue without the out param
		internal TValue GetValueOrDefault([NotNull] TKey key)
		{
			int i = FindEntry(key);
			return i >= 0
						? _entries[i].Value
						: default(TValue);
		}

		private void CopyTo([NotNull] KeyValuePair<TKey, TValue>[] array, int index)
		{
			if (!index.InRangeRx(0, array.Length)) throw new ArgumentOutOfRangeException(nameof(index));
			if (array.Length - index < Count) throw new ArgumentException("Array is too small to hold all the values.", nameof(array));

			int count = _count;
			Entry[] entries = _entries;

			for (int i = 0; i < count; i++)
			{
				if (entries[i].HashCode < 0) continue;
				array[index++] = new KeyValuePair<TKey, TValue>(entries[i].Key, entries[i].Value);
			}
		}

		private int FindEntry([NotNull] TKey key)
		{
			if (_buckets == null) return -1;

			int hashCode = Comparer.GetHashCode(key) & 0x7FFFFFFF;

			for (int i = _buckets[hashCode % _buckets.Length]; i >= 0; i = _entries[i].Next)
			{
				if (_entries[i].HashCode != hashCode || !Comparer.Equals(_entries[i].Key, key)) continue;
				return i;
			}

			return -1;
		}

		private void Initialize(int capacity)
		{
			int size = asmMath.GetPrime(capacity);
			_buckets = new int[size];

			for (int i = 0; i < _buckets.Length; i++) _buckets[i] = -1;

			_entries = new Entry[size];
			_freeList = -1;
		}

		private void Resize()
		{
			int newSize = asmMath.ExpandPrime(_count);
			if (newSize <= _count) throw new InvalidOperationException("Cannot expand the set any further.");
			Resize(newSize, false);
		}

		private void Resize(int newSize, bool forceNewHashCodes)
		{
			Contract.Assert(newSize >= _entries.Length);
			int[] newBuckets = new int[newSize];

			for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;

			Entry[] newEntries = new Entry[newSize];
			Array.Copy(_entries, 0, newEntries, 0, _count);

			if (forceNewHashCodes)
			{
				for (int i = 0; i < _count; i++)
				{
					if (newEntries[i].HashCode != -1)
						newEntries[i].HashCode = Comparer.GetHashCode(newEntries[i].Key) & 0x7FFFFFFF;
				}
			}

			for (int i = 0; i < _count; i++)
			{
				if (newEntries[i].HashCode < 0) continue;
				int bucket = newEntries[i].HashCode % newSize;
				newEntries[i].Next = newBuckets[bucket];
				newBuckets[bucket] = i;
			}

			_buckets = newBuckets;
			_entries = newEntries;
		}
	}
}