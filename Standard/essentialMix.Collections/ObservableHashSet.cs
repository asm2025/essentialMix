using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Numeric;
using essentialMix.Threading;
using JetBrains.Annotations;
using Other.Microsoft.Collections;

namespace essentialMix.Collections;

// based on https://github.com/microsoft/referencesource/blob/master/System.Core/System/Collections/Generic/HashSet.cs
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_HashSetDebugView<>))]
[Serializable]
[HostProtection(MayLeakOnAbort = true)]
public class ObservableHashSet<T> : ISet<T>, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback, INotifyPropertyChanged, INotifyCollectionChanged
{
	private const string ITEMS_NAME = "Item[]";

	// store lower 31 bits of hash code
	private const int LOWER31_BIT_MASK = 0x7FFFFFFF;

	// cutoff point, above which we won't do stackallocs. This corresponds to 100 integers.
	private const int STACK_ALLOC_THRESHOLD = 100;

	// when constructing a hashset from an existing collection, it may contain duplicates, 
	// so this is used as the max acceptable excess ratio of capacity to count. Note that
	// this is only used on the ctor and not to automatically shrink if the hashset has, e.g,
	// a lot of adds followed by removes. Users must explicitly shrink by calling TrimExcess.
	// This is set to 3 because capacity is acceptable as 2x rounded up to nearest prime.
	private const int SHRINK_THRESHOLD = 3;
		
	private const string VERSION_NAME = "Version";
	private const string CAPACITY_NAME = "Capacity";

	[Serializable]
	[HostProtection(MayLeakOnAbort = true)]
	public struct Enumerator : IEnumerator<T>, IEnumerator
	{
		[NonSerialized]
		private readonly ObservableHashSet<T> _set;
		private readonly int _version;

		private int _index;

		internal Enumerator([NotNull] ObservableHashSet<T> set)
		{
			_set = set;
			_index = 0;
			_version = set._version;
			Current = default(T);
		}

		public T Current { get; private set; }

		object IEnumerator.Current
		{
			get
			{
				if (!_index.InRange(0, _set.Count)) throw new InvalidOperationException();
				return Current;
			}
		}

		public void Dispose() { }

		public bool MoveNext()
		{
			if (_version == _set._version && _index < _set.Count)
			{
				while (_index < _set._lastIndex)
				{
					if (_set._slots[_index].HashCode >= 0)
					{
						Current = _set._slots[_index].Value;
						_index++;
						return true;
					}

					_index++;
				}
			}
			return MoveNextRare();
		}

		private bool MoveNextRare()
		{
			if (_version != _set._version) throw new VersionChangedException();
			_index = _set._lastIndex + 1;
			Current = default(T);
			return false;
		}

		void IEnumerator.Reset()
		{
			if (_version != _set._version) throw new VersionChangedException();
			_index = 0;
			Current = default(T);
		}
	}

	// used for set checking operations (using enumerable) that rely on counting
	internal struct ElementCount
	{
		internal int UniqueCount;
		internal int UnfoundCount;
	}

	internal struct Slot
	{
		internal int HashCode; // Lower 31 bits of hash code, -1 if unused
		internal int Next; // Index of next entry, -1 if last
		internal T Value;
	}

	private int _version;
	private SimpleMonitor _monitor = new SimpleMonitor();

	private int[] _buckets;
	private Slot[] _slots;
	private int _lastIndex;
	private int _freeList;

	// temporary variable needed during deserialization
	private SerializationInfo _siInfo;

	public ObservableHashSet()
		: this((IEqualityComparer<T>)null)
	{
	}

	public ObservableHashSet(int capacity)
		: this(capacity, null)
	{
	}

	public ObservableHashSet(IEqualityComparer<T> comparer) 
	{
		Comparer = comparer ?? EqualityComparer<T>.Default;
		_freeList = -1;
	}

	public ObservableHashSet(int capacity, IEqualityComparer<T> comparer) 
		: this(comparer)
	{
		if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
		if (capacity > 0) Initialize(capacity);
	}

	public ObservableHashSet([NotNull] IEnumerable<T> enumerable)
		: this(enumerable, null)
	{
	}

	/// <summary>
	///     Implementation Notes:
	///     Since resizes are relatively expensive (require rehashing), this attempts to minimize
	///     the need to resize by setting the initial capacity based on size of collection.
	/// </summary>
	/// <param name="enumerable"></param>
	/// <param name="comparer"></param>
	public ObservableHashSet([NotNull] IEnumerable<T> enumerable, IEqualityComparer<T> comparer) 
		: this(comparer)
	{
		if (enumerable is ISet<T> otherAsHashSet && AreEqualityComparersEqual(this, otherAsHashSet))
		{
			CopyFrom(otherAsHashSet);
		}
		else
		{
			// to avoid excess resizes, first set size based on collection's count. Collection
			// may contain duplicates, so call TrimExcess if resulting hashset is larger than
			// threshold
			int suggestedCapacity = enumerable.FastCount().NotBelow(0);
			Initialize(suggestedCapacity);
			UnionWith(enumerable);
			if (Count > 0 && _slots.Length / Count > SHRINK_THRESHOLD) TrimExcess();
		}
	}

	protected ObservableHashSet(SerializationInfo info, StreamingContext context) 
	{
		// We can't do anything with the keys and values until the entire graph has been 
		// deserialized and we have a reasonable estimate that GetHashCode is not going to 
		// fail.  For the time being, we'll just cache this.  The graph is not valid until 
		// OnDeserialization has been called.
		_siInfo = info;
	}

	/// <summary>
	///     Gets the IEqualityComparer that is used to determine equality of keys for
	///     the HashSet.
	/// </summary>
	public IEqualityComparer<T> Comparer { get; private set; }

	/// <summary>
	///     Number of elements
	/// </summary>
	public int Count { get; private set; }

	/// <summary>
	///     Whether this is readonly
	/// </summary>
	bool ICollection<T>.IsReadOnly => false;

	protected bool SuppressCollectionEvents { get; set; }

	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add => PropertyChanged += value;
		remove => PropertyChanged -= value;
	}

	public event PropertyChangedEventHandler PropertyChanged;
	public event NotifyCollectionChangedEventHandler CollectionChanged;

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { GetObjectData(info, context); }
	[SecurityCritical]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null) throw new ArgumentNullException(nameof(info));

		// need to serialize version to avoid problems with serializing while enumerating
		info.AddValue(VERSION_NAME, _version);
		info.AddValue(nameof(Comparer), Comparer, typeof(IEqualityComparer<T>));
		info.AddValue(CAPACITY_NAME, _buckets?.Length ?? 0);

		if (_buckets == null) return;
		T[] array = new T[Count];
		CopyTo(array);
		info.AddValue(ITEMS_NAME, array, typeof(T[]));
	}

	void IDeserializationCallback.OnDeserialization(object sender) { OnDeserialization(); }
	protected virtual void OnDeserialization()
	{
		if (_siInfo == null)
		{
			// It might be necessary to call OnDeserialization from a container if the 
			// container object also implements OnDeserialization. However, remoting will 
			// call OnDeserialization again. We can return immediately if this function is 
			// called twice. Note we set _siInfo to null at the end of this method.
			return;
		}

		int capacity = _siInfo.GetInt32(CAPACITY_NAME);
		Comparer = (IEqualityComparer<T>)_siInfo.GetValue(nameof(Comparer), typeof(IEqualityComparer<T>));
		_freeList = -1;

		if (capacity > 0)
		{
			_buckets = new int[capacity];
			_slots = new Slot[capacity];
			T[] array = (T[])_siInfo.GetValue(ITEMS_NAME, typeof(T[])) ?? throw new SerializationException("Missing keys");
			SuppressCollectionEvents = true;

			try
			{
				// there are no resizes here because we already set capacity above
				foreach (T item in array) 
					AddIfNotPresent(item);
			}
			finally
			{
				SuppressCollectionEvents = false;
			}

			OnPropertyChanged(nameof(Count));
			OnCollectionChanged();
		}
		else
		{
			_buckets = null;
		}

		_version = _siInfo.GetInt32(VERSION_NAME);
		_siInfo = null;
	}

	public IEnumerator<T> GetEnumerator() { return new Enumerator(this); }

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	/// <summary>
	///     Add item to this HashSet. Returns bool indicating whether item was added (won't be
	///     added if already present)
	/// </summary>
	/// <param name="item"></param>
	/// <returns>true if added, false if already present</returns>
	public bool Add(T item) { return AddIfNotPresent(item); }

	/// <summary>
	///     Add item to this hashset. This is the explicit implementation of the ICollection&lt;T&gt;
	///     interface. The other Add method returns bool indicating whether item was added.
	/// </summary>
	/// <param name="item">item to add</param>
	void ICollection<T>.Add(T item) { AddIfNotPresent(item); }

	/// <summary>
	///     Remove item from this hashset
	/// </summary>
	/// <param name="item">item to remove</param>
	/// <returns>true if removed; false if not (i.e. if the item wasn't in the HashSet)</returns>
	public bool Remove(T item)
	{
		CheckReentrancy();
		if (_buckets == null) return false;
			
		int hashCode = InternalGetHashCode(item);
		int bucket = hashCode % _buckets.Length;
		int last = -1;

		for (int i = _buckets[bucket] - 1; i >= 0; last = i, i = _slots[i].Next)
		{
			if (_slots[i].HashCode != hashCode || !Comparer.Equals(_slots[i].Value, item)) continue;
				
			if (last < 0)
			{
				// first iteration; update buckets
				_buckets[bucket] = _slots[i].Next + 1;
			}
			else
			{
				// subsequent iterations; update 'next' pointers
				_slots[last].Next = _slots[i].Next;
			}

			_slots[i].HashCode = -1;
			_slots[i].Value = default(T);
			_slots[i].Next = _freeList;

			Count--;
			_version++;

			if (Count == 0)
			{
				_lastIndex = 0;
				_freeList = -1;
			}
			else
			{
				_freeList = i;
			}

			if (SuppressCollectionEvents) return true;
			OnPropertyChanged(nameof(Count));
			OnCollectionChanged();
			return true;
		}

		// either _buckets is null or wasn't found
		return false;
	}

	/// <summary>
	///     Remove all items from this set. This clears the elements but not the underlying
	///     buckets and slots array. Follow this call by TrimExcess to release these.
	/// </summary>
	public void Clear()
	{
		CheckReentrancy();
		if (_lastIndex <= 0) return;
		// clear the elements so that the gc can reclaim the references.
		// clear only up to _lastIndex for _slots 
		Array.Clear(_slots, 0, _lastIndex);
		Array.Clear(_buckets, 0, _buckets.Length);
		_lastIndex = 0;
		Count = 0;
		_freeList = -1;
		_version++;
		if (SuppressCollectionEvents) return;
		OnPropertyChanged(nameof(Count));
		OnCollectionChanged();
	}

	/// <summary>
	///     Remove elements that match specified predicate. Returns the number of elements removed
	/// </summary>
	/// <param name="match"></param>
	/// <returns></returns>
	public int RemoveWhere([NotNull] Predicate<T> match)
	{
		CheckReentrancy();
		int numRemoved = 0;
		SuppressCollectionEvents = true;

		try
		{
			for (int i = 0; i < _lastIndex; i++)
			{
				if (_slots[i].HashCode < 0) continue;
				
				// cache value in case delegate removes it
				T value = _slots[i].Value;

				if (!match(value)) continue;
				// check again that remove actually removed it
				if (Remove(value)) numRemoved++;
			}
		}
		finally
		{
			SuppressCollectionEvents = false;
		}

		if (numRemoved == 0) return 0;
		OnPropertyChanged(nameof(Count));
		OnCollectionChanged();
		return numRemoved;
	}

	/// <summary>
	///     Checks if this hashset contains the item
	/// </summary>
	/// <param name="item">item to check for containment</param>
	/// <returns>true if item contained; false if not</returns>
	public bool Contains(T item)
	{
		if (_buckets == null) return false;
			
		int hashCode = InternalGetHashCode(item);
			
		// see note at "HashSet" level describing why "- 1" appears in for loop
		for (int i = _buckets[hashCode % _buckets.Length] - 1; i >= 0; i = _slots[i].Next)
		{
			if (_slots[i].HashCode == hashCode && Comparer.Equals(_slots[i].Value, item))
				return true;
		}

		// either _buckets is null or wasn't found
		return false;
	}

	/// <summary>
	///     Copy items in this hashset to array, starting at arrayIndex
	/// </summary>
	/// <param name="array">array to add items to</param>
	/// <param name="arrayIndex">index to start at</param>
	public void CopyTo(T[] array, int arrayIndex) { CopyTo(array, arrayIndex, Count); }
	public void CopyTo([NotNull] T[] array) { CopyTo(array, 0, Count); }
	public void CopyTo([NotNull] T[] array, int arrayIndex, int count)
	{
		if (array == null) throw new ArgumentNullException(nameof(array));
		Count.ValidateRange(arrayIndex, ref count);

		int numCopied = 0;
			
		for (int i = 0; i < _lastIndex && numCopied < count; i++)
		{
			if (_slots[i].HashCode < 0) continue;
			array[arrayIndex + numCopied] = _slots[i].Value;
			numCopied++;
		}
	}

	/// <summary>
	///     Take the union of this HashSet with other. Modifies this set.
	///     Implementation note: GetSuggestedCapacity (to increase capacity in advance avoiding
	///     multiple resizes ended up not being useful in practice; quickly gets to the
	///     point where it's a wasteful check.
	/// </summary>
	/// <param name="other">enumerable with items to add</param>
	public void UnionWith(IEnumerable<T> other)
	{
		CheckReentrancy();

		bool added = false;
		SuppressCollectionEvents = true;

		try
		{
			foreach (T item in other) 
				added |= AddIfNotPresent(item);
		}
		finally
		{
			SuppressCollectionEvents = false;
		}

		if (!added) return;
		OnPropertyChanged(nameof(Count));
		OnCollectionChanged();
	}

	/// <summary>
	///     Takes the intersection of this set with other. Modifies this set.
	///     Implementation Notes:
	///     We get better perf if other is a hashset using same equality comparer, because we
	///     get constant contains check in other. Resulting cost is O(n1) to iterate over this.
	///     If we can't go above route, iterate over the other and mark intersection by checking
	///     contains in this. Then loop over and delete any unmarked elements. Total cost is n2+n1.
	///     Attempts to return early based on counts alone, using the property that the
	///     intersection of anything with the empty set is the empty set.
	/// </summary>
	/// <param name="other">enumerable with items to add </param>
	public void IntersectWith(IEnumerable<T> other)
	{
		// intersection of anything with empty set is empty set, so return if count is 0
		if (Count == 0) return;

		// if other is empty, intersection is empty set; remove all elements and we're done
		// can only figure this out if implements ICollection<T>. (IEnumerable<T> has no count)
		if (other is ICollection<T> otherAsCollection)
		{
			if (otherAsCollection.Count == 0)
			{
				Clear();
				return;
			}

			// faster if other is a hashset using same equality comparer; so check 
			// that other is a hashset using the same equality comparer.
			if (other is ISet<T> otherAsSet && AreEqualityComparersEqual(this, otherAsSet))
			{
				IntersectWithHashSetWithSameEc(otherAsSet);
				return;
			}
		}

		IntersectWithEnumerable(other);
	}

	/// <summary>
	///     Remove items in other from this set. Modifies this set.
	/// </summary>
	/// <param name="other">enumerable with items to remove</param>
	public void ExceptWith(IEnumerable<T> other)
	{
		// this is already the empty set; return
		if (Count == 0) return;

		// special case if other is this; a set minus itself is the empty set
		if (ReferenceEquals(other, this))
		{
			Clear();
			return;
		}

		bool removed = false;
		SuppressCollectionEvents = true;

		try
		{
			// remove every element in other from this
			foreach (T element in other) 
				removed |= Remove(element);
		}
		finally
		{
			SuppressCollectionEvents = false;
		}

		if (!removed) return;
		OnPropertyChanged(nameof(Count));
		OnCollectionChanged();
	}

	/// <summary>
	///     Takes symmetric difference (XOR) with other and this set. Modifies this set.
	/// </summary>
	/// <param name="other">enumerable with items to XOR</param>
	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		// if set is empty, then symmetric difference is other
		if (Count == 0)
		{
			UnionWith(other);
			return;
		}

		// special case this; the symmetric difference of a set with itself is the empty set
		if (ReferenceEquals(other, this))
		{
			Clear();
			return;
		}

		// If other is a HashSet, it has unique elements according to its equality comparer,
		// but if they're using different equality comparers, then assumption of uniqueness
		// will fail. So first check if other is a hashset using the same equality comparer;
		// symmetric except is a lot faster and avoids bit array allocations if we can assume
		// uniqueness
		if (other is ISet<T> otherAsSet && AreEqualityComparersEqual(this, otherAsSet)) SymmetricExceptWithUniqueHashSet(otherAsSet);
		else SymmetricExceptWithEnumerable(other);
	}

	/// <summary>
	///     Checks if this is a subset of other.
	///     Implementation Notes:
	///     The following properties are used up-front to avoid element-wise checks:
	///     1. If this is the empty set, then it's a subset of anything, including the empty set
	///     2. If other has unique elements according to this equality comparer, and this has more
	///     elements than other, then it can't be a subset.
	///     Furthermore, if other is a hashset using the same equality comparer, we can use a
	///     faster element-wise check.
	/// </summary>
	/// <param name="other"></param>
	/// <returns>true if this is a subset of other; false if not</returns>
	public bool IsSubsetOf(IEnumerable<T> other)
	{
		// The empty set is a subset of any set
		if (Count == 0) return true;

		// faster if other has unique elements according to this equality comparer; so check 
		// that other is a hashset using the same equality comparer.
		if (other is ISet<T> otherAsSet && AreEqualityComparersEqual(this, otherAsSet))
		{
			// if this has more elements then it can't be a subset
			// already checked that we're using same equality comparer. simply check that 
			// each element in this is contained in other.
			return Count <= otherAsSet.Count && IsSubsetOfHashSetWithSameEc(otherAsSet);
		}

		ElementCount result = CheckUniqueAndUnfoundElements(other, false);
		return result.UniqueCount == Count && result.UnfoundCount >= 0;
	}

	/// <summary>
	///     Checks if this is a proper subset of other (i.e. strictly contained in)
	///     Implementation Notes:
	///     The following properties are used up-front to avoid element-wise checks:
	///     1. If this is the empty set, then it's a proper subset of a set that contains at least
	///     one element, but it's not a proper subset of the empty set.
	///     2. If other has unique elements according to this equality comparer, and this has >=
	///     the number of elements in other, then this can't be a proper subset.
	///     Furthermore, if other is a hashset using the same equality comparer, we can use a
	///     faster element-wise check.
	/// </summary>
	/// <param name="other"></param>
	/// <returns>true if this is a proper subset of other; false if not</returns>
	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		if (other is ICollection<T> otherAsCollection)
		{
			// the empty set is a proper subset of anything but the empty set
			if (Count == 0) return otherAsCollection.Count > 0;

			// faster if other is a hashset (and we're using same equality comparer)
			if (other is ISet<T> otherAsSet && AreEqualityComparersEqual(this, otherAsSet))
			{
				// this has strictly less than number of items in other, so the following
				// check suffices for proper subset.
				return Count < otherAsSet.Count && IsSubsetOfHashSetWithSameEc(otherAsSet);
			}
		}

		ElementCount result = CheckUniqueAndUnfoundElements(other, false);
		return result.UniqueCount == Count && result.UnfoundCount > 0;
	}

	/// <summary>
	///     Checks if this is a superset of other
	///     Implementation Notes:
	///     The following properties are used up-front to avoid element-wise checks:
	///     1. If other has no elements (it's the empty set), then this is a superset, even if this
	///     is also the empty set.
	///     2. If other has unique elements according to this equality comparer, and this has less
	///     than the number of elements in other, then this can't be a superset
	/// </summary>
	/// <param name="other"></param>
	/// <returns>true if this is a superset of other; false if not</returns>
	public bool IsSupersetOf(IEnumerable<T> other)
	{
		// try to fall out early based on counts
		if (other is not ICollection<T> otherAsCollection) return ContainsAllElements(other);
		// if other is the empty set then this is a superset
		if (otherAsCollection.Count == 0) return true;

		// try to compare based on counts alone if other is a hashset with
		// same equality comparer
		if (other is not ISet<T> otherAsSet || !AreEqualityComparersEqual(this, otherAsSet)) return ContainsAllElements(other);
		return otherAsSet.Count <= Count && ContainsAllElements(other);
	}

	/// <summary>
	///     Checks if this is a proper superset of other (i.e. other strictly contained in this)
	///     Implementation Notes:
	///     This is slightly more complicated than above because we have to keep track if there
	///     was at least one element not contained in other.
	///     The following properties are used up-front to avoid element-wise checks:
	///     1. If this is the empty set, then it can't be a proper superset of any set, even if
	///     other is the empty set.
	///     2. If other is an empty set and this contains at least 1 element, then this is a proper
	///     superset.
	///     3. If other has unique elements according to this equality comparer, and others' count
	///     is greater than or equal to this count, then this can't be a proper superset
	///     Furthermore, if other has unique elements according to this equality comparer, we can
	///     use a faster element-wise check.
	/// </summary>
	/// <param name="other"></param>
	/// <returns>true if this is a proper superset of other; false if not</returns>
	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		// the empty set isn't a proper superset of any set.
		if (Count == 0) return false;

		if (other is ICollection<T> otherAsCollection)
		{
			// if other is the empty set then this is a superset
			if (otherAsCollection.Count == 0)
			{
				// note that this has at least one element, based on above check
				return true;
			}

			// faster if other is a hashset with the same equality comparer
			if (other is ISet<T> otherAsSet && AreEqualityComparersEqual(this, otherAsSet))
				return otherAsSet.Count < Count && ContainsAllElements(otherAsSet);
		}

		// couldn't fall out in the above cases; do it the long way
		ElementCount result = CheckUniqueAndUnfoundElements(other, true);
		return result.UniqueCount < Count && result.UnfoundCount == 0;
	}

	/// <summary>
	///     Checks if this set overlaps other (i.e. they share at least one item)
	/// </summary>
	/// <param name="other"></param>
	/// <returns>true if these have at least one common element; false if disjoint</returns>
	public bool Overlaps(IEnumerable<T> other)
	{
		if (Count == 0) return false;

		foreach (T item in other)
		{
			if (Contains(item)) return true;
		}

		return false;
	}

	/// <summary>
	///     Checks if this and other contain the same elements. This is set equality:
	///     duplicates and order are ignored
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public bool SetEquals(IEnumerable<T> other)
	{
		switch (other)
		{
			// faster if other is a hashset and we're using same equality comparer
			case ISet<T> otherAsSet when AreEqualityComparersEqual(this, otherAsSet):
			{
				// attempt to return early: since both contain unique elements, if they have 
				// different counts, then they can't be equal
				if (Count != otherAsSet.Count) return false;

				// already confirmed that the sets have the same number of distinct elements, so if
				// one is a superset of the other then they must be equal
				return ContainsAllElements(otherAsSet);
			}
			// if this count is 0 but other contains at least one element, they can't be equal
			case ICollection<T> otherAsCollection when Count == 0 && otherAsCollection.Count > 0:
				return false;
			default:
			{
				ElementCount result = CheckUniqueAndUnfoundElements(other, true);
				return result.UniqueCount == Count && result.UnfoundCount == 0;
			}
		}
	}

	/// <summary>
	///     Searches the set for a given value and returns the equal value it finds, if any.
	/// </summary>
	/// <param name="equalValue">The value to search for.</param>
	/// <param name="actualValue">
	///     The value from the set that the search found, or the default value of
	///     <typeparamref name="T" /> when the search yielded no match.
	/// </param>
	/// <returns>A value indicating whether the search was successful.</returns>
	/// <remarks>
	///     This can be useful when you want to reuse a previously stored reference instead of
	///     a newly constructed one (so that more sharing of references can occur) or to look up
	///     a value that has more complete data than the value you currently have, although their
	///     comparer functions indicate they are equal.
	/// </remarks>
	public bool TryGetValue(T equalValue, out T actualValue)
	{
		if (_buckets != null)
		{
			int i = InternalIndexOf(equalValue);

			if (i >= 0)
			{
				actualValue = _slots[i].Value;
				return true;
			}
		}

		actualValue = default(T);
		return false;
	}

	/// <summary>
	///     Sets the capacity of this list to the size of the list (rounded up to nearest prime),
	///     unless count is 0, in which case we release references.
	///     This method can be used to minimize a list's memory overhead once it is known that no
	///     new elements will be added to the list. To completely clear a list and release all
	///     memory referenced by the list, execute the following statements:
	///     list.Clear();
	///     list.TrimExcess();
	/// </summary>
	public void TrimExcess()
	{
		if (Count == 0)
		{
			// if count is zero, clear references
			_buckets = null;
			_slots = null;
			_version++;
		}
		else
		{
			// similar to IncreaseCapacity but moves down elements in case add/remove/etc
			// caused fragmentation
			int newSize = Math2.GetPrime(Count);
			Slot[] newSlots = new Slot[newSize];
			int[] newBuckets = new int[newSize];

			// move down slots and rehash at the same time. newIndex keeps track of current 
			// position in newSlots array
			int newIndex = 0;
				
			for (int i = 0; i < _lastIndex; i++)
			{
				if (_slots[i].HashCode < 0) continue;
				newSlots[newIndex] = _slots[i];

				// rehash
				int bucket = newSlots[newIndex].HashCode % newSize;
				newSlots[newIndex].Next = newBuckets[bucket] - 1;
				newBuckets[bucket] = newIndex + 1;

				newIndex++;
			}

			_lastIndex = newIndex;
			_slots = newSlots;
			_buckets = newBuckets;
			_freeList = -1;
		}
	}

	/// <summary>
	///     Adds value to HashSet if not contained already
	///     Returns true if added and false if already present
	/// </summary>
	/// <param name="value">value to find</param>
	/// <returns></returns>
	protected bool AddIfNotPresent(T value)
	{
		CheckReentrancy();
		if (_buckets == null) Initialize(0);

		int hashCode = InternalGetHashCode(value);
		int bucket = hashCode % _buckets.Length;

		for (int i = _buckets[hashCode % _buckets.Length] - 1; i >= 0; i = _slots[i].Next)
		{
			if (_slots[i].HashCode == hashCode && Comparer.Equals(_slots[i].Value, value)) return false;
		}

		int index;

		if (_freeList >= 0)
		{
			index = _freeList;
			_freeList = _slots[index].Next;
		}
		else
		{
			if (_lastIndex == _slots.Length)
			{
				IncreaseCapacity();
				// this will change during resize
				bucket = hashCode % _buckets.Length;
			}

			index = _lastIndex;
			_lastIndex++;
		}

		_slots[index].HashCode = hashCode;
		_slots[index].Value = value;
		_slots[index].Next = _buckets[bucket] - 1;
		_buckets[bucket] = index + 1;
		Count++;
		_version++;
		if (SuppressCollectionEvents) return true;
		OnPropertyChanged(nameof(Count));
		OnCollectionChanged();
		return true;
	}

	[NotifyPropertyChangedInvocator]
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		if (SuppressCollectionEvents || PropertyChanged == null) return;
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	protected virtual void OnPropertyChanged([NotNull] PropertyChangedEventArgs e)
	{
		if (SuppressCollectionEvents) return;
		PropertyChanged?.Invoke(this, e);
	}

	protected void OnCollectionChanged()
	{
		if (SuppressCollectionEvents || CollectionChanged == null) return;
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	protected virtual void OnCollectionChanged([NotNull] NotifyCollectionChangedEventArgs e)
	{
		if (SuppressCollectionEvents || CollectionChanged == null) return;

		using (BlockReentrancy())
		{
			CollectionChanged?.Invoke(this, e);
		}
	}

	protected IDisposable BlockReentrancy()
	{
		_monitor.Enter();
		return _monitor;
	}

	protected void CheckReentrancy()
	{
		if (!_monitor.Busy) return;
		// we can allow changes if there's only one listener - the problem
		// only arises if reentrant changes make the original event args
		// invalid for later listeners.  This keeps existing code working
		// (e.g. Selector.SelectedItems).
		if (CollectionChanged != null && CollectionChanged.GetInvocationList().Length > 1)
			throw new InvalidOperationException("Reentrancy not allowed.");
	}

	/// <summary>
	///     Copies this to an array. Used for DebugView
	/// </summary>
	/// <returns></returns>
	[NotNull]
	internal T[] ToArray()
	{
		T[] newArray = new T[Count];
		CopyTo(newArray);
		return newArray;
	}

	/// <summary>
	///     Initializes buckets and slots arrays. Uses suggested capacity by finding next prime
	///     greater than or equal to capacity.
	/// </summary>
	/// <param name="capacity"></param>
	private void Initialize(int capacity)
	{
		int size = Math2.GetPrime(capacity);
		_buckets = new int[size];
		_slots = new Slot[size];
	}

	// Initializes the HashSet from another HashSet with the same element type and
	// equality comparer.
	private void CopyFrom([NotNull] ISet<T> source)
	{
		int count = source.Count;

		if (count == 0)
		{
			// As well as short-circuiting on the rest of the work done,
			// this avoids errors from trying to access otherAsHashSet._buckets
			// or otherAsHashSet._slots when they aren't initialized.
			return;
		}

		bool added = false;
		SuppressCollectionEvents = true;

		try
		{
			if (source is ObservableHashSet<T> hashSetBase)
			{
				int capacity = source.Count;
				int threshold = Math2.ExpandPrime(count + 1);
				if (threshold <= count) throw new InvalidOperationException("Cannot expand the set any further.");

				if (threshold >= capacity)
				{
					_buckets = (int[])hashSetBase._buckets.Clone();
					_slots = (Slot[])hashSetBase._slots.Clone();
					_lastIndex = hashSetBase._lastIndex;
					_freeList = hashSetBase._freeList;
					added = true;
				}
				else
				{
					int lastIndex = hashSetBase._lastIndex;
					Slot[] slots = hashSetBase._slots;
					Initialize(count);
					
					int index = 0;

					for (int i = 0; i < lastIndex; ++i)
					{
						int hashCode = slots[i].HashCode;
						if (hashCode < 0) continue;
						AddValue(index, hashCode, slots[i].Value);
						added = true;
						++index;
					}

					_lastIndex = index;
				}

				Count = count;
			}
			else
			{
				foreach (T item in source)
					added |= AddIfNotPresent(item);
			}
		}
		finally
		{
			SuppressCollectionEvents = false;
		}

		if (!added) return;
		OnPropertyChanged(nameof(Count));
		OnCollectionChanged();
	}

	/// <summary>
	///     Expand to new capacity. New capacity is next prime greater than or equal to suggested
	///     size. This is called when the underlying array is filled. This performs no
	///     fragmentation, allowing faster execution; note that this is reasonable since
	///     AddIfNotPresent attempts to insert new elements in re-opened spots.
	/// </summary>
	private void IncreaseCapacity()
	{
		int newSize = Math2.ExpandPrime(Count);
		if (newSize <= Count) throw new InvalidOperationException("Cannot expand the set any further.");
		// Able to increase capacity; copy elements to larger array and rehash
		SetCapacity(newSize, false);
	}

	/// <summary>
	///     Set the underlying buckets array to size newSize and rehash.  Note that newSize
	///     *must* be a prime.  It is very likely that you want to call IncreaseCapacity()
	///     instead of this method.
	/// </summary>
	private void SetCapacity(int newSize, bool forceNewHashCodes)
	{
		Contract.Assert(Math2.IsPrime(newSize), "New size is not prime!");
		Contract.Assert(_buckets != null, "SetCapacity called on a set with no elements");

		Slot[] newSlots = new Slot[newSize];
		if (_slots != null) Array.Copy(_slots, 0, newSlots, 0, _lastIndex);

		if (forceNewHashCodes)
		{
			for (int i = 0; i < _lastIndex; i++)
			{
				if (newSlots[i].HashCode == -1) continue;
				newSlots[i].HashCode = InternalGetHashCode(newSlots[i].Value);
			}
		}

		int[] newBuckets = new int[newSize];
			
		for (int i = 0; i < _lastIndex; i++)
		{
			int bucket = newSlots[i].HashCode % newSize;
			newSlots[i].Next = newBuckets[bucket] - 1;
			newBuckets[bucket] = i + 1;
		}

		_slots = newSlots;
		_buckets = newBuckets;
	}

	// Add value at known index with known hash code. Used only
	// when constructing from another HashSet.
	private void AddValue(int index, int hashCode, T value)
	{
		int bucket = hashCode % _buckets.Length;
		_slots[index].HashCode = hashCode;
		_slots[index].Value = value;
		_slots[index].Next = _buckets[bucket] - 1;
		_buckets[bucket] = index + 1;
	}

	/// <summary>
	///     Checks if this contains of other elements. Iterates over other elements and
	///     returns false as soon as it finds an element in other that's not in this.
	///     Used by SupersetOf, ProperSupersetOf, and SetEquals.
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	private bool ContainsAllElements([NotNull] IEnumerable<T> other)
	{
		foreach (T item in other)
		{
			if (!Contains(item)) return false;
		}

		return true;
	}

	/// <summary>
	///     Implementation Notes:
	///     If other is a hashset and is using same equality comparer, then checking subset is
	///     faster. Simply check that each element in this is in other.
	///     Note: if other doesn't use same equality comparer, then Contains check is invalid,
	///     which is why callers must take are of this.
	///     If callers are concerned about whether this is a proper subset, they take care of that.
	/// </summary>
	/// <param name="other"></param>
	private bool IsSubsetOfHashSetWithSameEc([NotNull] ISet<T> other)
	{
		foreach (T item in this)
		{
			if (!other.Contains(item)) return false;
		}
			
		return true;
	}

	/// <summary>
	///     If other is a hashset that uses same equality comparer, intersect is much faster
	///     because we can use other Contains
	/// </summary>
	/// <param name="other"></param>
	private void IntersectWithHashSetWithSameEc(ISet<T> other)
	{
		CheckReentrancy();
		bool removed = false;
		SuppressCollectionEvents = true;

		try
		{
			for (int i = 0; i < _lastIndex; i++)
			{
				if (_slots[i].HashCode < 0) continue;
				T item = _slots[i].Value;
				if (!other.Contains(item)) removed |= Remove(item);
			}
		}
		finally
		{
			SuppressCollectionEvents = false;
		}

		if (!removed) return;
		OnPropertyChanged(nameof(Count));
		OnCollectionChanged();
	}

	/// <summary>
	///     Iterate over other. If contained in this, mark an element in bit array corresponding to
	///     its position in _slots. If anything is unmarked (in bit array), remove it.
	///     This attempts to allocate on the stack, if below StackAllocThreshold.
	/// </summary>
	/// <param name="other"></param>
	[SecuritySafeCritical]
	private unsafe void IntersectWithEnumerable([NotNull] IEnumerable<T> other)
	{
		CheckReentrancy();
		// keep track of current last index; don't want to move past the end of our bit array
		// (could happen if another thread is modifying the collection)
		int originalLastIndex = _lastIndex;
		int intArrayLength = BitHelper.ToIntArrayLength(originalLastIndex);
		Bits bits;

		if (intArrayLength <= STACK_ALLOC_THRESHOLD)
		{
			int* bitArrayPtr = stackalloc int[intArrayLength];
			bits = new Bits(bitArrayPtr, intArrayLength);
		}
		else
		{
			int[] bitArray = new int[intArrayLength];
			bits = new Bits(bitArray, intArrayLength);
		}

		// mark if contains: find index of in slots array and mark corresponding element in bit array
		foreach (T item in other)
		{
			int index = InternalIndexOf(item);
			if (index >= 0) bits.MarkBit(index);
		}

		bool removed = false;
		SuppressCollectionEvents = true;

		try
		{
			// if anything unmarked, remove it. Perf can be optimized here if Bits had a 
			// FindFirstUnmarked method.
			for (int i = 0; i < originalLastIndex; i++)
			{
				if (_slots[i].HashCode < 0 || bits.IsMarked(i)) continue;
				removed |= Remove(_slots[i].Value);
			}
		}
		finally
		{
			SuppressCollectionEvents = false;
		}

		if (!removed) return;
		OnPropertyChanged(nameof(Count));
		OnCollectionChanged();
	}

	/// <summary>
	///     Used internally by set operations which have to rely on bit array marking. This is like
	///     Contains but returns index in slots array.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	private int InternalIndexOf(T item)
	{
		int hashCode = InternalGetHashCode(item);

		for (int i = _buckets[hashCode % _buckets.Length] - 1; i >= 0; i = _slots[i].Next)
		{
			if (_slots[i].HashCode != hashCode || !Comparer.Equals(_slots[i].Value, item)) continue;
			return i;
		}

		// wasn't found
		return -1;
	}

	/// <summary>
	///     if other is a set, we can assume it doesn't have duplicate elements, so use this
	///     technique: if can't remove, then it wasn't present in this set, so add.
	///     As with other methods, callers take care of ensuring that other is a hashset using the
	///     same equality comparer.
	/// </summary>
	/// <param name="other"></param>
	private void SymmetricExceptWithUniqueHashSet([NotNull] ISet<T> other)
	{
		CheckReentrancy();
		bool added = false;
		SuppressCollectionEvents = true;

		try
		{
			foreach (T item in other)
			{
				if (Remove(item)) continue;
				added |= AddIfNotPresent(item);
			}
		}
		finally
		{
			SuppressCollectionEvents = false;
		}

		if (!added) return;
		OnPropertyChanged(nameof(Count));
		OnCollectionChanged();
	}

	/// <summary>
	///     Implementation notes:
	///     Used for symmetric except when other isn't a HashSet. This is more tedious because
	///     other may contain duplicates. HashSet technique could fail in these situations:
	///     1. Other has a duplicate that's not in this: HashSet technique would add then
	///     remove it.
	///     2. Other has a duplicate that's in this: HashSet technique would remove then add it
	///     back.
	///     In general, its presence would be toggled each time it appears in other.
	///     This technique uses bit marking to indicate whether to add/remove the item. If already
	///     present in collection, it will get marked for deletion. If added from other, it will
	///     get marked as something not to remove.
	/// </summary>
	/// <param name="other"></param>
	[SecuritySafeCritical]
	private unsafe void SymmetricExceptWithEnumerable([NotNull] IEnumerable<T> other)
	{
		CheckReentrancy();
		int originalLastIndex = _lastIndex;
		int intArrayLength = BitHelper.ToIntArrayLength(originalLastIndex);
		Bits itemsToRemove;
		Bits itemsAddedFromOther;

		if (intArrayLength <= STACK_ALLOC_THRESHOLD / 2)
		{
			int* itemsToRemovePtr = stackalloc int[intArrayLength];
			itemsToRemove = new Bits(itemsToRemovePtr, intArrayLength);

			int* itemsAddedFromOtherPtr = stackalloc int[intArrayLength];
			itemsAddedFromOther = new Bits(itemsAddedFromOtherPtr, intArrayLength);
		}
		else
		{
			int[] itemsToRemoveArray = new int[intArrayLength];
			itemsToRemove = new Bits(itemsToRemoveArray, intArrayLength);

			int[] itemsAddedFromOtherArray = new int[intArrayLength];
			itemsAddedFromOther = new Bits(itemsAddedFromOtherArray, intArrayLength);
		}

		bool changed = false;
		SuppressCollectionEvents = true;

		try
		{
			foreach (T item in other)
			{
				bool added = AddOrGetLocation(item, out int location);
					
				if (added)
				{
					// wasn't already present in collection; flag it as something not to remove
					// *NOTE* if location is out of range, we should ignore. Bits will
					// detect that it's out of bounds and not try to mark it. But it's 
					// expected that location could be out of bounds because adding the item
					// will increase _lastIndex as soon as all the free spots are filled.
					itemsAddedFromOther.MarkBit(location);
					changed = true;
				}
				else
				{
					// already there...if not added from other, mark for remove. 
					// *NOTE* Even though Bits will check that location is in range, we want 
					// to check here. There's no point in checking items beyond originalLastIndex
					// because they could not have been in the original collection
					if (location < originalLastIndex && !itemsAddedFromOther.IsMarked(location)) itemsToRemove.MarkBit(location);
				}
			}

			// if anything marked, remove it
			for (int i = 0; i < originalLastIndex; i++)
			{
				if (!itemsToRemove.IsMarked(i)) continue;
				changed |= Remove(_slots[i].Value);
			}
		}
		finally
		{
			SuppressCollectionEvents = false;
		}

		if (!changed) return;
		OnPropertyChanged(nameof(Count));
		OnCollectionChanged();
	}

	/// <summary>
	///     Add if not already in hashset. Returns an out param indicating index where added. This
	///     is used by SymmetricExcept because it needs to know the following things:
	///     - whether the item was already present in the collection or added from other
	///     - where it's located (if already present, it will get marked for removal, otherwise
	///     marked for keeping)
	/// </summary>
	/// <param name="value"></param>
	/// <param name="location"></param>
	/// <returns></returns>
	private bool AddOrGetLocation(T value, out int location)
	{
		int hashCode = InternalGetHashCode(value);
		int bucket = hashCode % _buckets.Length;

		for (int i = _buckets[hashCode % _buckets.Length] - 1; i >= 0; i = _slots[i].Next)
		{
			if (_slots[i].HashCode != hashCode || !Comparer.Equals(_slots[i].Value, value)) continue;
			location = i;
			return false; //already present
		}

		int index;

		if (_freeList >= 0)
		{
			index = _freeList;
			_freeList = _slots[index].Next;
		}
		else
		{
			if (_lastIndex == _slots.Length)
			{
				IncreaseCapacity();
				// this will change during resize
				bucket = hashCode % _buckets.Length;
			}

			index = _lastIndex;
			_lastIndex++;
		}

		_slots[index].HashCode = hashCode;
		_slots[index].Value = value;
		_slots[index].Next = _buckets[bucket] - 1;
		_buckets[bucket] = index + 1;
		Count++;
		_version++;
		location = index;
		OnPropertyChanged(nameof(Count));
		OnCollectionChanged();
		return true;
	}

	/// <summary>
	///     Determines counts that can be used to determine equality, subset, and superset. This
	///     is only used when other is an IEnumerable and not a HashSet. If other is a HashSet
	///     these properties can be checked faster without use of marking because we can assume
	///     other has no duplicates.
	///     The following count checks are performed by callers:
	///     1. Equals: checks if unfoundCount = 0 and uniqueFoundCount = _count; i.e. everything
	///     in other is in this and everything in this is in other
	///     2. Subset: checks if unfoundCount >= 0 and uniqueFoundCount = _count; i.e. other may
	///     have elements not in this and everything in this is in other
	///     3. Proper subset: checks if unfoundCount > 0 and uniqueFoundCount = _count; i.e
	///     other must have at least one element not in this and everything in this is in other
	///     4. Proper superset: checks if unfound count = 0 and uniqueFoundCount strictly less
	///     than _count; i.e. everything in other was in this and this had at least one element
	///     not contained in other.
	///     An earlier implementation used delegates to perform these checks rather than returning
	///     an ElementCount struct; however this was changed due to the perf overhead of delegates.
	/// </summary>
	/// <param name="other"></param>
	/// <param name="returnIfUnfound">
	///     Allows us to finish faster for equals and proper superset
	///     because unfoundCount must be 0.
	/// </param>
	/// <returns></returns>
	[SecuritySafeCritical]
	private unsafe ElementCount CheckUniqueAndUnfoundElements([NotNull] IEnumerable<T> other, bool returnIfUnfound)
	{
		ElementCount result;

		// need special case in case this has no elements. 
		if (Count == 0)
		{
			int numElementsInOther = 0;
			if (other.FastCount() > -1) numElementsInOther++;
			result.UniqueCount = 0;
			result.UnfoundCount = numElementsInOther;
			return result;
		}

		int originalLastIndex = _lastIndex;
		int intArrayLength = BitHelper.ToIntArrayLength(originalLastIndex);

		Bits bits;

		if (intArrayLength <= STACK_ALLOC_THRESHOLD)
		{
			int* bitArrayPtr = stackalloc int[intArrayLength];
			bits = new Bits(bitArrayPtr, intArrayLength);
		}
		else
		{
			int[] bitArray = new int[intArrayLength];
			bits = new Bits(bitArray, intArrayLength);
		}

		// count of items in other not found in this
		int unfoundCount = 0;
		// count of unique items in other found in this
		int uniqueFoundCount = 0;

		foreach (T item in other)
		{
			int index = InternalIndexOf(item);

			if (index >= 0)
			{
				if (bits.IsMarked(index)) continue;
				// item hasn't been seen yet
				bits.MarkBit(index);
				uniqueFoundCount++;
			}
			else
			{
				unfoundCount++;
				if (returnIfUnfound) break;
			}
		}

		result.UniqueCount = uniqueFoundCount;
		result.UnfoundCount = unfoundCount;
		return result;
	}

	/// <summary>
	///     Workaround Comparers that throw ArgumentNullException for GetHashCode(null).
	/// </summary>
	/// <param name="item"></param>
	/// <returns>hash code</returns>
	private int InternalGetHashCode(T item)
	{
		return ReferenceEquals(item, null)
					? 0
					: Comparer.GetHashCode(item) & LOWER31_BIT_MASK;
	}

	/// <summary>
	///     Internal method used for HashSetEqualityComparer. Compares set1 and set2 according
	///     to specified comparer.
	///     Because items are hashed according to a specific equality comparer, we have to resort
	///     to n^2 search if they're using different equality comparers.
	/// </summary>
	/// <param name="set1"></param>
	/// <param name="set2"></param>
	/// <param name="comparer"></param>
	/// <returns></returns>
	internal static bool HashSetEquals(ISet<T> set1, ISet<T> set2, IEqualityComparer<T> comparer)
	{
		// handle null cases first
		if (set1 == null) return set2 == null;

		if (set2 == null)
		{
			// set1 != null
			return false;
		}

		// all comparers are the same; this is faster
		if (AreEqualityComparersEqual(set1, set2))
		{
			if (set1.Count != set2.Count) return false;
			// suffices to check subset
			foreach (T item in set2)
			{
				if (!set1.Contains(item))
					return false;
			}

			return true;
		}

		// O(n^2) search because items are hashed according to their respective ECs
		foreach (T set2Item in set2)
		{
			bool found = false;

			foreach (T set1Item in set1)
			{
				if (!comparer.Equals(set2Item, set1Item)) continue;
				found = true;
				break;
			}

			if (!found) return false;
		}

		return true;
	}

	/// <summary>
	///     Checks if equality comparers are equal. This is used for algorithms that can
	///     speed up if it knows the other item has unique elements. I.e. if they're using
	///     different equality comparers, then uniqueness assumption between sets break.
	/// </summary>
	/// <param name="set1"></param>
	/// <param name="set2"></param>
	/// <returns></returns>
	private static bool AreEqualityComparersEqual([NotNull] ISet<T> set1, [NotNull] ISet<T> set2)
	{
		IEqualityComparer<T> comparer1 = set1.GetComparer();
		IEqualityComparer<T> comparer2 = set2.GetComparer();
		if (comparer1 == null && comparer2 == null) return true;
		if (comparer1 == null || comparer2 == null) return false;
		return comparer1.Equals(comparer2);
	}
}