using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[Serializable]
[TypeConverter(typeof(DisplayNameExpandableObjectConverter))]
public class ReadOnlyRange<T> : IReadOnlyRange<T>, ICollection, IComparable, IComparable<IReadOnlyRange<T>>, IEquatable<IReadOnlyRange<T>>
	where T : struct, IComparable
{
	private readonly IReadOnlyRange<T> _range;
	private readonly ICollection _collection;

	public ReadOnlyRange([NotNull] LambdaRange<T> range)
		: this(new Range<T>(range.Minimum, range.Maximum))
	{
	}

	public ReadOnlyRange([NotNull] IRange<T> range) 
	{
		_range = range;
		_collection = _range as ICollection ?? throw new ArgumentException("Argument does not implement ICollection.", nameof(range));
	}

	/// <inheritdoc />
	public T Value => _range.Value;

	/// <inheritdoc />
	public T Minimum => _range.Minimum;

	/// <inheritdoc />
	public T Maximum => _range.Maximum;

	/// <inheritdoc />
	public T this[int index] => _range[index];

	/// <inheritdoc />
	public bool IsRange => _range.IsRange;

	/// <inheritdoc />
	public bool IsEmpty => _range.IsEmpty;

	/// <inheritdoc />
	public bool IsIterable => _range.IsIterable;

	/// <inheritdoc />
	public bool HasMany => _range.HasMany;

	/// <inheritdoc />
	public bool HasOne => _range.HasOne;

	/// <inheritdoc cref="IReadOnlyList{T}" />
	public int Count => _range.Count;

	/// <inheritdoc />
	bool ICollection.IsSynchronized => _collection.IsSynchronized;

	/// <inheritdoc />
	object ICollection.SyncRoot => _collection.SyncRoot;

	/// <inheritdoc />
	[NotNull]
	public override string ToString() { return _range.ToString(); }

	public override int GetHashCode() { return _range.GetHashCode(); }

	/// <inheritdoc />
	public bool Equals(IReadOnlyRange<T> other) { return _range.Equals(other); }

	/// <inheritdoc />
	public override bool Equals(object obj) { return obj != null && (ReferenceEquals(this, obj) || Equals(obj as IReadOnlyRange<T>)); }

	/// <inheritdoc />
	public int CompareTo(IReadOnlyRange<T> other) { return RangeComparer<T>.Default.Compare(_range, other); }

	/// <inheritdoc />
	public int CompareTo(object obj) { return RangeComparer<T>.Default.Compare(_range, obj); }

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator() { return _range.GetEnumerator(); }

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	/// <inheritdoc />
	public bool InRange(T value) { return _range.InRange(value); }

	/// <inheritdoc />
	public bool InRangeEx(T value) { return _range.InRangeEx(value); }

	/// <inheritdoc />
	public bool InRangeLx(T value) { return _range.InRangeLx(value); }

	/// <inheritdoc />
	public bool InRangeRx(T value) { return _range.InRangeRx(value); }

	/// <inheritdoc />
	public T Within(T value) { return _range.Within(value); }

	/// <inheritdoc />
	public T WithinEx(T value) { return _range.WithinEx(value); }

	/// <inheritdoc />
	public T WithinLx(T value) { return _range.WithinLx(value); }

	/// <inheritdoc />
	public T WithinRx(T value) { return _range.WithinRx(value); }

	/// <inheritdoc />
	public bool Contains(T item) { return _range.Contains(item); }

	/// <inheritdoc />
	public bool Contains(IReadOnlyRange<T> other) { return _range.Contains(other); }

	/// <inheritdoc />
	public bool Overlaps(IReadOnlyRange<T> other) { return _range.Overlaps(other); }

	/// <inheritdoc />
	public bool Overlaps(T minimum, T maximum) { return _range.Overlaps(minimum, maximum); }

	/// <inheritdoc />
	public bool IsPreviousTo(IReadOnlyRange<T> other) { return _range.IsPreviousTo(other); }

	/// <inheritdoc />
	public bool IsNextTo(IReadOnlyRange<T> other) { return _range.IsNextTo(other); }
		
	/// <inheritdoc />
	public void CopyTo(T[] array, int arrayIndex) { _range.CopyTo(array, arrayIndex); }
	
	/// <inheritdoc />
	void ICollection.CopyTo(Array array, int index) { _collection.CopyTo(array, index); }

	public static (T Minimum, T Maximum) Bounds { get; } = TypeHelper.Bounds<T>();

	public static bool operator ==(ReadOnlyRange<T> x, ReadOnlyRange<T> y) { return RangeComparer<T>.Default.Equals(x, y); }

	public static bool operator !=(ReadOnlyRange<T> x, ReadOnlyRange<T> y) { return !(x == y); }

	public static bool operator >(ReadOnlyRange<T> x, ReadOnlyRange<T> y) { return RangeComparer<T>.Default.Compare(x, y) > 0; }

	public static bool operator <(ReadOnlyRange<T> x, ReadOnlyRange<T> y) { return RangeComparer<T>.Default.Compare(x, y) < 0; }

	public static bool operator >=(ReadOnlyRange<T> x, ReadOnlyRange<T> y) { return RangeComparer<T>.Default.Compare(x, y) >= 0; }

	public static bool operator <=(ReadOnlyRange<T> x, ReadOnlyRange<T> y) { return RangeComparer<T>.Default.Compare(x, y) <= 0; }
}

public static class ReadOnlyRangeExtension
{
	[NotNull]
	public static ReadOnlyRange<T> AsReadOnly<T>([NotNull] this IRange<T> thisValue)
		where T : struct, IComparable
	{
		return new ReadOnlyRange<T>(thisValue);
	}

	[NotNull]
	public static ReadOnlyRange<T> AsReadOnly<T>([NotNull] this LambdaRange<T> thisValue)
		where T : struct, IComparable
	{
		return new ReadOnlyRange<T>(thisValue);
	}
}