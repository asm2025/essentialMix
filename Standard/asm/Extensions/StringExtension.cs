using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using asm.Collections;
using asm.Helpers;
using asm.Patterns.String;
using CharEnumerator = asm.Collections.CharEnumerator;

namespace asm.Extensions
{
	public static class StringExtension
	{
		private const string RGX_PARTITIONS = "[^{0}]+";
		private const string RGX_WORDS = @"\w+";

		private static readonly Regex RGX_PRINTF = new Regex(@"\%(\d*\$)?([\'\#\-\+ ]*)(\d*)(?:\.(\d+))?([hl])?([dioxXucsfeEgGpn%])", RegexHelper.OPTIONS_I | RegexOptions.Multiline);

		public static string ToNullIfEmpty(this string thisValue)
		{
			thisValue = thisValue?.Trim();
			if (string.IsNullOrEmpty(thisValue)) thisValue = null;
			return thisValue;
		}

		public static bool Contains([NotNull] this string thisValue, char value) { return thisValue.IndexOf(value, 0, thisValue.Length) > -1; }

		public static bool Contains([NotNull] this string thisValue, char value, int startIndex)
		{
			return thisValue.IndexOf(value, startIndex, thisValue.Length - startIndex) > -1;
		}

		public static bool Contains([NotNull] this string thisValue, char value, int startIndex, int count) { return thisValue.IndexOf(value, startIndex, count) > -1; }

		public static bool Contains([NotNull] this string thisValue, [NotNull] string value, StringComparison comparison) { return thisValue.IndexOf(value, comparison) > -1; }

		public static bool Contains([NotNull] this string thisValue, [NotNull] string value, int startIndex)
		{
			return thisValue.IndexOf(value, startIndex, StringComparison.Ordinal) > -1;
		}

		public static bool Contains([NotNull] this string thisValue, [NotNull] string value, int startIndex, StringComparison comparison)
		{
			return thisValue.IndexOf(value, startIndex, comparison) > -1;
		}

		public static bool Contains([NotNull] this string thisValue, [NotNull] string value, int startIndex, int count)
		{
			return thisValue.IndexOf(value, startIndex, count, StringComparison.Ordinal) > -1;
		}

		public static bool Contains([NotNull] this string thisValue, [NotNull] string value, int startIndex, int count, StringComparison comparison)
		{
			return thisValue.IndexOf(value, startIndex, count, comparison) > -1;
		}

		public static bool StartsWith([NotNull] this string thisValue, char value) { return thisValue.Length != 0 && thisValue[0] == value; }

		public static bool StartsWith([NotNull] this string thisValue, string value, StringComparison comparison)
		{
			return thisValue.Length != 0 && !string.IsNullOrEmpty(value) && value.Length <= thisValue.Length && thisValue.IndexOf(value, 0, value.Length, comparison) == 0;
		}

		public static bool EndsWith([NotNull] this string thisValue, char value) { return thisValue.Length != 0 && thisValue[thisValue.Length - 1] == value; }

		public static bool EndsWith([NotNull] this string thisValue, string value, StringComparison comparison)
		{
			if (string.IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(value) || value.Length > thisValue.Length) return false;

			int index = thisValue.Length - value.Length;
			return thisValue.IndexOf(value, index, value.Length, comparison) == index;
		}

		public static bool In([NotNull] this string thisValue, [NotNull] params string[] list) { return In(thisValue, (IEqualityComparer<string>)null, list); }

		public static bool In([NotNull] this string thisValue, [NotNull] Comparison<string> comparison, [NotNull] params string[] list)
		{
			return In(thisValue, comparison.Create(), list);
		}

		public static bool In([NotNull] this string thisValue, IEqualityComparer<string> comparer, [NotNull] params string[] list)
		{
			if (string.IsNullOrEmpty(thisValue) || list.Length == 0) return false;
			comparer ??= StringComparer.Ordinal;
			return list.Where(s => !string.IsNullOrEmpty(s)).Any(s => comparer.Equals(thisValue, s));
		}

		public static bool In([NotNull] this string thisValue, [NotNull] IEnumerable<string> enumerable, IEqualityComparer<string> comparer = null, int startIndex = 0, int count = -1)
		{
			return enumerable.IndexOf(thisValue, comparer, startIndex, count) > -1;
		}

		public static string FirstNotNullOrEmptyOrDefault([NotNull] this IEnumerable<string> thisValue) { return thisValue.FirstOrDefault(e => !string.IsNullOrEmpty(e)); }
		public static string LastNotNullOrEmptyOrDefault([NotNull] this IEnumerable<string> thisValue) { return thisValue.LastOrDefault(e => !string.IsNullOrEmpty(e)); }

		public static string FirstNotNullOrEmpty([NotNull] this IEnumerable<string> thisValue) { return thisValue.First(e => !string.IsNullOrEmpty(e)); }
		public static string LastNotNullOrEmpty([NotNull] this IEnumerable<string> thisValue) { return thisValue.Last(e => !string.IsNullOrEmpty(e)); }

		public static string SingleNotNullOrEmptyOrDefault([NotNull] this IEnumerable<string> thisValue) { return thisValue.SingleOrDefault(e => !string.IsNullOrEmpty(e)); }

		public static string FirstNotNullOrWhiteSpaceOrDefault([NotNull] this IEnumerable<string> thisValue) { return thisValue.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e)); }
		public static string LastNotNullOrWhiteSpaceOrDefault([NotNull] this IEnumerable<string> thisValue) { return thisValue.LastOrDefault(e => !string.IsNullOrWhiteSpace(e)); }

		public static string FirstNotNullOrWhiteSpace([NotNull] this IEnumerable<string> thisValue) { return thisValue.First(e => !string.IsNullOrWhiteSpace(e)); }
		public static string LastNotNullOrWhiteSpace([NotNull] this IEnumerable<string> thisValue) { return thisValue.Last(e => !string.IsNullOrWhiteSpace(e)); }

		public static string SingleNotNullOrWhiteSpaceOrDefault([NotNull] this IEnumerable<string> thisValue) { return thisValue.SingleOrDefault(e => !string.IsNullOrWhiteSpace(e)); }

		[NotNull]
		public static string Format([NotNull] this string thisValue, [NotNull] params object[] objects)
		{
			return objects.IsNullOrEmpty()
						? thisValue
						: string.IsNullOrEmpty(thisValue)
							? thisValue
							: string.Format(thisValue, objects);
		}

		[NotNull]
		public static string Append([NotNull] this string thisValue, [NotNull] params char[] values)
		{
			switch (values.Length)
			{
				case 0:
					return thisValue;
				case 1:
					return string.Concat(thisValue, values[0]);
				case 2:
					return string.Concat(thisValue, values[0], values[1]);
				case 3:
					return string.Concat(thisValue, values[0], values[1], values[2]);
				default:
					return new StringBuilder(thisValue).Append(values).ToString();
			}
		}

		[NotNull]
		public static string Append([NotNull] this string thisValue, [NotNull] params string[] values)
		{
			switch (values.Length)
			{
				case 0:
					return thisValue;
				case 1:
					return string.Concat(thisValue, values[0]);
				case 2:
					return string.Concat(thisValue, values[0], values[1]);
				case 3:
					return string.Concat(thisValue, values[0], values[1], values[2]);
				default:
					return new StringBuilder(thisValue).Append(values).ToString();
			}
		}

		[NotNull]
		public static string Append([NotNull] this string thisValue, [NotNull] params object[] values)
		{
			switch (values.Length)
			{
				case 0:
					return thisValue;
				case 1:
					return string.Concat(thisValue, values[0]);
				case 2:
					return string.Concat(thisValue, values[0], values[1]);
				case 3:
					return string.Concat(thisValue, values[0], values[1], values[2]);
				default:
					return new StringBuilder(thisValue).Append(values).ToString();
			}
		}

		[NotNull]
		public static string AppendIfDoesNotEndWith([NotNull] this string thisValue, char value)
		{
			if (thisValue.Length > 0 && thisValue[thisValue.Length - 1] == value) return thisValue;
			return thisValue.Append(value);
		}

		[NotNull]
		public static string AppendIfDoesNotEndWith([NotNull] this string thisValue, string value)
		{
			if (string.IsNullOrEmpty(value)) return thisValue;
			return thisValue.EndsWith(value)
				? thisValue
				: thisValue.Append(value);
		}

		public static bool HasMatch([NotNull] this string thisValue, [NotNull] string pattern) { return HasMatch(thisValue, pattern, RegexHelper.OPTIONS_I); }

		public static bool HasMatch([NotNull] this string thisValue, [NotNull] string pattern, RegexOptions options)
		{
			return thisValue.Length != 0 &&
					!string.IsNullOrEmpty(pattern) &&
					Regex.IsMatch(thisValue, pattern, options);
		}

		public static bool HasMatch([NotNull] this string thisValue, [NotNull] Regex expression, int startIndex)
		{
			return thisValue.Length != 0 &&
					InBounds(thisValue, startIndex) &&
					expression.IsMatch(thisValue, startIndex);
		}

		public static bool InBounds([NotNull] this string thisValue, int index) { return thisValue.Length != 0 && index.InRangeRx(0, thisValue.Length); }

		public static bool ContainsAny([NotNull] this string thisValue, [NotNull] params char[] value)
		{
			return thisValue.Length != 0 && ContainsAny(thisValue, 0, thisValue.Length, value);
		}

		public static bool ContainsAny([NotNull] this string thisValue, int startIndex, [NotNull] params char[] value)
		{
			return thisValue.Length != 0 && ContainsAny(thisValue, startIndex, thisValue.Length - startIndex, value);
		}

		public static bool ContainsAny([NotNull] this string thisValue, int startIndex, int count, [NotNull] params char[] value)
		{
			return IndexOfAny(thisValue, startIndex, count, value) > -1;
		}

		public static int IndexOfAny([NotNull] this string thisValue, [NotNull] params char[] value)
		{
			return string.IsNullOrEmpty(thisValue)
						? -1
						: IndexOfAny(thisValue, 0, thisValue.Length, value);
		}

		public static int IndexOfAny([NotNull] this string thisValue, int startIndex, [NotNull] params char[] value)
		{
			return string.IsNullOrEmpty(thisValue)
						? -1
						: IndexOfAny(thisValue, startIndex, thisValue.Length - startIndex, value);
		}

		public static int IndexOfAny([NotNull] this string thisValue, int startIndex, int count, [NotNull] params char[] value)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (string.IsNullOrEmpty(thisValue) || value.IsNullOrEmpty() || count == 0) return -1;

			int lastPos = startIndex + count;

			while (startIndex < lastPos)
			{
				char v = thisValue[startIndex];
				if (value.Contains(v)) return startIndex;
				++startIndex;
			}

			return -1;
		}

		public static int LastIndexOfAny([NotNull] this string thisValue, [NotNull] params char[] value)
		{
			return string.IsNullOrEmpty(thisValue) || value.IsNullOrEmpty()
						? -1
						: LastIndexOfAny(thisValue, thisValue.Length - 1, thisValue.Length, value);
		}

		public static int LastIndexOfAny([NotNull] this string thisValue, int startIndex, [NotNull] params char[] value)
		{
			return string.IsNullOrEmpty(thisValue) || value.IsNullOrEmpty()
						? -1
						: LastIndexOfAny(thisValue, startIndex, startIndex + 1, value);
		}

		public static int LastIndexOfAny([NotNull] this string thisValue, int startIndex, int count, [NotNull] params char[] value)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (string.IsNullOrEmpty(thisValue) || value.IsNullOrEmpty() || count == 0) return -1;

			int lastPos = startIndex - count + 1;

			for (int i = startIndex; i >= lastPos; --i)
			{
				char v = thisValue[i];

				foreach (char c in value)
				{
					if (v == c) return i;
				}
			}

			return -1;
		}

		public static bool IsAscii(this string thisValue) { return !string.IsNullOrEmpty(thisValue) && thisValue.All(c => c.IsAscii()); }

		public static bool IsDigits(this string thisValue) { return !string.IsNullOrEmpty(thisValue) && thisValue.All(c => c.IsDigit()); }

		public static bool IsSame(this string thisValue, string value) { return IsSameOrdinal(thisValue, value); }

		public static bool IsSame([NotNull] this string thisValue, int indexA, [NotNull] string strB, int indexB, int length)
		{
			return IsSameOrdinal(thisValue, indexA, strB, indexB, length);
		}

		public static bool IsSameAsAny(this string thisValue, [NotNull] params string[] values) { return IsSameAsAnyOrdinal(thisValue, values); }

		public static bool IsSameOrdinal(this string thisValue, string value) { return string.Equals(thisValue, value, StringComparison.OrdinalIgnoreCase); }

		public static bool IsSameOrdinal([NotNull] this string thisValue, int indexA, [NotNull] string strB, int indexB, int length)
		{
			return string.Compare(thisValue, indexA, strB, indexB, length, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public static bool IsSameAsAnyOrdinal(this string thisValue, [NotNull] params string[] values)
		{
			if (thisValue == null) return values.Any(e => e == null);
			return thisValue.Length == 0 
				? values.Any(e => e != null && e.Length == 0) 
				: values.Any(value => string.Equals(thisValue, value, StringComparison.OrdinalIgnoreCase));
		}

		public static bool IsSameCurrentCulture(this string thisValue, string value) { return string.Equals(thisValue, value, StringComparison.CurrentCultureIgnoreCase); }

		public static bool IsSameCurrentCulture([NotNull] this string thisValue, int indexA, [NotNull] string strB, int indexB, int length)
		{
			return string.Compare(thisValue, indexA, strB, indexB, length, StringComparison.CurrentCultureIgnoreCase) == 0;
		}

		public static bool IsSameAsAnyCurrentCulture(this string thisValue, [NotNull] params string[] values) { return values.Any(value => string.Equals(thisValue, value, StringComparison.CurrentCultureIgnoreCase)); }

		public static bool IsSameInvariantCulture(this string thisValue, string value) { return string.Equals(thisValue, value, StringComparison.InvariantCultureIgnoreCase); }

		public static bool IsSameInvariantCulture([NotNull] this string thisValue, int indexA, [NotNull] string strB, int indexB, int length)
		{
			return string.Compare(thisValue, indexA, strB, indexB, length, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		public static bool IsSameAsAnyInvariantCulture(this string thisValue, [NotNull] params string[] values) { return values.Any(value => string.Equals(thisValue, value, StringComparison.InvariantCultureIgnoreCase)); }

		public static T IfSame<T>(this string thisValue, string value, T trueResponse, T falseResponse = default(T))
		{
			return IsSame(thisValue, value)
						? trueResponse
						: falseResponse;
		}

		public static T IfSameAsAny<T>(this string thisValue, [NotNull] string[] values, T trueResponse, T falseResponse = default(T))
		{
			return IsSameAsAny(thisValue, values)
						? trueResponse
						: falseResponse;
		}

		public static T IfSameOrdinal<T>(this string thisValue, string value, T trueResponse, T falseResponse = default(T))
		{
			return IsSameOrdinal(thisValue, value)
						? trueResponse
						: falseResponse;
		}

		public static T IfSameAsAnyOrdinal<T>(this string thisValue, [NotNull] string[] values, T trueResponse, T falseResponse = default(T))
		{
			return IsSameAsAnyOrdinal(thisValue, values)
						? trueResponse
						: falseResponse;
		}

		public static T IfSameCurrentCulture<T>(this string thisValue, string value, T trueResponse, T falseResponse = default(T))
		{
			return IsSameCurrentCulture(thisValue, value)
						? trueResponse
						: falseResponse;
		}

		public static T IfSameAsAnyCurrentCulture<T>(this string thisValue, [NotNull] string[] values, T trueResponse, T falseResponse = default(T))
		{
			return IsSameAsAnyCurrentCulture(thisValue, values)
						? trueResponse
						: falseResponse;
		}

		public static T IfSameInvariantCulture<T>(this string thisValue, string value, T trueResponse, T falseResponse = default(T))
		{
			return IsSameInvariantCulture(thisValue, value)
						? trueResponse
						: falseResponse;
		}

		public static T IfSameAsAnyInvariantCulture<T>(this string thisValue, [NotNull] string[] values, T trueResponse, T falseResponse = default(T))
		{
			return IsSameAsAnyInvariantCulture(thisValue, values)
						? trueResponse
						: falseResponse;
		}

		public static bool IsEqual(this string thisValue, string value) { return IsEqualOrdinal(thisValue, value); }

		public static bool IsEqual(this string thisValue, string value, StringComparison comparison) { return string.Compare(thisValue, value, comparison) == 0; }

		public static bool IsEqual(this string thisValue, string value, [NotNull] StringComparer comparer) { return comparer.Equals(thisValue, value); }

		public static bool IsEqual([NotNull] this string thisValue, int indexA, [NotNull] string strB, int indexB, int length)
		{
			return IsEqualOrdinal(thisValue, indexA, strB, indexB, length);
		}

		public static bool IsEqual([NotNull] this string thisValue, int indexA, [NotNull] string strB, int indexB, int length, StringComparison comparison)
		{
			return string.Compare(thisValue, indexA, strB, indexB, length, comparison) == 0;
		}

		public static bool IsEqualToAny(this string thisValue, [NotNull] params string[] values) { return IsEqualToAny(thisValue, values, StringComparison.Ordinal); }

		public static bool IsEqualToAny(this string thisValue, [NotNull] string[] values, StringComparison comparison) { return values.Any(value => string.Equals(thisValue, value, comparison)); }
		public static bool IsEqualToAny(this string thisValue, [NotNull] string[] values, [NotNull] StringComparer comparer) { return values.Any(value => comparer.Equals(thisValue, value)); }

		public static bool IsEqualOrdinal(this string thisValue, string value) { return string.Equals(thisValue, value, StringComparison.Ordinal); }

		public static bool IsEqualOrdinal([NotNull] this string thisValue, int indexA, [NotNull] string strB, int indexB, int length)
		{
			return string.Compare(thisValue, indexA, strB, indexB, length, StringComparison.Ordinal) == 0;
		}

		public static bool IsEqualCurrentCulture(this string thisValue, string value) { return string.Equals(thisValue, value, StringComparison.CurrentCulture); }

		public static bool IsEqualCurrentCulture([NotNull] this string thisValue, int indexA, [NotNull] string strB, int indexB, int length)
		{
			return string.Compare(thisValue, indexA, strB, indexB, length, StringComparison.CurrentCulture) == 0;
		}

		public static bool IsEqualInvariantCulture(this string thisValue, string value) { return string.Equals(thisValue, value, StringComparison.InvariantCulture); }

		public static bool IsEqualInvariantCulture([NotNull] this string thisValue, int indexA, [NotNull] string strB, int indexB, int length)
		{
			return string.Compare(thisValue, indexA, strB, indexB, length, StringComparison.InvariantCulture) == 0;
		}

		public static T IfEqual<T>(this string thisValue, string value, T trueResponse, T falseResponse = default(T))
		{
			return IsEqual(thisValue, value)
						? trueResponse
						: falseResponse;
		}

		public static T IfEqualOrdinal<T>(this string thisValue, string value, T trueResponse, T falseResponse = default(T))
		{
			return IsEqualOrdinal(thisValue, value)
						? trueResponse
						: falseResponse;
		}

		public static T IfEqualCurrentCulture<T>(this string thisValue, string value, T trueResponse, T falseResponse = default(T))
		{
			return IsEqualCurrentCulture(thisValue, value)
						? trueResponse
						: falseResponse;
		}

		public static T IfEqualInvariantCulture<T>(this string thisValue, string value, T trueResponse, T falseResponse = default(T))
		{
			return IsEqualInvariantCulture(thisValue, value)
						? trueResponse
						: falseResponse;
		}

		public static T IfEqualToAny<T>(this string thisValue, [NotNull] string[] values, T trueResponse, T falseResponse = default(T))
		{
			return IsEqualToAny(thisValue, values)
						? trueResponse
						: falseResponse;
		}

		public static T IfEqualToAny<T>(this string thisValue, [NotNull] string[] values, StringComparison comparison, T trueResponse, T falseResponse = default(T))
		{
			return IsEqualToAny(thisValue, values, comparison)
						? trueResponse
						: falseResponse;
		}

		public static T IfEqualToAny<T>(this string thisValue, [NotNull] string[] values, [NotNull] StringComparer comparer, T trueResponse, T falseResponse = default(T))
		{
			return IsEqualToAny(thisValue, values, comparer)
						? trueResponse
						: falseResponse;
		}

		public static bool ContainsAny(this string thisValue, bool ignoreCase, [NotNull] params string[] values) { return ContainsAnyOrdinal(thisValue, ignoreCase, values); }

		public static bool ContainsOrdinal(this string thisValue, string value, bool ignoreCase = true)
		{
			if (thisValue == null && value == null) return true;
			if (thisValue == null || value == null) return false;
			return thisValue.Contains(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		}

		public static bool ContainsAnyOrdinal(this string thisValue, bool ignoreCase, [NotNull] params string[] values)
		{
			if (thisValue == null) return values.Length == 0 || values.Any(e => e == null);
			if (string.IsNullOrEmpty(thisValue)) return values.Length > 0 && values.Any(e => e != null && e.Length == 0);
			StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			return values.Any(value => thisValue.Contains(value, comparison));
		}

		public static bool ContainsCurrentCulture(this string thisValue, string value, bool ignoreCase = true)
		{
			if (thisValue == null && value == null) return true;
			if (thisValue == null || value == null) return false;
			return thisValue.Contains(value, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
		}

		public static bool ContainsAnyCurrentCulture(this string thisValue, bool ignoreCase, [NotNull] params string[] values)
		{
			if (thisValue == null) return values.Length == 0 || values.Any(e => e == null);
			if (string.IsNullOrEmpty(thisValue)) return values.Length > 0 && values.Any(e => e != null && e.Length == 0);
			StringComparison comparison = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
			return values.Any(value => thisValue.Contains(value, comparison));
		}

		public static bool ContainsInvariantCulture(this string thisValue, string value, bool ignoreCase = true)
		{
			if (thisValue == null && value == null) return true;
			if (thisValue == null || value == null) return false;
			return thisValue.Contains(value, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
		}

		public static bool ContainsAnyInvariantCulture(this string thisValue, bool ignoreCase, [NotNull] params string[] values)
		{
			if (thisValue == null) return values.Length == 0 || values.Any(e => e == null);
			if (string.IsNullOrEmpty(thisValue)) return values.Length > 0 && values.Any(e => e != null && e.Length == 0);
			StringComparison comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
			return values.Any(value => thisValue.Contains(value, comparison));
		}

		public static bool StartsWithAny(this string thisValue, bool ignoreCase, [NotNull] params string[] values) { return StartsWithAnyOrdinal(thisValue, ignoreCase, values); }

		public static bool StartsWithOrdinal(this string thisValue, string value, bool ignoreCase = true)
		{
			if (thisValue == null && value == null) return true;
			if (thisValue == null || value == null) return false;
			return thisValue.StartsWith(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		}

		public static bool StartsWithAnyOrdinal(this string thisValue, bool ignoreCase, [NotNull] params string[] values)
		{
			if (thisValue == null) return values.Length == 0 || values.Any(e => e == null);
			if (string.IsNullOrEmpty(thisValue)) return values.Length > 0 && values.Any(e => e != null && e.Length == 0);
			StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			return values.Any(value => thisValue.StartsWith(value, comparison));
		}

		public static bool StartsWithCurrentCulture(this string thisValue, string value, bool ignoreCase = true)
		{
			if (thisValue == null && value == null) return true;
			if (thisValue == null || value == null) return false;
			return thisValue.StartsWith(value, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
		}

		public static bool StartsWithAnyCurrentCulture(this string thisValue, bool ignoreCase, [NotNull] params string[] values)
		{
			if (thisValue == null) return values.Length == 0 || values.Any(e => e == null);
			if (string.IsNullOrEmpty(thisValue)) return values.Length > 0 && values.Any(e => e != null && e.Length == 0);
			StringComparison comparison = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
			return values.Any(value => thisValue.StartsWith(value, comparison));
		}

		public static bool StartsWithInvariantCulture(this string thisValue, string value, bool ignoreCase = true)
		{
			if (thisValue == null && value == null) return true;
			if (thisValue == null || value == null) return false;
			return thisValue.StartsWith(value, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
		}

		public static bool StartsWithAnyInvariantCulture(this string thisValue, bool ignoreCase, [NotNull] params string[] values)
		{
			if (thisValue == null) return values.Length == 0 || values.Any(e => e == null);
			if (string.IsNullOrEmpty(thisValue)) return values.Length > 0 && values.Any(e => e != null && e.Length == 0);
			StringComparison comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
			return values.Any(value => thisValue.StartsWith(value, comparison));
		}

		public static bool EndsWithAny(this string thisValue, bool ignoreCase, [NotNull] params string[] values) { return EndsWithAnyOrdinal(thisValue, ignoreCase, values); }

		public static bool EndsWithOrdinal(this string thisValue, string value, bool ignoreCase = true)
		{
			if (thisValue == null && value == null) return true;
			if (thisValue == null || value == null) return false;
			return thisValue.EndsWith(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		}

		public static bool EndsWithAnyOrdinal(this string thisValue, bool ignoreCase, [NotNull] params string[] values)
		{
			if (thisValue == null) return values.Length == 0 || values.Any(e => e == null);
			if (string.IsNullOrEmpty(thisValue)) return values.Length > 0 && values.Any(e => e != null && e.Length == 0);
			StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			return values.Any(value => thisValue.EndsWith(value, comparison));
		}

		public static bool EndsWithCurrentCulture(this string thisValue, string value, bool ignoreCase = true)
		{
			if (thisValue == null && value == null) return true;
			if (thisValue == null || value == null) return false;
			return thisValue.EndsWith(value, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
		}

		public static bool EndsWithAnyCurrentCulture(this string thisValue, bool ignoreCase, [NotNull] params string[] values)
		{
			if (thisValue == null) return values.Length == 0 || values.Any(e => e == null);
			if (string.IsNullOrEmpty(thisValue)) return values.Length > 0 && values.Any(e => e != null && e.Length == 0);
			StringComparison comparison = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
			return values.Any(value => thisValue.EndsWith(value, comparison));
		}

		public static bool EndsWithInvariantCulture(this string thisValue, string value, bool ignoreCase = true)
		{
			if (thisValue == null && value == null) return true;
			if (thisValue == null || value == null) return false;
			return thisValue.EndsWith(value, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
		}

		public static bool EndsWithAnyInvariantCulture(this string thisValue, bool ignoreCase, [NotNull] params string[] values)
		{
			if (thisValue == null) return values.Length == 0 || values.Any(e => e == null);
			if (string.IsNullOrEmpty(thisValue)) return values.Length > 0 && values.Any(e => e != null && e.Length == 0);
			StringComparison comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
			return values.Any(value => thisValue.EndsWith(value, comparison));
		}

		public static string IfNullOrEmpty(this string thisValue, string trueResponse) { return IfNullOrEmpty(thisValue, trueResponse, thisValue); }

		public static T IfNullOrEmpty<T>(this string thisValue, T trueResponse, T falseResponse = default(T))
		{
			return string.IsNullOrEmpty(thisValue)
						? trueResponse
						: falseResponse;
		}

		public static string IfNullOrEmpty(this string thisValue, [NotNull] Func<string> trueFunc, Func<string> falseFunc = null)
		{
			return string.IsNullOrEmpty(thisValue)
						? trueFunc()
						: falseFunc == null
							? thisValue
							: falseFunc();
		}

		public static string IfNotNullOrEmpty([NotNull] this string thisValue, string trueResponse) { return IfNotNullOrEmpty(thisValue, trueResponse, thisValue); }

		public static T IfNotNullOrEmpty<T>([NotNull] this string thisValue, T trueResponse, T falseResponse = default(T))
		{
			return thisValue.Length != 0
						? trueResponse
						: falseResponse;
		}

		public static string IfNotNullOrEmpty([NotNull] this string thisValue, [NotNull] Func<string> trueFunc, Func<string> falseFunc = null)
		{
			return thisValue.Length != 0
						? trueFunc()
						: falseFunc == null
							? thisValue
							: falseFunc();
		}

		public static string IfNullOrWhiteSpace(this string thisValue, string trueResponse) { return IfNullOrWhiteSpace(thisValue, trueResponse, thisValue); }

		public static T IfNullOrWhiteSpace<T>(this string thisValue, T trueResponse, T falseResponse = default(T))
		{
			return string.IsNullOrWhiteSpace(thisValue)
						? trueResponse
						: falseResponse;
		}

		public static string IfNullOrWhiteSpace(this string thisValue, [NotNull] Func<string> trueFunc, Func<string> falseFunc = null)
		{
			return string.IsNullOrWhiteSpace(thisValue)
						? trueFunc()
						: falseFunc == null
							? thisValue
							: falseFunc();
		}

		public static string IfNotNullOrWhiteSpace(this string thisValue, string trueResponse) { return IfNotNullOrWhiteSpace(thisValue, trueResponse, thisValue); }

		public static T IfNotNullOrWhiteSpace<T>(this string thisValue, T trueResponse, T falseResponse = default)
		{
			return !string.IsNullOrWhiteSpace(thisValue)
						? trueResponse
						: falseResponse;
		}

		public static string IfNotNullOrWhiteSpace(this string thisValue, [NotNull] Func<string> trueFunc, Func<string> falseFunc = null)
		{
			return !string.IsNullOrWhiteSpace(thisValue)
						? trueFunc()
						: falseFunc == null
							? thisValue
							: falseFunc();
		}

		public static int Compare(this string thisValue, string value, [NotNull] StringComparer comparer) { return comparer.Compare(thisValue, value); }

		public static int CompareOrdinal(this string thisValue, string value, bool ignoreCase = true)
		{
			return Compare(thisValue, value, ignoreCase
												? StringComparer.OrdinalIgnoreCase
												: StringComparer.Ordinal);
		}

		public static int CompareCurrentCulture(this string thisValue, string value, bool ignoreCase = true)
		{
			return Compare(thisValue, value, ignoreCase
												? StringComparer.CurrentCultureIgnoreCase
												: StringComparer.CurrentCulture);
		}

		public static int CompareInvariantCulture(this string thisValue, string value, bool ignoreCase = true)
		{
			return Compare(thisValue, value, ignoreCase
												? StringComparer.InvariantCultureIgnoreCase
												: StringComparer.InvariantCulture);
		}

		public static bool IsLetterOrDigit(this string thisValue) { return !string.IsNullOrEmpty(thisValue) && thisValue.All(c => c.IsLetterOrDigit()); }

		public static bool IsLetters(this string thisValue) { return !string.IsNullOrEmpty(thisValue) && thisValue.All(c => c.IsLetter()); }

		public static bool IsLower(this string thisValue) { return !string.IsNullOrEmpty(thisValue) && thisValue.All(c => c.IsLower()); }

		public static bool IsNumbers(this string thisValue) { return !string.IsNullOrEmpty(thisValue) && thisValue.All(c => c.IsNumber()); }

		public static bool IsUpper(this string thisValue) { return !string.IsNullOrEmpty(thisValue) && thisValue.All(c => c.IsUpper()); }

		[NotNull]
		public static string Left([NotNull] this string thisValue, int length = -1)
		{
			if (length == -1) length = thisValue.Length;
			if (!length.InRange(0, thisValue.Length)) throw new ArgumentOutOfRangeException(nameof(length));
			return length == 0
						? string.Empty
						: length == thisValue.Length
							? thisValue
							: thisValue.Substring(0, length);
		}

		[NotNull]
		public static string LeftMax([NotNull] this string thisValue, int length = -1) { return Left(thisValue, length.NotAbove(thisValue.Length)); }

		[NotNull]
		public static string Right([NotNull] this string thisValue, int length = -1)
		{
			if (length == -1) length = thisValue.Length;
			if (!length.InRange(0, thisValue.Length)) throw new ArgumentOutOfRangeException(nameof(length));
			return length == 0
						? string.Empty
						: length == thisValue.Length
							? thisValue
							: thisValue.Substring(thisValue.Length - length, length);
		}

		[NotNull]
		public static string RightMax([NotNull] this string thisValue, int length = -1) { return Right(thisValue, length.NotAbove(thisValue.Length)); }

		public static bool IsLike(this string thisValue, string pattern) { return IsLike(thisValue, pattern, RegexHelper.OPTIONS_I); }

		public static bool IsLike(this string thisValue, string pattern, RegexOptions options)
		{
			if (thisValue == null && pattern == null) return true;
			if (thisValue == null || pattern == null) return false;
			string pat = RegexHelper.FromWildCards(pattern);
			return !string.IsNullOrEmpty(pat) && Regex.IsMatch(thisValue, pat, options);
		}

		public static bool IsMatch(this string thisValue, string pattern) { return IsMatch(thisValue, pattern, RegexHelper.OPTIONS_I); }

		public static bool IsMatch(this string thisValue, string pattern, RegexOptions options)
		{
			if (thisValue == null && pattern == null) return true;
			if (thisValue == null || pattern == null) return false;
			return Regex.IsMatch(thisValue, pattern, options);
		}

		public static bool IsMatch([NotNull] this string thisValue, [NotNull] Regex expression, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return expression.IsMatch(thisValue, startIndex);
		}

		[NotNull]
		public static Match Match(this string thisValue, string pattern) { return Match(thisValue, pattern, RegexHelper.OPTIONS_I); }

		[NotNull]
		public static Match Match(this string thisValue, string pattern, RegexOptions options)
		{
			if (thisValue == null || pattern == null) return System.Text.RegularExpressions.Match.Empty;
			return string.IsNullOrEmpty(thisValue) || pattern.Length == 0 ? System.Text.RegularExpressions.Match.Empty : Regex.Match(thisValue, pattern, options);
		}

		[NotNull]
		public static Match Match([NotNull] this string thisValue, [NotNull] Regex expression, int startIndex = 0, int count = -1)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (string.IsNullOrEmpty(thisValue) || count == 0) return System.Text.RegularExpressions.Match.Empty;
			return expression.Match(thisValue, startIndex, count);
		}

		public static MatchCollection Matches(this string thisValue, string pattern) { return Matches(thisValue, pattern, RegexHelper.OPTIONS_I); }

		public static MatchCollection Matches(this string thisValue, string pattern, RegexOptions options)
		{
			return string.IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(pattern)
						? null
						: Regex.Matches(thisValue, pattern, options);
		}

		[NotNull]
		public static MatchCollection Matches([NotNull] this string thisValue, [NotNull] Regex expression, int startIndex)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return expression.Matches(thisValue, startIndex);
		}

		[NotNull]
		public static string Remove([NotNull] this string thisValue, [NotNull] params char[] remove) { return Remove(thisValue, 0, thisValue.Length, remove); }

		[NotNull]
		public static string Remove([NotNull] this string thisValue, int startIndex, [NotNull] params char[] remove)
		{
			return Remove(thisValue, startIndex, thisValue.Length - startIndex, remove);
		}

		[NotNull]
		public static string Remove([NotNull] this string thisValue, int startIndex, int count, [NotNull] params char[] remove)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (string.IsNullOrEmpty(thisValue) || remove.Length == 0 || count == 0) return thisValue;

			string ch = remove.ToString(0);
			if (string.IsNullOrEmpty(ch)) return thisValue;
			Regex expression = new Regex($"[{Regex.Escape(ch)}]", RegexHelper.OPTIONS | RegexOptions.Multiline);
			return expression.Replace(thisValue, string.Empty, count, startIndex);
		}

		[NotNull]
		public static string Remove([NotNull] this string thisValue, [NotNull] string remove) { return thisValue.Replace(remove, string.Empty); }

		[NotNull]
		public static string Remove([NotNull] this string thisValue, string remove, int startIndex, int count) { return Remove(thisValue, remove, true, startIndex, count); }

		[NotNull]
		public static string Remove([NotNull] this string thisValue, string remove, bool ignoreCase, int startIndex, int count)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (string.IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(remove) || count == 0) return string.Empty;
			Regex expression = new Regex(remove, ignoreCase ? RegexHelper.OPTIONS_I : RegexHelper.OPTIONS | RegexOptions.Multiline);
			return expression.Replace(thisValue, string.Empty, count, startIndex);
		}

		public static string RemovePrefix(this string thisValue, string value, bool ignoreCase = true)
		{
			thisValue = thisValue?.Trim();
			if (string.IsNullOrEmpty(thisValue)) return thisValue;
			value = value?.Trim();
			if (string.IsNullOrEmpty(value)) return thisValue;
			if (thisValue.StartsWith(value, ignoreCase, CultureInfo.InvariantCulture)) thisValue = thisValue.Substring(value.Length);
			return thisValue;
		}

		public static string RemoveSuffix(this string thisValue, string value, bool ignoreCase = true)
		{
			thisValue = thisValue?.Trim();
			if (string.IsNullOrEmpty(thisValue)) return thisValue;
			value = value?.Trim();
			if (string.IsNullOrEmpty(value)) return thisValue;
			if (thisValue.EndsWith(value, ignoreCase, CultureInfo.InvariantCulture)) thisValue = thisValue.Substring(0, thisValue.Length - value.Length);
			return thisValue;
		}

		public static string Replace(this string thisValue, string pattern, string replace, RegexOptions options)
		{
			/*
			 * For regular expression, you can use the following to test this method:
			 * thisValue => The capacity is adjusted as needed.
			 * pattern => \b(\w+)\s+\1\b
			 * replace => $1
			 * 
			 * This will replace the duplicated word 'adjusted' with only one
			 * The key to achieve this is by using the captured group $1 
			 * which is (\w+)
			*/
			if (string.IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(pattern)) return thisValue;
			return Regex.Replace(thisValue, pattern, replace ?? string.Empty, options);
		}

		[NotNull]
		public static string Replace([NotNull] this string thisValue, string replace, [NotNull] Regex expression, int startIndex = 0, int count = -1)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (string.IsNullOrEmpty(thisValue) || count == 0) return thisValue;
			return expression.Replace(thisValue, replace ?? string.Empty, count, startIndex);
		}

		public static string Replace(this string thisValue, string pattern, [NotNull] MatchEvaluator evaluator, RegexOptions options = RegexHelper.OPTIONS_I)
		{
			if (string.IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(pattern)) return thisValue;
			return Regex.Replace(thisValue, pattern, evaluator, options);
		}

		[NotNull]
		public static string Replace([NotNull] this string thisValue, [NotNull] Regex expression, [NotNull] MatchEvaluator evaluator, int startIndex = 0, int count = -1)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (string.IsNullOrEmpty(thisValue) || count == 0) return thisValue;
			return expression.Replace(thisValue, evaluator, count, startIndex);
		}

		public static string Replace(this string thisValue, char replace, [NotNull] params char[] anyOf)
		{
			if (string.IsNullOrEmpty(thisValue) || anyOf.Length == 0) return thisValue;

			StringBuilder sb = new StringBuilder(thisValue.Length);

			foreach (char c in thisValue)
				sb.Append(anyOf.Contains(c) ? replace : c);

			return sb.ToString();
		}

		public static string Replace(this string thisValue, string replace, [NotNull] params string[] anyOf)
		{
			if (string.IsNullOrEmpty(thisValue) || anyOf.Length == 0) return thisValue;

			StringBuilder result = new StringBuilder(thisValue);

			foreach (string s in anyOf)
			{
				if (string.IsNullOrEmpty(s)) continue;
				result.Replace(s, replace);
			}

			return result.ToString();
		}

		public static string Replace(this string thisValue, [NotNull] params string[][] anyOf)
		{
			if (string.IsNullOrEmpty(thisValue) || anyOf.Length == 0) return thisValue;

			StringBuilder sb = new StringBuilder(thisValue);

			foreach (string[] strings in anyOf)
			{
				if (strings.Rank != 1) throw new RankException();
				if (strings.Length == 0 || string.IsNullOrEmpty(strings[0])) continue;

				switch (strings.Length)
				{
					case 1:
						sb.Replace(strings[0], string.Empty);
						break;
					default:
						sb.Replace(strings[0], strings[1] ?? string.Empty);
						break;
				}
			}

			return sb.ToString();
		}

		[NotNull] 
		public static string Replace([NotNull] this string thisValue, string oldValue, string newValue, StringComparison comparison) { return Replace(thisValue, oldValue, newValue, comparison, 0, -1); }
		[NotNull] 
		public static string Replace([NotNull] this string thisValue, string oldValue, string newValue, StringComparison comparison, int startIndex) { return Replace(thisValue, oldValue, newValue, comparison, startIndex, -1); }
		[NotNull]
		public static string Replace([NotNull] this string thisValue, string oldValue, string newValue, StringComparison comparison, int startIndex, int count)
		{
			CultureInfo cultureInfo;
			bool ignoreCase;
			bool ordinal;

			switch (comparison)
			{
				case StringComparison.CurrentCulture:
					cultureInfo = CultureInfo.CurrentCulture;
					ignoreCase = false;
					ordinal = false;
					break;
				case StringComparison.CurrentCultureIgnoreCase:
					cultureInfo = CultureInfo.CurrentCulture;
					ignoreCase = true;
					ordinal = false;
					break;
				case StringComparison.InvariantCulture:
				case StringComparison.Ordinal:
					cultureInfo = CultureInfo.InvariantCulture;
					ignoreCase = false;
					ordinal = true;
					break;
				case StringComparison.InvariantCultureIgnoreCase:
				case StringComparison.OrdinalIgnoreCase:
					cultureInfo = CultureInfo.InvariantCulture;
					ignoreCase = true;
					ordinal = true;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
			}

			return Replace(thisValue, oldValue, newValue, ignoreCase, ordinal, cultureInfo.CompareInfo, startIndex, count);
		}

		[NotNull]
		public static string Replace([NotNull] this string thisValue, string oldValue, string newValue, [NotNull] CultureInfo cultureInfo) { return Replace(thisValue, oldValue, newValue, false, cultureInfo, 0, -1); }
		[NotNull]
		public static string Replace([NotNull] this string thisValue, string oldValue, string newValue, [NotNull] CultureInfo cultureInfo, int startIndex) { return Replace(thisValue, oldValue, newValue, false, cultureInfo, startIndex, -1); }
		[NotNull]
		public static string Replace([NotNull] this string thisValue, string oldValue, string newValue, [NotNull] CultureInfo cultureInfo, int startIndex, int count) { return Replace(thisValue, oldValue, newValue, false, cultureInfo, startIndex, count); }
		[NotNull]
		public static string Replace([NotNull] this string thisValue, string oldValue, string newValue, bool ignoreCase, [NotNull] CultureInfo cultureInfo) { return Replace(thisValue, oldValue, newValue, ignoreCase, cultureInfo, 0, -1); }
		[NotNull]
		public static string Replace([NotNull] this string thisValue, string oldValue, string newValue, bool ignoreCase, [NotNull] CultureInfo cultureInfo, int startIndex) { return Replace(thisValue, oldValue, newValue, ignoreCase, cultureInfo, startIndex, -1); }
		[NotNull]
		public static string Replace([NotNull] this string thisValue, string oldValue, string newValue, bool ignoreCase, [NotNull] CultureInfo cultureInfo, int startIndex, int count) { return Replace(thisValue, oldValue, newValue, ignoreCase, Equals(cultureInfo, CultureInfo.InvariantCulture), cultureInfo.CompareInfo, startIndex, count); }

		public static string Reverse(this string thisValue, [NotNull] params char[] separator)
		{
			if (string.IsNullOrEmpty(thisValue)) return thisValue;

			if (separator.IsNullOrEmpty())
			{
				char[] ch = thisValue.ToCharArray();
				Array.Reverse(ch);
				return new string(ch);
			}

			string pattern = string.Format(RegexHelper.RGX_PARTITIONS_P, Regex.Escape(separator.ToString(0)).Replace("-", @"\-"));
			List<string> parts = new List<string>();
			Match m = Regex.Match(thisValue, pattern, RegexHelper.OPTIONS_I);

			while (m.Success)
			{
				if (m.Length == 0)
				{
					m = m.NextMatch();
					continue;
				}

				parts.Add(m.Value);
				m = m.NextMatch();
			}

			if (parts.Count == 0) return null;

			StringBuilder sb = new StringBuilder();

			sb.EnsureCapacity(thisValue.Length);
			sb.Append(parts[parts.Count - 1]);
			parts.RemoveAt(parts.Count - 1);
			parts.Reverse();

			foreach (string part in parts)
			{
				sb.Append(part[part.Length - 1]);
				if (part.Length > 1) sb.Append(part.Substring(0, part.Length - 1));
			}

			return sb.ToString();
		}

		public static string Separator(this string thisValue, char separator)
		{
			return string.IsNullOrEmpty(thisValue)
						? thisValue
						: string.Concat(thisValue, separator);
		}

		public static string Separator(this string thisValue, string separator)
		{
			return string.IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(separator)
						? thisValue
						: string.Concat(thisValue, separator);
		}

		public static string Prefix(this string thisValue, char prefix)
		{
			if (string.IsNullOrEmpty(thisValue) || thisValue[0] == prefix) return thisValue;
			return string.Concat(prefix, thisValue);
		}

		public static string Prefix(this string thisValue, string prefix, bool ignoreCase = true)
		{
			if (string.IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(prefix) || StartsWithOrdinal(thisValue, prefix, ignoreCase)) return thisValue;
			return string.Concat(prefix, thisValue);
		}

		public static string Suffix(this string thisValue, char suffix)
		{
			if (string.IsNullOrEmpty(thisValue) || thisValue[thisValue.Length - 1] == suffix) return thisValue;
			return string.Concat(suffix, thisValue);
		}

		public static string Suffix(this string thisValue, string suffix, bool ignoreCase = true)
		{
			if (string.IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(suffix) || EndsWithOrdinal(thisValue, suffix, ignoreCase)) return thisValue;
			return string.Concat(suffix, thisValue);
		}

		[NotNull]
		public static string Quote(this string thisValue)
		{
			bool lq, rq;

			if (string.IsNullOrEmpty(thisValue))
				lq = rq = true;
			else
			{
				lq = thisValue[0] != '\"';
				rq = thisValue[thisValue.Length - 1] != '\"';
			}

			return $"{(lq ? "\"" : string.Empty)}{thisValue ?? string.Empty}{(rq ? "\"" : string.Empty)}";
		}

		[NotNull]
		public static string SingleQuote(this string thisValue)
		{
			bool lq, rq;

			if (string.IsNullOrEmpty(thisValue))
				lq = rq = true;
			else
			{
				lq = thisValue[0] != '\'';
				rq = thisValue[thisValue.Length - 1] != '\'';
			}

			return $"{(lq ? "\'" : string.Empty)}{thisValue ?? string.Empty}{(rq ? "\'" : string.Empty)}";
		}

		[NotNull]
		public static string[] Split([NotNull] this string thisValue, [NotNull] params char[] separator)
		{
			return string.IsNullOrEmpty(thisValue)
						? Array.Empty<string>()
						: thisValue.Split(separator, StringSplitOptions.None);
		}

		[NotNull]
		public static string[] Split([NotNull] this string thisValue, StringSplitOptions options, [NotNull] params char[] separator)
		{
			return string.IsNullOrEmpty(thisValue)
						? Array.Empty<string>()
						: thisValue.Split(separator, options);
		}

		[NotNull]
		public static string[] Split([NotNull] this string thisValue, int count, [NotNull] params char[] separator)
		{
			return string.IsNullOrEmpty(thisValue)
						? Array.Empty<string>()
						: thisValue.Split(separator, count);
		}

		[NotNull]
		public static string[] Split([NotNull] this string thisValue, int count, StringSplitOptions options, [NotNull] params char[] separator)
		{
			return string.IsNullOrEmpty(thisValue)
						? Array.Empty<string>()
						: thisValue.Split(separator, count, options);
		}

		[NotNull]
		public static string[] Split([NotNull] this string thisValue, [NotNull] params string[] separator)
		{
			return string.IsNullOrEmpty(thisValue)
						? Array.Empty<string>()
						: thisValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		}

		[NotNull]
		public static string[] Split([NotNull] this string thisValue, StringSplitOptions options, [NotNull] params string[] separator)
		{
			return string.IsNullOrEmpty(thisValue)
						? Array.Empty<string>()
						: thisValue.Split(separator, options);
		}

		[NotNull]
		public static string[] Split([NotNull] this string thisValue, int count, StringSplitOptions options, [NotNull] params string[] separator)
		{
			return string.IsNullOrEmpty(thisValue)
						? Array.Empty<string>()
						: thisValue.Split(separator, count, options);
		}

		[NotNull]
		public static string[] Split([NotNull] this string thisValue, [NotNull] string pattern) { return Split(thisValue, pattern, RegexHelper.OPTIONS_I); }

		[NotNull]
		public static string[] Split([NotNull] this string thisValue, [NotNull] string pattern, RegexOptions options)
		{
			return string.IsNullOrEmpty(thisValue)
						? Array.Empty<string>()
						: Regex.Split(thisValue, pattern, options);
		}

		public static int ByteSize(this string thisValue, Encoding encoding = null)
		{
			if (string.IsNullOrEmpty(thisValue)) return 0;
			return encoding?.GetByteCount(thisValue) ?? thisValue.Length * sizeof(char);
		}

		[NotNull]
		public static byte[] ToBytes(this string thisValue, Encoding encoding = null)
		{
			return string.IsNullOrEmpty(thisValue)
						? Array.Empty<byte>()
						: (encoding ?? EncodingHelper.Default).GetBytes(thisValue);
		}

		public static string ToTitleCase(this string thisValue, CultureInfo culture = null)
		{
			return string.IsNullOrEmpty(thisValue)
						? thisValue
						: (culture ?? CultureInfoHelper.Default).TextInfo.ToTitleCase(thisValue);
		}

		public static string UnQuote(this string thisValue)
		{
			return string.IsNullOrEmpty(thisValue)
						? thisValue
						: thisValue.Trim('\'', '\"');
		}

		[NotNull]
		public static IList<string> Words(this string thisValue)
		{
			if (string.IsNullOrEmpty(thisValue)) return Array.Empty<string>();

			List<string> strings = new List<string>();
			Match m = Regex.Match(thisValue, RGX_WORDS, RegexHelper.OPTIONS_I);

			while (m.Success)
			{
				if (m.Length == 0)
				{
					m = m.NextMatch();
					continue;
				}

				strings.Add(m.Value);
				m = m.NextMatch();
			}

			return strings;
		}

		public static string Escape(this string thisValue)
		{
			return string.IsNullOrEmpty(thisValue)
						? thisValue
						: Regex.Escape(thisValue);
		}

		[NotNull]
		public static string Unescape([NotNull] this string thisValue)
		{
			return string.IsNullOrEmpty(thisValue)
						? thisValue
						: Regex.Unescape(thisValue);
		}

		[NotNull]
		public static string Surrogate([NotNull] this string thisValue)
		{
			// Converts "a\u0304\u0308bc\u0327" into a normal string
			if (string.IsNullOrEmpty(thisValue)) return thisValue;

			StringBuilder sb = new StringBuilder();
			TextElementEnumerator ce = StringInfo.GetTextElementEnumerator(thisValue);

			while (ce.MoveNext()) sb.Append(ce.GetTextElement());

			return sb.ToString();
		}

		public static string ToBase64String(this string thisValue, Encoding encoding = null)
		{
			if (string.IsNullOrEmpty(thisValue)) return thisValue;
			byte[] bytes = thisValue.ToBytes(encoding ?? EncodingHelper.Default);
			return bytes.Length == 0
				? string.Empty 
				: Convert.ToBase64String(bytes);
		}

		public static string FromBase64String(this string thisValue, Encoding encoding)
		{
			if (string.IsNullOrEmpty(thisValue)) return thisValue;
			byte[] bytes = Convert.FromBase64String(thisValue);
			return (encoding ?? EncodingHelper.Default).GetString(bytes);
		}

		public static char FindSeparator(string thisValue, char defaultValue, [NotNull] params char[] separators)
		{
			if (string.IsNullOrEmpty(thisValue)) return defaultValue;

			List<char> separator = new List<char>();
			if (!separators.IsNullOrEmpty()) separator.AddRange(separators);
			if (separator.Count == 0) separator.AddRange(CultureInfoHelper.GetDefaultListSeparators());

			bool quoted = false;
			bool firstChar = true;
			int[] separatorsCount = new int[separator.Count];

			for (int i = 0; i < thisValue.Length; i++)
			{
				switch (thisValue[i])
				{
					case '"':
						if (quoted)
						{
							int n = i + 1;

							if (n >= thisValue.Length || thisValue[n] != '"') // Value is quoted and current character is " and next character is not ".
								quoted = false;
							else i = n; // Value is quoted and current and next characters are string.Empty - read (skip) peeked quote.
						}
						else
						{
							if (firstChar) // Set value as quoted only if this quote is the first char in the value.
								quoted = true;
						}
						break;
					case '\n':
						if (!quoted)
						{
							firstChar = true;
							continue;
						}
						break;
					default:
						if (!quoted)
						{
							int index = separator.IndexOf(thisValue[i]);

							if (index != -1)
							{
								++separatorsCount[index];
								firstChar = true;
								continue;
							}
						}
						break;
				}

				if (firstChar) firstChar = false;
			}

			int maxCount = separatorsCount.Max();
			return maxCount == 0 ? defaultValue : separator[separatorsCount.IndexOf(maxCount)];
		}

		[NotNull]
		public static string Join(this string thisValue, char separator, [NotNull] params object[] objects) { return Join(thisValue, objects, separator); }

		[NotNull]
		public static string Join(this string thisValue, [NotNull] IEnumerable enumerable, char separator)
		{
		   StringBuilder sb = new StringBuilder(thisValue);
			sb.Join(enumerable, separator);
			return sb.ToString();
		}

		[NotNull]
		public static string Join(this string thisValue, string separator, [NotNull] params object[] objects) { return Join(thisValue, objects, separator); }

		[NotNull]
		public static string Join(this string thisValue, [NotNull] IEnumerable enumerable, string separator)
		{
		   StringBuilder sb = new StringBuilder(thisValue);
			sb.Join(enumerable, separator);
			return sb.ToString();
		}

		[NotNull]
		public static string Join<TKey, TValue>([NotNull] this string thisValue, [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> enumerable, char separator, char group)
		{
			StringBuilder sb = new StringBuilder(thisValue);
			sb.Join(enumerable, separator, group);
			return sb.ToString();
		}

		[NotNull]
		public static string Join<TKey, TValue>([NotNull] this string thisValue, [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> enumerable, string separator, string group)
		{
			StringBuilder sb = new StringBuilder(thisValue);
			sb.Join(enumerable, separator, group);
			return sb.ToString();
		}

		public static IEnumerable<string> Partition(this string thisValue, [NotNull] params char[] separator) { return Partition(thisValue, false, separator); }

		public static IEnumerable<string> Partition(this string thisValue, bool includeSeparator, [NotNull] params char[] separator)
		{
			return Partition(thisValue, includeSeparator, RegexHelper.OPTIONS_I, separator);
		}

		[ItemNotNull]
		public static IEnumerable<string> Partition(this string thisValue, bool includeSeparator, RegexOptions options, [NotNull] params char[] separator)
		{
			if (string.IsNullOrEmpty(thisValue)) yield break;

			if (separator.IsNullOrEmpty())
				yield return thisValue;
			else
			{
				string separators = Regex.Escape(separator.ToString(0)).Replace("-", @"\-");
				string pattern = string.Format(includeSeparator ? RegexHelper.RGX_PARTITIONS_P : RGX_PARTITIONS, separators);
				Match m = Regex.Match(thisValue, pattern, options);

				while (m.Success)
				{
					if (m.Length == 0)
					{
						m = m.NextMatch();
						continue;
					}

					yield return m.Value;
					m = m.NextMatch();
				}
			}
		}

		public static IEnumerable<string> Partition([NotNull] this string thisValue, int size, PartitionSize type = PartitionSize.PerPartition, char padLeft = '\0')
		{
			return Partition(thisValue, 0, thisValue.Length, size, type, padLeft);
		}

		[ItemNotNull]
		public static IEnumerable<string> Partition([NotNull] this string thisValue, int startIndex, int count, int size, PartitionSize type = PartitionSize.PerPartition, char padLeft = '\0')
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (size == 0 || count == 0 || string.IsNullOrEmpty(thisValue)) yield break;

			int lastPos = startIndex + count;
			int sz;

			switch (type)
			{
				case PartitionSize.TotalCount:
					sz = (int)Math.Ceiling(count / (double)size);
					break;
				default:
					sz = size.NotAbove(count);
					break;
			}

			if (padLeft != '\0')
			{
				for (int i = startIndex; i < lastPos; i += size)
				{
					int n = Math.Min(sz, lastPos - i);

					if (n == sz)
						yield return thisValue.Substring(i, n);
					else
						yield return thisValue.Substring(i, n).PadLeft(sz, padLeft);
				}
			}
			else
			{
				for (int i = startIndex; i < lastPos; i += size)
				{
					yield return thisValue.Substring(i, Math.Min(sz, lastPos - i));
				}
			}
		}

		public static string Patternize(this string thisValue, char separator, [NotNull] params int[] positions) { return Patternize(thisValue, separator, false, positions); }

		public static string Patternize(this string thisValue, char separator, bool rightToLeft, [NotNull] params int[] positions)
		{
			return Patternize(thisValue, separator, rightToLeft, true, positions);
		}

		public static string Patternize(this string thisValue, char separator, bool rightToLeft, bool allString, [NotNull] params int[] positions)
		{
			return Patternize(thisValue, separator, rightToLeft, allString, null, positions);
		}

		public static string Patternize(this string thisValue, char separator, bool rightToLeft, bool allString, string prefix, [NotNull] params int[] positions)
		{
			return Patternize(thisValue, separator, rightToLeft, allString, prefix, null, positions);
		}

		public static string Patternize(this string thisValue, char separator, bool rightToLeft, bool allString, string prefix, string suffix, [NotNull] params int[] positions)
		{
			/*
			 * turns a string into pattern.
			 * Examples:
			 * Patternize('-', 4) will turn "123456789" to "1234-5678-9"
			 * Patternize('-', true, 4) will turn "123456789" to "1-2345-6789"
			 * Patternize('-', 3, 4) will turn "123456789" to "123-4567-89" or "1234567890" to "123-4567-890"
			 * Patternize('/', false, false, 2, 2) will turn "123456789" to "12/34/56789"
			 * Patternize('-', false, false, "0x", 4) will turn "12F4C78E" to "0x12F4-C78E"
			 * Patternize(' ', false, false, 4) will turn "01101101" to "0110 1101"
			*/

			if (string.IsNullOrEmpty(thisValue) || positions.IsNullOrEmpty()) return thisValue;
			if (positions.All(item => item == 0)) throw new ArgumentException("Position must be none zero.", nameof(positions));

			StringBuilder sb = new StringBuilder(prefix);

			int x = 0, n = 0, t = 0;

			while (x < thisValue.Length)
			{
				while (n == 0)
				{
					if (t >= positions.Length)
					{
						if (!allString)
						{
							n = thisValue.Length - x;
							break;
						}

						t = 0;
					}

					n = Math.Abs(positions[t]);
					t++;
				}

				if (x + n > thisValue.Length) n = thisValue.Length - x;

				if (rightToLeft) sb.Insert(0, thisValue.Substring(thisValue.Length - (x + n), n));
				else sb.Append(thisValue.Substring(x, n));

				x += n;
				n = 0;

				if (x >= thisValue.Length) continue;

				if (rightToLeft)
				{
					if (sb[0] != separator) sb.Insert(0, separator);
				}
				else if (sb[sb.Length - 1] != separator) sb.Append(separator);
			}

			sb.Append(suffix);
			return sb.ToString();
		}

		[NotNull]
		// This is lame!
		// ReSharper disable once FunctionComplexityOverflow
		public static string Sprintf([NotNull] this string thisValue, params object[] parameters)
		{
			if (parameters.IsNullOrEmpty()) return thisValue;

			StringBuilder f = new StringBuilder();

			//"%[parameter][flags][width][.precision][length]type"
			int defaultParamIx = 0;

			// find all format parameters in format string
			f.Append(thisValue);
			Match m = RGX_PRINTF.Match(f.ToString());

			while (m.Success)
			{
				int paramIx = defaultParamIx;

				if (m.Groups[1] != null && m.Groups[1].Value.Length > 0)
				{
					string val = m.Groups[1].Value.Substring(0, m.Groups[1].Value.Length - 1);
					paramIx = Convert.ToInt32(val) - 1;
				}

				// extract format flags
				bool flagAlternate = false;
				bool flagLeft2Right = false;
				bool flagPositiveSign = false;
				bool flagPositiveSpace = false;
				bool flagZeroPadding = false;
				bool flagGroupThousands = false;

				if (m.Groups[2] != null && m.Groups[2].Value.Length > 0)
				{
					string flags = m.Groups[2].Value;

					flagAlternate = flags.IndexOf('#') >= 0;
					flagLeft2Right = flags.IndexOf('-') >= 0;
					flagPositiveSign = flags.IndexOf('+') >= 0;
					flagPositiveSpace = flags.IndexOf(' ') >= 0;
					flagGroupThousands = flags.IndexOf('\'') >= 0;

					// positive + indicator overrides a
					// positive space character
					if (flagPositiveSign && flagPositiveSpace) flagPositiveSpace = false;
				}

				// extract field length and 
				// padding character
				char paddingCharacter = ' ';
				int fieldLength = int.MinValue;

				if (m.Groups[3] != null && m.Groups[3].Value.Length > 0)
				{
					fieldLength = Convert.ToInt32(m.Groups[3].Value);
					flagZeroPadding = m.Groups[3].Value[0] == '0';
				}

				if (flagZeroPadding) paddingCharacter = '0';

				// left2right alignment overrides zero padding
				if (flagLeft2Right && flagZeroPadding) paddingCharacter = ' ';

				// extract field precision
				int fieldPrecision = int.MinValue;
				if (m.Groups[4] != null && m.Groups[4].Value.Length > 0) fieldPrecision = Convert.ToInt32(m.Groups[4].Value);

				// extract short / long indicator
				char shortLongIndicator = char.MinValue;
				if (m.Groups[5] != null && m.Groups[5].Value.Length > 0) shortLongIndicator = m.Groups[5].Value[0];

				// extract format
				char formatSpecifier = char.MinValue;
				if (m.Groups[6] != null && m.Groups[6].Value.Length > 0) formatSpecifier = m.Groups[6].Value[0];

				// default precision is 6 digits if none is specified except
				if (fieldPrecision == int.MinValue &&
					formatSpecifier != 's' &&
					formatSpecifier != 'c' &&
					char.ToUpper(formatSpecifier) != 'X' &&
					formatSpecifier != 'o')
				{
					fieldPrecision = 6;
				}

				// get next value parameter and convert value parameter depending on short / long indicator
				object o;

				if (parameters == null || paramIx >= parameters.Length) o = null;
				else
				{
					o = parameters[paramIx];

					switch (shortLongIndicator)
					{
						case 'h':
							switch (o)
							{
								case int i:
									o = (short)i;
									break;
								case long l:
									o = (short)l;
									break;
								case uint u:
									o = (ushort)u;
									break;
								case ulong ul:
									o = (ushort)ul;
									break;
							}
							break;
						case 'l':
							switch (o)
							{
								case short s:
									o = (long)s;
									break;
								case int i:
									o = (long)i;
									break;
								case ushort us:
									o = (ulong)us;
									break;
								case uint u:
									o = (ulong)u;
									break;
							}
							break;
					}
				}

				// convert value parameters to a string depending on the formatSpecifier
				string w = string.Empty;

				switch (formatSpecifier)
				{
					case '%': // % character
						w = "%";
						break;
					case 'd': // integer
						w = FormatNumber(flagGroupThousands ? "n" : "d",
							fieldLength, int.MinValue, flagLeft2Right,
							flagPositiveSign, flagPositiveSpace,
							paddingCharacter, o);
						defaultParamIx++;
						break;
					case 'i': // integer
						goto case 'd';
					case 'o': // octal integer - no leading zero
						w = FormatOct(flagAlternate,
							fieldLength, flagLeft2Right,
							paddingCharacter, o);
						defaultParamIx++;
						break;
					case 'x': // hex integer - no leading zero
						w = FormatHex("x", flagAlternate,
							fieldLength, fieldPrecision, flagLeft2Right,
							paddingCharacter, o);
						defaultParamIx++;
						break;
					case 'X': // same as x but with capital hex characters
						w = FormatHex("X", flagAlternate,
							fieldLength, fieldPrecision, flagLeft2Right,
							paddingCharacter, o);
						defaultParamIx++;
						break;

					case 'u': // unsigned integer
						IComparable comparable = o as IComparable;
						w = FormatNumber(flagGroupThousands ? "n" : "d",
							fieldLength, int.MinValue, flagLeft2Right,
							false, false,
							paddingCharacter, comparable.ToUnsigned());
						defaultParamIx++;
						break;
					case 'c': // character
						if (o.IsNumeric())
						{
							w = Convert.ToChar(o).ToString();
						}
						else
						{
							switch (o)
							{
								case char c:
									w = c.ToString();
									break;
								case string s when s.Length > 0:
									w = s[0].ToString();
									break;
							}
						}

						defaultParamIx++;
						break;
					case 's': // string
						w = o.ToString();
						if (fieldPrecision >= 0) w = w.Substring(0, fieldPrecision);
						if (fieldLength != int.MinValue) w = flagLeft2Right ? w.PadRight(fieldLength, paddingCharacter) : w.PadLeft(fieldLength, paddingCharacter);
						defaultParamIx++;
						break;
					case 'f': // double
						w = FormatNumber(flagGroupThousands ? "n" : "f",
							fieldLength, fieldPrecision, flagLeft2Right,
							flagPositiveSign, flagPositiveSpace,
							paddingCharacter, o);
						defaultParamIx++;
						break;
					case 'e': // double / exponent
						w = FormatNumber("e",
							fieldLength, fieldPrecision, flagLeft2Right,
							flagPositiveSign, flagPositiveSpace,
							paddingCharacter, o);
						defaultParamIx++;
						break;
					case 'E': // double / exponent
						w = FormatNumber("E",
							fieldLength, fieldPrecision, flagLeft2Right,
							flagPositiveSign, flagPositiveSpace,
							paddingCharacter, o);
						defaultParamIx++;
						break;
					case 'g': // double / exponent
						w = FormatNumber("g",
							fieldLength, fieldPrecision, flagLeft2Right,
							flagPositiveSign, flagPositiveSpace,
							paddingCharacter, o);
						defaultParamIx++;
						break;
					case 'G': // double / exponent
						w = FormatNumber("G",
							fieldLength, fieldPrecision, flagLeft2Right,
							flagPositiveSign, flagPositiveSpace,
							paddingCharacter, o);
						defaultParamIx++;
						break;
					case 'p': // pointer
						if (o is IntPtr ptr) w = "0x" + ptr.ToString("x");
						defaultParamIx++;
						break;
					case 'n': // number of characters so far
						w = FormatNumber("d",
							fieldLength, int.MinValue, flagLeft2Right,
							flagPositiveSign, flagPositiveSpace,
							paddingCharacter, m.Index);
						break;
					default:
						w = string.Empty;
						defaultParamIx++;
						break;
				}

				// replace format parameter with parameter value
				// and start searching for the next format parameter
				// AFTER the position of the current inserted value
				// to prohibit recursive matches if the value also
				// includes a format specifier
				f.Remove(m.Index, m.Length);
				f.Insert(m.Index, w);
				m = RGX_PRINTF.Match(f.ToString(), m.Index + w.Length);
			}

			return f.ToString();
		}

		public static int Count(this string thisValue, char value)
		{
			if (string.IsNullOrEmpty(thisValue)) return 0;

			int count = 0;

			foreach (char c in thisValue)
			{
				if (c != value) continue;
				count++;
			}

			return count;
		}

		public static int Count(this string thisValue, string value, StringComparison comparison = StringComparison.Ordinal)
		{
			if (string.IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(value) || value.Length > thisValue.Length) return 0;

			int index = 0, count = 0;

			do
			{
				index = thisValue.IndexOf(value, index, comparison);
				if (index < 0) break;
				count++;
				index += value.Length;
			}
			while (index.InRangeRx(0, thisValue.Length));

			return count;
		}

		public static void ForEach(this string thisValue, char delimiter, [NotNull] Action<string> action)
		{
			if (string.IsNullOrEmpty(thisValue)) return;

			using (CharEnumerator enumerator = Enumerate(thisValue, delimiter))
			{
				while (enumerator.MoveNext())
					action(enumerator.Current);
			}
		}

		public static void ForEach(this string thisValue, [NotNull] string delimiter, [NotNull] Action<string> action, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
		{
			if (string.IsNullOrEmpty(thisValue)) return;

			using (StringEnumerator enumerator = Enumerate(thisValue, delimiter, comparison))
			{
				while (enumerator.MoveNext())
					action(enumerator.Current);
			}
		}

		public static bool All(this string thisValue, char delimiter, [NotNull] Func<string, bool> predicate)
		{
			if (string.IsNullOrEmpty(thisValue)) return false;

			using (CharEnumerator enumerator = Enumerate(thisValue, delimiter))
			{
				return enumerator.All(predicate);
			}
		}

		public static bool All(this string thisValue, char delimiter, [NotNull] Func<string, int, bool> predicate)
		{
			if (string.IsNullOrEmpty(thisValue)) return false;

			using (CharEnumerator enumerator = Enumerate(thisValue, delimiter))
			{
				return enumerator.All(predicate);
			}
		}

		public static bool All(this string thisValue, [NotNull] string delimiter, [NotNull] Func<string, bool> predicate, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
		{
			if (string.IsNullOrEmpty(thisValue)) return false;

			using (StringEnumerator enumerator = Enumerate(thisValue, delimiter, comparison))
			{
				return enumerator.All(predicate);
			}
		}

		public static bool All(this string thisValue, [NotNull] string delimiter, [NotNull] Func<string, int, bool> predicate, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
		{
			if (string.IsNullOrEmpty(thisValue)) return false;

			using (StringEnumerator enumerator = Enumerate(thisValue, delimiter, comparison))
			{
				return enumerator.All(predicate);
			}
		}

		public static bool Any(this string thisValue, char delimiter, [NotNull] Func<string, bool> predicate)
		{
			if (string.IsNullOrEmpty(thisValue)) return false;

			using (CharEnumerator enumerator = Enumerate(thisValue, delimiter))
			{
				return enumerator.Any(predicate);
			}
		}

		public static bool Any(this string thisValue, [NotNull] string delimiter, [NotNull] Func<string, bool> predicate, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
		{
			if (string.IsNullOrEmpty(thisValue)) return false;

			using (StringEnumerator enumerator = Enumerate(thisValue, delimiter, comparison))
			{
				return enumerator.Any(predicate);
			}
		}

		[NotNull]
		public static CharEnumerator Enumerate([NotNull] this string thisValue, char delimiter) { return new CharEnumerator(thisValue, delimiter); }

		[NotNull]
		public static StringEnumerator Enumerate([NotNull] this string thisValue, [NotNull] string delimiter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
		{
			return new StringEnumerator(thisValue, delimiter, comparison);
		}

		[NotNull]
		public static CharLister ToList([NotNull] this string thisValue, char delimiter) { return new CharLister(thisValue, delimiter); }

		[NotNull]
		public static StringLister ToList([NotNull] this string thisValue, [NotNull] string delimiter, StringComparison comparison = StringComparison.Ordinal)
		{
			return new StringLister(thisValue, delimiter, comparison);
		}

		[NotNull]
		public static string JoinNotEmpty(this string thisValue, [NotNull] params string[] values)
		{
			if (values.Length == 0) return string.Empty;
			IEnumerable<string> enumerable = values.Where(e => !string.IsNullOrEmpty(e));
			StringBuilder sb = new StringBuilder();
			sb.Join(enumerable, thisValue);
			return sb.ToString();
		}

		[NotNull]
		public static SecureString Secure([NotNull] this string thisValue)
		{
			SecureString secureString = new SecureString();

			foreach (char c in thisValue)
				secureString.AppendChar(c);

			return secureString.ToReadOnly();
		}

		[NotNull]
		public static string SubstringUntil([NotNull] this string thisValue, string value, StringComparison comparison = StringComparison.Ordinal)
		{
			int index = string.IsNullOrEmpty(thisValue) ? -1 : thisValue.IndexOf(value, comparison);
			return index < 0 ? thisValue : thisValue.Substring(0, index);
		}

		[NotNull]
		public static string SubstringAfter([NotNull] this string thisValue, string value, StringComparison comparison = StringComparison.Ordinal)
		{
			int index = string.IsNullOrEmpty(thisValue) ? -1 : thisValue.IndexOf(value, comparison);
			return index < 0 ? string.Empty : thisValue.Substring(index + value.Length, thisValue.Length - index - value.Length);
		}

		[NotNull]
		public static Stream ToStream(this string thisValue, Encoding encoding = null)
		{
			MemoryStream stream = new MemoryStream();

			if (!string.IsNullOrEmpty(thisValue))
			{
				using (StreamWriter writer = new StreamWriter(stream, encoding ?? EncodingHelper.Default, Math.Min(thisValue.Length, StreamHelper.BUFFER_DEFAULT), true))
				{
					writer.Write(thisValue);
					writer.Flush();
					stream.Seek(0, SeekOrigin.Begin);
				}
			}

			return stream;
		}

		public static string ToPascalCase(this string thisValue)
		{
			if (thisValue == null) return null;
			StringBuilder sb = ToSeparatedCase(thisValue, ' ');
			return sb.Length < 2
						? sb.ToString().ToUpperInvariant()
						: CultureInfoHelper.English.TextInfo.ToTitleCase(sb.ToString()).Replace(" ", string.Empty);
		}

		public static string ToCamelCase(this string thisValue)
		{
			string value = ToPascalCase(thisValue);
			if (value == null) return null;
			if (value.Length < 2) return value.ToLowerInvariant();
			return char.ToLowerInvariant(value[0]) + value.Substring(1);
		}

		public static string ToStartCase(this string thisValue)
		{
			if (thisValue == null) return null;
			StringBuilder sb = ToSeparatedCase(thisValue, ' ');
			if (sb.Length < 2) return sb.ToString().ToUpperInvariant();
			return char.ToUpperInvariant(sb[0]) + sb.ToString(1, sb.Length);
		}

		public static string ToSnakeCase(this string thisValue) { return ToSeparatedCase(thisValue, '_')?.ToString(); }

		public static string ToDashCase(this string thisValue) { return ToSeparatedCase(thisValue, '-')?.ToString(); }

		public static string ToDotCase(this string thisValue) { return ToSeparatedCase(thisValue, '.')?.ToString(); }

		public static StringBuilder ToSeparatedCase(this string thisValue, char separator)
		{
			if (thisValue == null) return null;
			thisValue = thisValue.Trim();

			StringBuilder sb = new StringBuilder(thisValue.Length);

			if (thisValue.Length < 2)
			{
				sb.Append(thisValue.ToLowerInvariant());
				return sb;
			}

			foreach (char c in thisValue)
			{
				if (!char.IsLetterOrDigit(c))
				{
					if (sb.Length < 1 || sb[sb.Length - 1] == ' ') continue;
					sb.Append(' ');
					continue;
				}

				sb.Append(c);
			}

			thisValue = sb.ToString().ToLowerInvariant();
			sb.Length = 0;
			sb.Append(separator == ' '
						? thisValue
						: thisValue.Replace(' ', separator));
			return sb;
		}

		public static string ChangeCasing(this string thisValue, TextCasing casing)
		{
			thisValue = thisValue?.Trim();
			if (string.IsNullOrEmpty(thisValue)) return thisValue;
			
			switch (casing)
			{
				case TextCasing.Upper:
					return CultureInfoHelper.English.TextInfo.ToUpper(thisValue); 
				case TextCasing.Lower:
					return CultureInfoHelper.English.TextInfo.ToLower(thisValue); 
				case TextCasing.Pascal:
					return ToPascalCase(thisValue); 
				case TextCasing.Camel:
					return ToCamelCase(thisValue);
				case TextCasing.StartCase:
					return ToStartCase(thisValue);
				case TextCasing.Snake:
					return ToSnakeCase(thisValue); 
				case TextCasing.Dash:
					return ToDashCase(thisValue);
				case TextCasing.Dot:
					return ToDotCase(thisValue);
				default:
					return thisValue;
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string Coalesce(this string thisValue, params string[] values)
		{
			return thisValue != null || values.IsNullOrEmpty()
						? thisValue
						: values.FirstOrDefault(value => value != null);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string CoalesceNotEmpty(this string thisValue, params string[] values)
		{
			return !string.IsNullOrEmpty(thisValue) || values.IsNullOrEmpty()
						? thisValue
						: values.SkipNullOrEmpty().FirstOrDefault();
		}

		[NotNull]
		private static string FormatOct(bool alternate, int fieldLength, bool left2Right, char padding, [NotNull] object value)
		{
			string w = string.Empty;
			string lengthFormat = "{0" + (fieldLength != int.MinValue ? "," + (left2Right ? "-" : string.Empty) + fieldLength : string.Empty) + "}";

			if (!value.IsNumeric()) return w;

			IComparable comparable = value as IComparable;
			w = Convert.ToString(comparable.UnboxToLong(true), 8);

			if (left2Right || padding == ' ')
			{
				if (alternate && w != "0") w = "0" + w;
				w = string.Format(lengthFormat, w);
			}
			else
			{
				if (fieldLength != int.MinValue) w = w.PadLeft(fieldLength - (alternate && w != "0" ? 1 : 0), padding);
				if (alternate && w != "0") w = "0" + w;
			}

			return w;
		}

		[NotNull]
		private static string FormatHex(string nativeFormat, bool alternate, int fieldLength, int fieldPrecision, bool left2Right, char padding, [NotNull] object value)
		{
			string w = string.Empty;
			string lengthFormat = "{0" + (fieldLength != int.MinValue ? "," + (left2Right ? "-" : string.Empty) + fieldLength : string.Empty) + "}";
			string numberFormat = "{0:" + nativeFormat + (fieldPrecision != int.MinValue ? fieldPrecision.ToString() : string.Empty) + "}";

			if (!value.IsNumeric()) return w;
			w = string.Format(numberFormat, value);

			if (left2Right || padding == ' ')
			{
				if (alternate) w = (nativeFormat == "x" ? "0x" : "0X") + w;
				w = string.Format(lengthFormat, w);
			}
			else
			{
				if (fieldLength != int.MinValue) w = w.PadLeft(fieldLength - (alternate ? 2 : 0), padding);
				if (alternate) w = (nativeFormat == "x" ? "0x" : "0X") + w;
			}

			return w;
		}

		[NotNull]
		private static string FormatNumber(string nativeFormat, int fieldLength, int fieldPrecision, bool left2Right, bool positiveSign, bool positiveSpace,
			char padding, [NotNull] object value)
		{
			string w = string.Empty;
			string lengthFormat = "{0" + (fieldLength != int.MinValue ? "," + (left2Right ? "-" : string.Empty) + fieldLength : string.Empty) + "}";
			string numberFormat = "{0:" + nativeFormat + (fieldPrecision != int.MinValue ? fieldPrecision.ToString() : "0") + "}";

			if (!value.IsNumeric()) return w;
			w = string.Format(numberFormat, value);

			IComparable comparable = value as IComparable;
			
			if (left2Right || padding == ' ')
			{
				if (comparable.IsPositiveObj()) w = (positiveSign ? "+" : positiveSpace ? " " : string.Empty) + w;
				w = string.Format(lengthFormat, w);
			}
			else
			{
				if (w.StartsWith("-")) w = w.Substring(1);
				if (fieldLength != int.MinValue) w = w.PadLeft(fieldLength - 1, padding);
				w = comparable.IsPositiveObj() ? (positiveSign ? "+" : positiveSpace ? " " : fieldLength != int.MinValue ? padding.ToString() : string.Empty) + w : "-" + w;
			}

			return w;
		}

		[NotNull]
		private static string Replace([NotNull] string thisValue, string oldValue, string newValue, bool ignoreCase, bool ordinal, CompareInfo compareInfo, int startIndex, int count)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (string.IsNullOrEmpty(thisValue) || count == 0 || string.IsNullOrEmpty(oldValue) || oldValue.Length > thisValue.Length) return thisValue;
			newValue ??= string.Empty;

			CompareOptions options = CompareOptions.None;
			
			if (ignoreCase)
			{
				options |= ordinal
								? CompareOptions.OrdinalIgnoreCase
								: CompareOptions.IgnoreCase;
			}
			else if (ordinal)
			{
				options |= CompareOptions.Ordinal;
			}

			int x = compareInfo.IndexOf(thisValue, oldValue, startIndex, count, options);
			if (x < 0) return thisValue;

			int p = 0;
			StringBuilder sb = new StringBuilder(Math.Max(thisValue.Length, newValue.Length));

			while (x > -1)
			{
				sb.Append(thisValue, p, x - p);
				sb.Append(newValue);
				p = x + oldValue.Length;
				x = compareInfo.IndexOf(thisValue, oldValue, p, count, options);
			}

			if (p < thisValue.Length - 1) sb.Append(thisValue, p, thisValue.Length - p);
			return sb.ToString();
		}
	}
}