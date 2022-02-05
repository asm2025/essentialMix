using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <inheritdoc />
/// <summary>
/// Simple non-unique map wrapper
/// </summary>
/// <remarks>
/// ApplyResultSelector (from Lookup[TKey, TElement] is not implemented,
/// since the caller could just as easily (or more-so) use .Select() with
/// a Func[IGrouping[TKey, TElement], TResult], since
/// IGrouping[TKey, TElement] already includes both the "TKey Key"
/// and the IEnumerable[TElement].
/// </remarks>
public sealed class Lookup<TKey, TElement> : ILookup<TKey, TElement>
{
	internal sealed class LookupGrouping : IGrouping<TKey, TElement>
	{
		private readonly List<TElement> _items = new List<TElement>();

		public LookupGrouping(TKey key)
		{
			Key = key;
		}

		public TKey Key { get; }

		public int Count => _items.Count;

		public void Add(TElement item) { _items.Add(item); }

		public bool Contains(TElement item) { return _items.Contains(item); }

		public bool Remove(TElement item) { return _items.Remove(item); }

		public void TrimExcess() { _items.TrimExcess(); }

		public IEnumerator<TElement> GetEnumerator() { return _items.GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	private readonly Dictionary<TKey, LookupGrouping> _groups;

	/// <inheritdoc />
	/// <summary>
	/// Creates a new Lookup using the default key-comparer
	/// </summary>
	public Lookup() 
		: this(null)
	{
	}

	/// <summary>
	/// Creates a new Lookup using the specified key-comparer
	/// </summary>
	/// <param name="keyComparer"></param>
	public Lookup(IEqualityComparer<TKey> keyComparer)
	{
		_groups = new Dictionary<TKey, LookupGrouping>(keyComparer ?? EqualityComparer<TKey>.Default);
	}

	/// <inheritdoc />
	/// <summary>
	/// Returns the set of values for the given key
	/// </summary>
	public IEnumerable<TElement> this[[NotNull] TKey key] => _groups.TryGetValue(key, out LookupGrouping group) ? group : Array.Empty<TElement>();

	/// <inheritdoc />
	/// <summary>
	/// Returns the number of distinct keys in the lookup
	/// </summary>
	public int Count => _groups.Count;

	/// <inheritdoc />
	/// <summary>
	/// Does the lookup contain any value(s) for the given key?
	/// </summary>
	public bool Contains(TKey key) { return _groups.TryGetValue(key, out LookupGrouping group) && group.Count > 0; }

	/// <summary>
	/// Does the lookup the specific key/value pair?
	/// </summary>
	public bool Contains([NotNull] TKey key, TElement value) { return _groups.TryGetValue(key, out LookupGrouping group) && group.Contains(value); }

	/// <summary>
	/// Adds a key/value pair to the lookup
	/// </summary>
	/// <remarks>If the value is already present it will be duplicated</remarks>
	public void Add([NotNull] TKey key, TElement value)
	{
		if (!_groups.TryGetValue(key, out LookupGrouping group))
		{
			group = new LookupGrouping(key);
			_groups.Add(key, group);
		}
		group.Add(value);
	}

	/// <summary>
	/// Adds a range of values against a single key
	/// </summary>
	/// <remarks>Any values already present will be duplicated</remarks>
	public void AddRange([NotNull] TKey key, [NotNull] IEnumerable<TElement> values)
	{
		if (!_groups.TryGetValue(key, out LookupGrouping group))
		{
			group = new LookupGrouping(key);
			_groups.Add(key, group);
		}

		foreach (TElement value in values)
			group.Add(value);

		if (group.Count == 0) _groups.Remove(key); // nothing there after all!
	}

	/// <summary>
	/// Add all key/value pairs from the supplied lookup
	/// to the current lookup
	/// </summary>
	/// <remarks>Any values already present will be duplicated</remarks>
	public void AddRange([NotNull] ILookup<TKey, TElement> lookup)
	{
		foreach (IGrouping<TKey, TElement> group in lookup)
			AddRange(group.Key, group);
	}

	/// <summary>
	/// Remove all values from the lookup for the given key
	/// </summary>
	/// <returns>True if any items were removed, else false</returns>
	public bool Remove([NotNull] TKey key) { return _groups.Remove(key); }

	/// <summary>
	/// Remove the specific key/value pair from the lookup
	/// </summary>
	/// <returns>True if the item was found, else false</returns>
	public bool Remove([NotNull] TKey key, TElement value)
	{
		if (_groups.TryGetValue(key, out LookupGrouping group))
		{
			bool removed = group.Remove(value);
			if (removed && group.Count == 0) _groups.Remove(key);
			return removed;
		}
		return false;
	}

	/// <summary>
	/// Trims the inner data-structure to remove
	/// any surplus space
	/// </summary>
	public void TrimExcess()
	{
		foreach (LookupGrouping group in _groups.Values)
			group.TrimExcess();
	}

	/// <inheritdoc />
	/// <summary>
	/// Returns the sequence of keys and their contained values
	/// </summary>
	public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() { return _groups.Values.Cast<IGrouping<TKey, TElement>>().GetEnumerator(); }

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}