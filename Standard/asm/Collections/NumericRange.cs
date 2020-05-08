using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	[TypeConverter(typeof(DisplayNameExpandableObjectConverter))]
	public class NumericRange<T> : Range<T>
		where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
	{
		public NumericRange(T entry)
			: base(entry)
		{
		}

		public NumericRange([NotNull] IReadOnlyRange<T> range) 
			: base(range)
		{
		}

		public NumericRange([NotNull] LambdaRange<T> range) 
			: base(range)
		{
		}

		public NumericRange(T minimum, T maximum) 
			: base(minimum, maximum)
		{
		}

		public override int GetHashCode() { return NumericRangeComparer<T>.Default.GetHashCode(this); }

		public override bool Equals(object obj) { return obj != null && (ReferenceEquals(this, obj) || Equals(obj as IReadOnlyRange<T>)); }

		public override bool Merge(IReadOnlyRange<T> other)
		{
			if (ReferenceEquals(this, other) || other.IsEmpty || Contains(other)) return true;

			if (Overlaps(other))
			{
				T min = Minimum.NotAbove(other.Minimum);
				T max = Maximum.NotBelow(other.Maximum.Maximum(min));
				if (min.IsEqual(Minimum) && max.IsEqual(Maximum)) return true;
				Minimum = min;
				Maximum = max;
				return true;
			}

			if (IsPreviousTo(other))
			{
				Maximum = other.Maximum;
				return true;
			}

			if (!IsNextTo(other)) return false;
			Minimum = other.Minimum;
			return true;

		}

		public static bool operator ==(NumericRange<T> x, NumericRange<T> y) { return NumericRangeComparer<T>.Default.Equals(x, y); }

		public static bool operator !=(NumericRange<T> x, NumericRange<T> y) { return !(x == y); }

		public static bool operator >([NotNull] NumericRange<T> x, NumericRange<T> y) { return x.IsIterable && x.Maximum.IsGreaterThan(y.Maximum); }

		public static bool operator <([NotNull] NumericRange<T> x, NumericRange<T> y) { return x.IsIterable && x.Minimum.IsLessThan(y.Minimum); }

		public static bool operator >=([NotNull] NumericRange<T> x, NumericRange<T> y) { return x.IsIterable && x.Maximum.IsGreaterThanOrEqual(y.Maximum); }

		public static bool operator <=([NotNull] NumericRange<T> x, NumericRange<T> y) { return x.IsIterable && x.Minimum.IsLessThanOrEqual(y.Minimum); }

		public static NumericRange<T> operator ++(NumericRange<T> value)
		{
			if (value == null) return null;
			value.InflateBy(default(T).Increment());
			return value;
		}

		public static NumericRange<T> operator --(NumericRange<T> value)
		{
			if (value == null) return null;
			value.InflateBy(default(T).Decrement());
			return value;
		}

		public static NumericRange<T> operator +(NumericRange<T> x, NumericRange<T> y)
		{
			if (x == null || y == null) return null;
			if (ReferenceEquals(x, y) || y.IsEmpty) return x;
			T min = x.Minimum.Minimum(y.Minimum);
			T max = x.Maximum.Maximum(y.Maximum);
			return new NumericRange<T>(min, max);
		}

		public static NumericRange<T> operator +(NumericRange<T> x, T value)
		{
			if (x == null) return null;
			NumericRange<T> range = new NumericRange<T>(x.Minimum, x.Maximum);
			range.InflateBy(value);
			return range;
		}

		public static NumericRange<T>[] operator -(NumericRange<T> x, NumericRange<T> y)
		{
			if (x == null || y == null) return null;
			if (ReferenceEquals(x, y) || x.IsEmpty || y.IsEmpty || !x.Overlaps(y)) return new[] { x };
			if (x.Minimum.IsGreaterThanOrEqual(y.Minimum) && x.Maximum.IsLessThanOrEqual(y.Maximum)) return new[] { new NumericRange<T>(default(T), default(T)) };

			if (x.Minimum.IsLessThan(y.Minimum) && x.Maximum.IsGreaterThan(y.Maximum))
			{
				return new[]
				{
					new NumericRange<T>(x.Minimum, y.Minimum),
					new NumericRange<T>(y.Maximum, x.Maximum)
				};
			}

			return x.Minimum.IsLessThanOrEqual(y.Minimum)
				? new[] { new NumericRange<T>(x.Minimum, y.Minimum) }
				: new[] { new NumericRange<T>(y.Maximum, x.Maximum) };
		}

		public static NumericRange<T> operator -(NumericRange<T> x, T value)
		{
			if (x == null) return null;
			NumericRange<T> range = new NumericRange<T>(x.Minimum, x.Maximum);
			range.InflateBy(value.Negate());
			return range;
		}
	}

	public static class NumericRange
	{
		public static NumericRange<T> Parse<T>(string value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			IsWellFormattedRangeString(value, out NumericRange<T> parsed);
			return parsed;
		}

		[NotNull]
		public static ICollection<NumericRange<T>> ParseGroup<T>(string value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			value = value?.Trim();
			if (string.IsNullOrEmpty(value)) return Array.Empty<NumericRange<T>>();

			if (!value.Contains(Range.GROUP))
			{
				if (!IsWellFormattedRangeString(value, out NumericRange<T> range)) return Array.Empty<NumericRange<T>>();
				return new Collection<NumericRange<T>> {range};
			}

			ICollection<NumericRange<T>> collection = new Collection<NumericRange<T>>();
			IsWellFormattedRangeGroupString(value, collection);
			return collection;
		}

		public static bool IsWellFormattedRangeString<T>(string value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			return IsWellFormattedRangeString<T>(value, out _);
		}

		private static bool IsWellFormattedRangeString<T>(string value, out NumericRange<T> parsed)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			parsed = null;
			if (string.IsNullOrEmpty(value)) return false;

			T defaultValue = default(T);

			if (!value.Contains(Range.SPLIT))
			{
				if (!value.IsNumbers()) return false;
				T p = value.To(defaultValue);
				parsed = new NumericRange<T>(p, p);
				return true;
			}

			string[] parts = value.Split(2, Range.SPLIT);
			if (parts.Length < 2 || !parts.All(s => s.IsNumbers())) return false;
			parsed = new NumericRange<T>(parts[0].To(defaultValue), parts[1].To(defaultValue));
			return true;
		}

		public static bool IsWellFormattedRangeGroupString<T>(string value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			return IsWellFormattedRangeGroupString<T>(value, null);
		}

		private static bool IsWellFormattedRangeGroupString<T>(string value, ICollection<NumericRange<T>> parsed)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			if (string.IsNullOrEmpty(value)) return false;

			if (!value.Contains(Range.GROUP))
			{
				if (!IsWellFormattedRangeString(value, out NumericRange<T> p)) return false;
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
					if (!IsWellFormattedRangeString(s, out NumericRange<T> p)) return false;
					parsed.Add(p);
					return true;
				};
			}

			return value.All(Range.GROUP, predicate);
		}
	}
}