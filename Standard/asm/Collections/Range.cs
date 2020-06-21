using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Helpers;
using asm.Other.MarcGravell;

namespace asm.Collections
{
	[Serializable]
	[TypeConverter(typeof(DisplayNameExpandableObjectConverter))]
	public class Range<T> : IRange<T>, ICollection, IComparable, IComparable<IReadOnlyRange<T>>, IEquatable<IReadOnlyRange<T>>
		where T : struct, IComparable
	{
		[NonSerialized]
		private object _syncRoot;

		private T _minimum;
		private T _maximum;
		private RangeLister<T> _lister;

		public Range(T entry)
			: this(entry, entry)
		{
		}

		public Range([NotNull] IReadOnlyRange<T> range)
			: this(range.Minimum, range.Maximum)
		{
		}

		public Range([NotNull] LambdaRange<T> range)
			: this(range.Minimum, range.Maximum)
		{
		}

		public Range(T minimum, T maximum)
		{
			if (minimum.IsGreaterThan(maximum)) throw new ArgumentException($"{nameof(minimum)} value must be greater than or equal to {nameof(maximum)}.");
			_minimum = minimum;
			_maximum = maximum;
		}

		/// <inheritdoc cref="IRange{T}" />
		public T Value
		{
			get => Minimum;
			set => Minimum = value;
		}

		/// <inheritdoc cref="IRange{T}" />
		public T Minimum
		{
			get => _minimum;
			set
			{
				if (_minimum.IsEqual(value)) return;
				_minimum = value;
				if (_maximum.IsLessThan(_minimum)) _maximum = _minimum;
				_lister = null;
			}
		}

		/// <inheritdoc cref="IRange{T}" />
		public T Maximum
		{
			get => _maximum;
			set
			{
				if (_maximum.Equals(value)) return;
				_maximum = value;

				if (_minimum.IsGreaterThan(_maximum))
				{
					if (_maximum.IsGreaterThan(default(T)))
						_minimum = _maximum;
					else
						_maximum = _minimum;
				}

				_lister = null;
			}
		}

		/// <inheritdoc />
		public T this[int index] => Lister.Item(index);
		
		/// <inheritdoc />
		public bool IsRange => HasMany;

		/// <inheritdoc />
		public bool IsEmpty => !IsIterable;

		/// <inheritdoc />
		public bool IsIterable => Lister.IsIterable;

		/// <inheritdoc />
		public bool HasMany => Lister.HasMany;

		/// <inheritdoc />
		public bool HasOne => Lister.HasOne;

		/// <inheritdoc cref="IReadOnlyList{T}" />
		public int Count => Lister.Count;

		/// <inheritdoc />
		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		/// <inheritdoc />
		bool ICollection.IsSynchronized => false;

		[NotNull]
		protected virtual RangeLister<T> Lister => _lister ??= new RangeLister<T>(Minimum, Maximum);

		[NotNull]
		public override string ToString()
		{
			return IsRange
						? $"{Minimum}{Range.SPLIT}{Maximum}"
						: $"{Value}";
		}

		public override int GetHashCode() { return RangeComparer<T>.Default.GetHashCode(this); }

		/// <inheritdoc />
		public bool Equals(IReadOnlyRange<T> other) { return RangeComparer<T>.Default.Equals(this, other); }

		/// <inheritdoc />
		public override bool Equals(object obj) { return obj != null && (ReferenceEquals(this, obj) || Equals(obj as IReadOnlyRange<T>)); }

		/// <inheritdoc />
		public int CompareTo(IReadOnlyRange<T> other) { return RangeComparer<T>.Default.Compare(this, other); }

		/// <inheritdoc />
		public virtual int CompareTo(object obj) { return RangeComparer<T>.Default.Compare(this, obj); }

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return Lister.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public bool InRange(T value)
		{
			return IsIterable && value.CompareTo(Minimum) >= 0 && value.CompareTo(Maximum) <= 0;
		}

		/// <inheritdoc />
		public bool InRangeEx(T value)
		{
			return IsIterable && value.CompareTo(Minimum) > 0 && value.CompareTo(Maximum) < 0;
		}

		/// <inheritdoc />
		public bool InRangeLx(T value)
		{
			return IsIterable && value.CompareTo(Minimum) > 0 && value.CompareTo(Maximum) <= 0;
		}

		/// <inheritdoc />
		public bool InRangeRx(T value)
		{
			return IsIterable && value.CompareTo(Minimum) >= 0 && value.CompareTo(Maximum) < 0;
		}

		/// <inheritdoc />
		public T Within(T value)
		{
			return !IsIterable
						? Minimum
						: Within(value, Minimum, Maximum);
		}

		protected T Within(T value, T minimum, T maximum)
		{
			if (minimum.CompareTo(maximum) > 0) throw new InvalidOperationException($"{nameof(minimum)} cannot be greater than {nameof(maximum)}.");
			return value.CompareTo(minimum) < 0
						? minimum
						: value.CompareTo(maximum) > 0
							? maximum
							: value;
		}

		/// <inheritdoc />
		public T WithinEx(T value)
		{
			return !IsIterable
						? Minimum
						: Within(value, Operator<T>.Increment(Minimum), Operator<T>.Decrement(Maximum));
		}

		/// <inheritdoc />
		public T WithinLx(T value)
		{
			return Within(value, Operator<T>.Increment(Minimum), Maximum);
		}

		/// <inheritdoc />
		public T WithinRx(T value)
		{
			return Within(value, Minimum, Operator<T>.Decrement(Maximum));
		}

		/// <inheritdoc />
		public bool Contains(T item) { return InRange(item); }

		/// <inheritdoc />
		public bool Contains(IReadOnlyRange<T> other) { return other != null && InRange(other.Minimum) && InRange(other.Maximum); }

		/// <inheritdoc />
		public bool Overlaps(IReadOnlyRange<T> other) { return other != null && Overlaps(other.Minimum, other.Maximum); }

		/// <inheritdoc />
		public bool Overlaps(T minimum, T maximum)
		{
			return minimum.IsLessThanOrEqual(maximum) && Minimum.IsLessThanOrEqual(maximum) && minimum.IsLessThanOrEqual(Maximum);
		}

		/// <inheritdoc />
		public bool IsPreviousTo(IReadOnlyRange<T> other) { return other != null && Maximum.IsLessThan(other.Minimum); }

		/// <inheritdoc />
		public bool IsNextTo(IReadOnlyRange<T> other) { return other != null && Minimum.IsGreaterThan(other.Maximum); }

		/// <inheritdoc />
		public virtual bool Merge(IReadOnlyRange<T> other)
		{
			if (ReferenceEquals(this, other) || other.IsEmpty || Contains(other)) return true;
			T min = Minimum.NotAbove(other.Minimum);
			T max = Maximum.NotBelow(other.Maximum.Maximum(min));
			if (min.IsEqual(Minimum) && max.IsEqual(Maximum)) return true;
			Minimum = min;
			Maximum = max;
			return true;
		}

		/// <inheritdoc />
		public virtual bool Exclude(IReadOnlyRange<T> other)
		{
			if (ReferenceEquals(this, other)) return false;
			if (other.IsEmpty) return true;
			if (!Overlaps(other)) return false;

			if (other.Minimum.IsLessThanOrEqual(Minimum) && other.Maximum.IsGreaterThanOrEqual(Maximum))
			{
				(T Minimum, T Maximum) bounds = Bounds;
				Minimum = bounds.Minimum;
				Maximum = bounds.Maximum;
				return true;
			}

			T min = Minimum.NotAbove(other.Minimum);
			T max = Maximum.NotBelow(other.Maximum.Maximum(min));
			if (min.IsEqual(Minimum) && max.IsEqual(Maximum)) return true;
			Minimum = min;
			Maximum = max;
			return true;
		}

		/// <inheritdoc />
		public virtual void ShiftBy(T steps)
		{
			T defaultValue = default(T);
			if (steps.IsEqual(defaultValue)) return;

			if (steps.IsGreaterThan(defaultValue))
			{
				/*
				 * steps is positive
				 * increase min, max by the amount of steps only
				 * if they will be less than or equal bounds max
				 */
				Minimum = Operator<T>.Add(Minimum, steps.NotAbove(Operator<T>.Subtract(Bounds.Maximum, Minimum)));
				Maximum = Operator<T>.Add(Maximum, steps.NotAbove(Operator<T>.Subtract(Bounds.Maximum, Maximum)));
			}
			else
			{
				/*
				 * steps is negative
				 * decrease min, max by the amount of steps only
				 * if they will be greater than or equal bounds min
				 */
				// This is not a mistake, I'll actually add a negative to a negative
				Minimum = Operator<T>.Add(Minimum, steps.NotBelow(Operator<T>.Subtract(Bounds.Minimum, Minimum)));
				Maximum = Operator<T>.Add(Maximum, steps.NotBelow(Operator<T>.Subtract(Bounds.Minimum, Maximum)));
			}
		}

		/// <inheritdoc />
		public virtual void InflateBy(T steps)
		{
			T defaultValue = default(T);
			if (steps.IsEqual(defaultValue)) return;

			if (steps.IsGreaterThan(defaultValue))
			{
				/*
				 * steps is positive
				 * decrease min, increase max by the amount of steps
				 */
				Minimum = Operator<T>.Add(Minimum, Operator<T>.Negate(steps).NotBelow(Operator<T>.Subtract(Bounds.Minimum, Minimum)));
				Maximum = Operator<T>.Add(Maximum, steps.NotAbove(Operator<T>.Subtract(Bounds.Maximum, Maximum)));
			}
			else
			{
				/*
				 * steps is negative
				 * increase min, decrease max by the amount of steps
				 */
				Minimum = Operator<T>.Subtract(Minimum, steps.NotAbove(Operator<T>.Subtract(Bounds.Maximum, Minimum)));
				Maximum = Operator<T>.Subtract(Maximum, Operator<T>.Negate(steps).NotBelow(Operator<T>.Subtract(Bounds.Minimum, Maximum)));
			}
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (!IsIterable)
			{
				array.Initialize(default(T), arrayIndex);
				return;
			}

			int i = arrayIndex;

			foreach (T value in Range.Get(Minimum, Maximum))
			{
				array[i] = value;
				i++;
			}
		}

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));
			if (Count == 0) return;

			if (array is T[] tArray)
			{
				CopyTo(tArray, index);
				return;
			}

			/*
			 * Catch the obvious case assignment will fail.
			 * We can find all possible problems by doing the check though.
			 * For example, if the element type of the Array is derived from T,
			 * we can't figure out if we can successfully copy the element beforehand.
			 */
			array.Length.ValidateRange(index, Count);

			Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
			Type sourceType = typeof(T);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[] objects)) throw new ArgumentException("Invalid array type", nameof(array));

			try
			{
				foreach (T item in this)
				{
					objects[index++] = item;
				}
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Invalid array type", nameof(array));
			}
		}

		public void ForEach([NotNull] Action<T> action)
		{
			foreach (T value in this) 
				action(value);
		}

		public void ForEach([NotNull] Action<T, int> action)
		{
			int i = 0;

			foreach (T value in this)
			{
				action(value, i);
				i++;
			}
		}

		public void ForEach([NotNull] Func<T, bool> action)
		{
			foreach (T value in this)
			{
				if (action(value)) continue;
				break;
			}
		}

		public void ForEach([NotNull] Func<T, int, bool> action)
		{
			int i = 0;

			foreach (T value in this)
			{
				if (!action(value, i)) break;
				i++;
			}
		}

		public static (T Minimum, T Maximum) Bounds { get; } = TypeHelper.Bounds<T>();

		public static bool operator ==(Range<T> x, Range<T> y) { return RangeComparer<T>.Default.Equals(x, y); }

		public static bool operator !=(Range<T> x, Range<T> y) { return !(x == y); }

		public static bool operator >(Range<T> x, Range<T> y) { return RangeComparer<T>.Default.Compare(x, y) > 0; }

		public static bool operator <(Range<T> x, Range<T> y) { return RangeComparer<T>.Default.Compare(x, y) < 0; }

		public static bool operator >=(Range<T> x, Range<T> y) { return RangeComparer<T>.Default.Compare(x, y) >= 0; }

		public static bool operator <=(Range<T> x, Range<T> y) { return RangeComparer<T>.Default.Compare(x, y) <= 0; }

		public static Range<T> operator ++(Range<T> value)
		{
			if (value == null) return null;
			value.InflateBy( Operator<T>.Increment(default(T)));
			return value;
		}

		public static Range<T> operator --(Range<T> value)
		{
			if (value == null) return null;
			value.InflateBy(Operator<T>.Decrement(default(T)));
			return value;
		}

		public static Range<T> operator +(Range<T> x, Range<T> y)
		{
			if (x == null || y == null) return null;
			if (ReferenceEquals(x, y) || y.IsEmpty) return x;
			T min = x.Minimum.Minimum(y.Minimum);
			T max = x.Maximum.Maximum(y.Maximum);
			return new Range<T>(min, max);
		}

		public static Range<T> operator +(Range<T> x, T value)
		{
			if (x == null) return null;
			Range<T> range = new Range<T>(x.Minimum, x.Maximum);
			range.InflateBy(value);
			return range;
		}

		public static Range<T>[] operator -(Range<T> x, Range<T> y)
		{
			if (x == null || y == null) return null;
			if (ReferenceEquals(x, y) || x.IsEmpty || y.IsEmpty || !x.Overlaps(y)) return new[] {x};

			if (x.Minimum.IsGreaterThanOrEqual(y.Minimum) && x.Maximum.IsLessThanOrEqual(y.Maximum)) return new[] { new Range<T>(default(T), default(T)) };

			if (x.Minimum.IsLessThan(y.Minimum) && x.Maximum.IsGreaterThan(y.Maximum))
			{
				return new[]
				{
					new Range<T>(x.Minimum, y.Minimum),
					new Range<T>(y.Maximum, x.Maximum)  
				};
			}

			return x.Minimum.IsLessThanOrEqual(y.Minimum) 
				? new[] {new Range<T>(x.Minimum, y.Minimum)} 
				: new[] {new Range<T>(y.Maximum, x.Maximum)};
		}

		public static Range<T> operator -(Range<T> x, T value)
		{
			if (x == null) return null;
			Range<T> range = new Range<T>(x.Minimum, x.Maximum);
			range.InflateBy(Operator<T>.Negate(value));
			return range;
		}

		[NotNull] 
		public static explicit operator Range<T>([NotNull] LambdaRange<T> value) { return new Range<T>(value); }
	}
	
	public static class Range
	{
		public const char SPLIT = '-';
		public const char GROUP = ',';

		[NotNull]
		public static IEnumerable<T> Get<T>(T start, T maximum)
			where T : struct, IComparable
		{
			return new RangeEnumerator<T>(start, maximum);
		}

		public static Range<T> Parse<T>(string value)
			where T : struct, IComparable
		{
			IsWellFormattedRangeString(value, out Range<T> parsed);
			return parsed;
		}

		[NotNull]
		public static ICollection<Range<T>> ParseGroup<T>(string value)
			where T : struct, IComparable
		{
			value = value?.Trim();
			if (string.IsNullOrEmpty(value)) return Array.Empty<Range<T>>();

			if (!value.Contains(GROUP))
			{
				if (!IsWellFormattedRangeString(value, out Range<T> range)) return Array.Empty<Range<T>>();
				return new Collection<Range<T>>
				{
					range
				};
			}

			ICollection<Range<T>> collection = new Collection<Range<T>>();
			IsWellFormattedRangeGroupString(value, collection);
			return collection;
		}

		public static bool IsWellFormattedRangeString<T>(string value)
			where T : struct, IComparable
		{
			return IsWellFormattedRangeString<T>(value, out _);
		}

		private static bool IsWellFormattedRangeString<T>(string value, out Range<T> parsed)
			where T : struct, IComparable
		{
			parsed = null;
			if (string.IsNullOrEmpty(value)) return false;

			T defaultValue = default(T);

			if (!value.Contains(SPLIT))
			{
				if (!value.IsNumbers()) return false;
				T p = value.To(defaultValue);
				parsed = new Range<T>(p, p);
				return true;
			}

			string[] parts = value.Split(2, SPLIT);
			if (parts.Length < 2 || !parts.All(s => s.IsNumbers())) return false;
			parsed = new Range<T>(parts[0].To(defaultValue), parts[1].To(defaultValue));
			return true;
		}

		public static bool IsWellFormattedRangeGroupString<T>(string value)
			where T : struct, IComparable
		{
			return IsWellFormattedRangeGroupString<T>(value, null);
		}

		private static bool IsWellFormattedRangeGroupString<T>(string value, ICollection<Range<T>> parsed)
			where T : struct, IComparable
		{
			if (string.IsNullOrEmpty(value)) return false;

			if (!value.Contains(GROUP))
			{
				if (!IsWellFormattedRangeString(value, out Range<T> p)) return false;
				parsed?.Add(p);
				return true;
			}

			Func<string, bool> predicate;

			if (parsed == null)
			{
				predicate = IsWellFormattedRangeString<T>;
			}
			else
			{
				predicate = s =>
				{
					if (!IsWellFormattedRangeString(s, out Range<T> p)) return false;
					parsed.Add(p);
					return true;
				};
			}

			return value.All(GROUP, predicate);
		}
	}
}