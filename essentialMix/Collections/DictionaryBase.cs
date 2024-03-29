﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using essentialMix.Numeric;
using JetBrains.Annotations;
using Other.Microsoft;

namespace essentialMix.Collections;

// based on https://github.com/microsoft/referencesource/blob/master/mscorlib/system/collections/generic/dictionary.cs
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_DictionaryDebugView<,>))]
[Serializable]
[ComVisible(false)]
public abstract class DictionaryBase<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>, ISerializable, IDeserializationCallback
{
	// constants for serialization
	private const string HASH_SIZE_NAME = "HashSize"; // Must save buckets.Length
	private const string ITEMS_NAME = "KeyValuePairs";

	protected struct Entry
	{
		public int HashCode; // Lower 31 bits of hash code, -1 if unused
		public int Next; // Index of next entry, -1 if last
		public TKey Key; // Key of entry
		public TValue Value; // Value of entry
	}

	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(Dbg_DictionaryKeyCollectionDebugView<,>))]
	[Serializable]
	public sealed class KeyCollection : ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
	{
		/// <inheritdoc cref="IEnumerator{T}" />
		[Serializable]
		// ReSharper disable once MemberHidesStaticFromOuterClass
		private struct Enumerator : IEnumerator<TKey>, IEnumerator
		{
			private readonly DictionaryBase<TKey, TValue> _dictionary;
			private int _index;
			private int _version;

			internal Enumerator([NotNull] DictionaryBase<TKey, TValue> dictionary)
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

		private DictionaryBase<TKey, TValue> _dictionary;

		public KeyCollection([NotNull] DictionaryBase<TKey, TValue> dictionary)
		{
			_dictionary = dictionary;
		}

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

		public int Count => _dictionary.Count;

		bool ICollection<TKey>.IsReadOnly => true;

		IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(_dictionary); }

		IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() { return new Enumerator(_dictionary); }

		[NotNull]
		public IEnumerator<TKey> GetEnumerator() { return new Enumerator(_dictionary); }

		void ICollection<TKey>.Add(TKey item) { throw new NotSupportedException(); }

		bool ICollection<TKey>.Remove(TKey item) { throw new NotSupportedException(); }

		void ICollection<TKey>.Clear() { throw new NotSupportedException(); }

		bool ICollection<TKey>.Contains(TKey item) { return item is not null && _dictionary.ContainsKey(item); }

		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new LBoundLargerThanZeroException();
			if (!index.InRangeRx(0, array.Length)) throw new ArgumentOutOfRangeException(nameof(index));
			if (array.Length - index < _dictionary.Count) throw new ArrayTooSmallException();

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
			if (array.Length - index < _dictionary.Count) throw new ArrayTooSmallException();

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
	[DebuggerTypeProxy(typeof(Dbg_DictionaryValueCollectionDebugView<,>))]
	[Serializable]
	public sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
	{
		/// <inheritdoc cref="IEnumerator{T}" />
		[Serializable]
		// ReSharper disable once MemberHidesStaticFromOuterClass
		private struct Enumerator : IEnumerator<TValue>, IEnumerator
		{
			private DictionaryBase<TKey, TValue> _dictionary;
			private int _index;
			private int _version;

			internal Enumerator([NotNull] DictionaryBase<TKey, TValue> dictionary)
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

		private DictionaryBase<TKey, TValue> _dictionary;

		public ValueCollection([NotNull] DictionaryBase<TKey, TValue> dictionary)
		{
			_dictionary = dictionary;
		}

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

		public int Count => _dictionary.Count;

		bool ICollection<TValue>.IsReadOnly => true;

		IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(_dictionary); }

		IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() { return new Enumerator(_dictionary); }

		[NotNull]
		public IEnumerator<TValue> GetEnumerator() { return new Enumerator(_dictionary); }

		void ICollection<TValue>.Add(TValue item) { throw new NotSupportedException(); }

		bool ICollection<TValue>.Remove(TValue item) { throw new NotSupportedException(); }

		void ICollection<TValue>.Clear() { throw new NotSupportedException(); }

		bool ICollection<TValue>.Contains(TValue item) { return _dictionary.ContainsValue(item); }

		public void CopyTo(TValue[] array, int index)
		{
			if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(nameof(index));
			if (array.Length - index < _dictionary.Count) throw new ArrayTooSmallException();

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
			if (array.GetLowerBound(0) != 0) throw new LBoundLargerThanZeroException();
			if (!index.InRangeRx(0, array.Length)) throw new ArgumentOutOfRangeException(nameof(index));
			if (array.Length - index < _dictionary.Count) throw new ArrayTooSmallException();

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
	private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
	{
		internal const int DICT_ENTRY = 1;
		internal const int KEY_VALUE_PAIR = 2;

		private readonly DictionaryBase<TKey, TValue> _dictionary;
		private readonly int _version;

		private int _index;
		private KeyValuePair<TKey, TValue> _current;
		private int _getEnumeratorRetType; // What should Enumerator.Current return?

		internal Enumerator([NotNull] DictionaryBase<TKey, TValue> dictionary, int getEnumeratorRetType)
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
							? new DictionaryEntry(_current.Key, _current.Value)
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

	protected internal int _version;

	private int[] _buckets;
	private Entry[] _entries;
	private int _count;
	private int _freeList;
	private int _freeCount;
	private KeyCollection _keys;
	private ValueCollection _values;

	[NonSerialized]
	private object _syncRoot;

	protected DictionaryBase()
		: this(0, null)
	{
	}

	protected DictionaryBase(int capacity)
		: this(capacity, null)
	{
	}

	protected DictionaryBase(IEqualityComparer<TKey> comparer)
		: this(0, comparer)
	{
	}

	protected DictionaryBase(int capacity, IEqualityComparer<TKey> comparer)
	{
		if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
		if (capacity > 0) Initialize(capacity);
		Comparer = comparer ?? EqualityComparer<TKey>.Default;
	}

	protected DictionaryBase([NotNull] IDictionary<TKey, TValue> dictionary)
		: this(dictionary, null)
	{
	}

	protected DictionaryBase([NotNull] IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
		: this(dictionary.Count, comparer)
	{
		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			Add(pair.Key, pair.Value);
	}

	protected DictionaryBase(SerializationInfo info, StreamingContext context)
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

	public TValue this[TKey key]
	{
		get
		{
			int i = FindEntry(key);
			if (i < 0) throw new KeyNotFoundException();
			return _entries[i].Value;
		}
		set => Insert(key, value, false);
	}

	public int Count => _count - _freeCount;

	object IDictionary.this[object key]
	{
		get
		{
			if (!IsCompatibleKey(key)) return null;
			int i = FindEntry((TKey)key);
			return i >= 0
						? _entries[i].Value
						: null;
		}
		set => this[(TKey)key] = (TValue)value;
	}

	public IEqualityComparer<TKey> Comparer { get; private set; }

	public bool IsReadOnly => false;

	public bool IsFixedSize => false;

	[NotNull]
	public KeyCollection Keys => _keys ??= new KeyCollection(this);

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

	ICollection IDictionary.Keys => Keys;

	[NotNull]
	public ValueCollection Values => _values ??= new ValueCollection(this);

	ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

	ICollection IDictionary.Values => Values;

	void IDeserializationCallback.OnDeserialization(object sender) { OnDeserialization(); }
	protected virtual void OnDeserialization()
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

		if (hashSize == 0)
		{
			_buckets = null;
			_version = realVersion;
			return;
		}

		_buckets = new int[hashSize];

		for (int i = 0; i < _buckets.Length; i++)
			_buckets[i] = -1;

		_entries = new Entry[hashSize];
		_freeList = -1;

		KeyValuePair<TKey, TValue>[] array = (KeyValuePair<TKey, TValue>[])siInfo.GetValue(ITEMS_NAME, typeof(KeyValuePair<TKey, TValue>[]));
		if (array == null) throw new SerializationDataMissingException();

		foreach (KeyValuePair<TKey, TValue> pair in array)
		{
			if (ReferenceEquals(pair.Key, null)) throw new NullKeyException();
			Insert(pair.Key, pair.Value, true);
		}

		_version = realVersion;
		HashCodeHelper.SerializationInfoTable.Remove(this);
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { GetObjectData(info, context); }
	[SecurityCritical]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue(nameof(_version), _version);
		info.AddValue(nameof(Comparer), Comparer, typeof(IEqualityComparer<TKey>));
		info.AddValue(HASH_SIZE_NAME, _buckets?.Length ?? 0); //This is the length of the bucket array.
		if (_buckets == null) return;
		KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[Count];
		CopyTo(array, 0);
		info.AddValue(ITEMS_NAME, array, typeof(KeyValuePair<TKey, TValue>[]));
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return new Enumerator(this, Enumerator.KEY_VALUE_PAIR); }

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	IDictionaryEnumerator IDictionary.GetEnumerator() { return new Enumerator(this, Enumerator.DICT_ENTRY); }

	public void Add(TKey key, TValue value) { Insert(key, value, true); }

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) { Add(keyValuePair.Key, keyValuePair.Value); }

	void IDictionary.Add(object key, object value) { Add((TKey)key, (TValue)value); }

	public virtual bool Remove(TKey key)
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
			entry.Value = default(TValue);
			_entries[i] = entry;
			_freeList = i;
			_freeCount++;
			_version++;
			return true;
		}

		return false;
	}

	public bool Remove(KeyValuePair<TKey, TValue> keyValuePair)
	{
		int i = FindEntry(keyValuePair.Key);
		if (i < 0 || !EqualityComparer<TValue>.Default.Equals(_entries[i].Value, keyValuePair.Value)) return false;
		Remove(keyValuePair.Key);
		return true;
	}

	void IDictionary.Remove(object key)
	{
		if (IsCompatibleKey(key)) Remove((TKey)key);
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

	public bool ContainsKey(TKey key) { return FindEntry(key) >= 0; }

	public bool ContainsValue(TValue value)
	{
		if (ReferenceEquals(value, null))
		{
			for (int i = 0; i < _count; i++)
			{
				if (_entries[i].HashCode >= 0 && ReferenceEquals(_entries[i].Value, null))
					return true;
			}
		}
		else
		{
			EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;

			for (int i = 0; i < _count; i++)
			{
				if (_entries[i].HashCode >= 0 && c.Equals(_entries[i].Value, value))
					return true;
			}
		}

		return false;
	}

	public bool Contains(KeyValuePair<TKey, TValue> keyValuePair)
	{
		int i = FindEntry(keyValuePair.Key);
		return i >= 0 && EqualityComparer<TValue>.Default.Equals(_entries[i].Value, keyValuePair.Value);
	}

	bool IDictionary.Contains(object key) { return IsCompatibleKey(key) && ContainsKey((TKey)key); }

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

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index) { CopyTo(array, index); }

	void ICollection.CopyTo(Array array, int index)
	{
		if (array.Rank != 1) throw new RankException();
		if (array.GetLowerBound(0) != 0) throw new LBoundLargerThanZeroException();
		if (!index.InRangeRx(0, array.Length)) throw new ArgumentOutOfRangeException(nameof(index));
		if (array.Length - index < Count) throw new ArrayTooSmallException();

		switch (array)
		{
			case KeyValuePair<TKey, TValue>[] pairs:
				CopyTo(pairs, index);
				break;
			case DictionaryEntry[] dictionaryEntries:
			{
				Entry[] entries = _entries;

				for (int i = 0; i < _count; i++)
				{
					if (entries[i].HashCode < 0) continue;
					dictionaryEntries[index++] = new DictionaryEntry(entries[i].Key, entries[i].Value);
				}

				break;
			}
			default:
			{
				if (array is not object[] objects) throw new ArrayTypeMismatchException();

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
		if (array.Length - index < Count) throw new ArrayTooSmallException();

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
		int size = Math2.GetPrime(capacity);
		_buckets = new int[size];

		for (int i = 0; i < _buckets.Length; i++) _buckets[i] = -1;

		_entries = new Entry[size];
		_freeList = -1;
	}

	private void Resize()
	{
		int newSize = Math2.ExpandPrime(_count);
		if (newSize <= _count) throw new InvalidOperationException("Cannot expand the set any further.");
		Resize(newSize, false);
	}

	private void Resize(int newSize, bool forceNewHashCodes)
	{
		Contract.Assert(newSize >= _entries.Length);
		int[] newBuckets = new int[newSize];

		for (int i = 0; i < newBuckets.Length; i++)
			newBuckets[i] = -1;

		Entry[] newEntries = new Entry[newSize];
		Array.Copy(_entries, 0, newEntries, 0, _count);

		if (forceNewHashCodes)
		{
			for (int i = 0; i < _count; i++)
				if (newEntries[i].HashCode != -1)
					newEntries[i].HashCode = Comparer.GetHashCode(newEntries[i].Key) & 0x7FFFFFFF;
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

	private static bool IsCompatibleKey(object key) { return key is TKey; }
}