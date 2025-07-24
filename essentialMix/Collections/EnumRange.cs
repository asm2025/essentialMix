using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[Serializable]
[TypeConverter(typeof(DisplayNameExpandableObjectConverter))]
public class EnumRange<T> : Range<T>
	where T : struct, Enum, IComparable
{
	static EnumRange()
	{
	}

	public EnumRange()
		: this(MinValue, MaxValue)
	{
	}

	public EnumRange(T entry)
		: this(entry, entry)
	{
	}

	public EnumRange([NotNull] IReadOnlyRange<T> range)
		: this(range.Minimum, range.Maximum)
	{
	}

	public EnumRange([NotNull] LambdaRange<T> range)
		: this(range.Minimum, range.Maximum)
	{
	}

	public EnumRange(T minimum, T maximum)
		: base(minimum, maximum)
	{
		if (typeof(T).HasAttribute<FlagsAttribute>()) throw new InvalidOperationException($"{EnumType.FullName} has {nameof(FlagsAttribute)} attribute which is not supported.");
		if (Values.IndexOf(minimum) < 0) throw new ArgumentOutOfRangeException(nameof(minimum));
		if (Values.IndexOf(maximum) < 0) throw new ArgumentOutOfRangeException(nameof(maximum));
		Lister = new RangeLister<T>(Values);
	}

	public override int GetHashCode() { return EnumRangeComparer<T>.Default.GetHashCode(this); }

	public virtual bool Equals(EnumRange<T> other) { return EnumRangeComparer<T>.Default.Equals(this, other); }

	public override bool Equals(object obj) { return obj != null && (ReferenceEquals(this, obj) || Equals(obj as EnumRange<T>)); }

	public virtual int CompareTo(EnumRange<T> other) { return EnumRangeComparer<T>.Default.Compare(this, other); }

	public override int CompareTo(object obj) { return EnumRangeComparer<T>.Default.Compare(this, obj); }

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

	public void ForEach([NotNull] Action<T, string> action)
	{
		int i = 0;
		string[] names = Names();

		foreach (T value in Values)
		{
			action(value, names[i]);
			i++;
		}
	}

	public void ForEach([NotNull] Action<T, string, int> action)
	{
		int i = 0;
		string[] names = Names();

		foreach (T value in Values)
		{
			action(value, names[i], i);
			i++;
		}
	}

	public void ForEach([NotNull] Action<T, string, string> action)
	{
		int i = 0;
		string[] names = Names();
		string[] displayNames = DisplayNames();

		foreach (T value in Values)
		{
			action(value, names[i], displayNames[i]);
			i++;
		}
	}

	public void ForEach([NotNull] Action<T, string, string, int> action)
	{
		int i = 0;
		string[] names = Names();
		string[] displayNames = DisplayNames();

		foreach (T value in Values)
		{
			action(value, names[i], displayNames[i], i);
			i++;
		}
	}

	public void ForEach([NotNull] Func<T, string, bool> action)
	{
		int i = 0;
		string[] names = Names();

		foreach (T value in Values)
		{
			if (!action(value, names[i])) break;
			i++;
		}
	}

	public void ForEach([NotNull] Func<T, string, int, bool> action)
	{
		int i = 0;
		string[] names = Names();

		foreach (T value in Values)
		{
			if (!action(value, names[i], i)) break;
			i++;
		}
	}

	public void ForEach([NotNull] Func<T, string, string, bool> action)
	{
		int i = 0;
		string[] names = Names();
		string[] displayNames = DisplayNames();

		foreach (T value in Values)
		{
			if (!action(value, names[i], displayNames[i])) break;
			i++;
		}
	}

	public void ForEach([NotNull] Func<T, string, string, int, bool> action)
	{
		int i = 0;
		string[] names = Names();
		string[] displayNames = DisplayNames();

		foreach (T value in Values)
		{
			if (!action(value, names[i], displayNames[i], i)) break;
			i++;
		}
	}

	protected override RangeLister<T> Lister { get; }

	public static Type EnumType { get; } = typeof(T);

	public static IReadOnlyList<T> Values { get; } = EnumHelper<T>.GetValues();

	[NotNull]
	public static string[] Names() { return Values.Select(v => v.ToString()).ToArray(); }

	[NotNull]
	public static string[] DisplayNames() { return Values.Select(v => EnumType.GetDisplayName(v)).ToArray(); }

	public static T MinValue { get; } = Values[0];

	public static T MaxValue { get; } = Values[Values.Count - 1];

	public static bool operator ==(EnumRange<T> x, EnumRange<T> y) { return EnumRangeComparer<T>.Default.Equals(x, y); }

	public static bool operator !=(EnumRange<T> x, EnumRange<T> y) { return !(x == y); }

	public static bool operator >([NotNull] EnumRange<T> x, EnumRange<T> y) { return x.IsIterable && x.Maximum.IsGreaterThan(y.Maximum); }

	public static bool operator <([NotNull] EnumRange<T> x, EnumRange<T> y) { return x.IsIterable && x.Minimum.IsLessThan(y.Minimum); }

	public static bool operator >=([NotNull] EnumRange<T> x, EnumRange<T> y) { return x.IsIterable && x.Maximum.IsGreaterThanOrEqual(y.Maximum); }

	public static bool operator <=([NotNull] EnumRange<T> x, EnumRange<T> y) { return x.IsIterable && x.Minimum.IsLessThanOrEqual(y.Minimum); }

	public static EnumRange<T> operator +(EnumRange<T> x, EnumRange<T> y)
	{
		if (x == null || y == null) return null;
		if (ReferenceEquals(x, y) || y.IsEmpty) return x;
		T min = x.Minimum.Minimum(y.Minimum);
		T max = x.Maximum.Maximum(y.Maximum);
		return new EnumRange<T>(min, max);
	}

	public static EnumRange<T>[] operator -(EnumRange<T> x, EnumRange<T> y)
	{
		if (x == null || y == null) return null;
		if (ReferenceEquals(x, y) || x.IsEmpty || y.IsEmpty || !x.Overlaps(y)) return [x];
		if (x.Minimum.IsGreaterThanOrEqual(y.Minimum) && x.Maximum.IsLessThanOrEqual(y.Maximum)) return [new EnumRange<T>()
		];

		if (x.Minimum.IsLessThan(y.Minimum) && x.Maximum.IsGreaterThan(y.Maximum))
		{
			return
			[
				new EnumRange<T>(x.Minimum, y.Minimum),
				new EnumRange<T>(y.Maximum, x.Maximum)
			];
		}

		return x.Minimum.IsLessThanOrEqual(y.Minimum)
					? [new EnumRange<T>(x.Minimum, y.Minimum)]
					: [new EnumRange<T>(y.Maximum, x.Maximum)];
	}
}

public static class EnumRange
{
	public static EnumRange<T> Parse<T>(string value)
		where T : struct, Enum, IComparable
	{
		IsWellFormattedRangeString(value, out EnumRange<T> parsed);
		return parsed;
	}

	[NotNull]
	public static ICollection<EnumRange<T>> ParseGroup<T>(string value)
		where T : struct, Enum, IComparable
	{
		value = value?.Trim();
		if (string.IsNullOrEmpty(value)) return Array.Empty<EnumRange<T>>();

		if (!value.Contains(Range.GROUP))
		{
			if (!IsWellFormattedRangeString(value, out EnumRange<T> range)) return Array.Empty<EnumRange<T>>();
			return new Collection<EnumRange<T>> { range };
		}

		ICollection<EnumRange<T>> collection = new Collection<EnumRange<T>>();
		IsWellFormattedRangeGroupString(value, collection);
		return collection;
	}

	public static bool IsWellFormattedRangeString<T>(string value)
		where T : struct, Enum, IComparable
	{
		return IsWellFormattedRangeString<T>(value, out _);
	}

	private static bool IsWellFormattedRangeString<T>(string value, out EnumRange<T> parsed)
		where T : struct, Enum, IComparable
	{
		parsed = null;
		if (string.IsNullOrEmpty(value)) return false;

		if (!value.Contains(Range.SPLIT))
		{
			if (!EnumHelper<T>.TryParse(value, true, out T p)) return false;
			parsed = new EnumRange<T>(p, p);
			return true;
		}

		string[] parts = value.Split(2, Range.SPLIT);
		if (parts.Length != 2 || parts.Any(string.IsNullOrEmpty)) return false;
		if (!EnumHelper<T>.TryParse(value, true, out T p1) || !EnumHelper<T>.TryParse(value, true, out T p2)) return false;
		parsed = new EnumRange<T>(p1, p2);
		return true;
	}

	public static bool IsWellFormattedRangeGroupString<T>(string value)
		where T : struct, Enum, IComparable
	{
		return IsWellFormattedRangeGroupString<T>(value, null);
	}

	private static bool IsWellFormattedRangeGroupString<T>(string value, ICollection<EnumRange<T>> parsed)
		where T : struct, Enum, IComparable
	{
		if (string.IsNullOrEmpty(value)) return false;

		if (!value.Contains(Range.GROUP))
		{
			if (!IsWellFormattedRangeString(value, out EnumRange<T> p)) return false;
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
				if (!IsWellFormattedRangeString(s, out EnumRange<T> p)) return false;
				parsed.Add(p);
				return true;
			};
		}

		return value.All(Range.GROUP, predicate);
	}
}