using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix.Extensions;

public static class StringBuilderExtension
{
	[NotNull]
	public static StringBuilder Append([NotNull] this StringBuilder thisValue, [NotNull] params char[] values)
	{
		if (values.Length == 0) return thisValue;
		thisValue.EnsureCapacity(thisValue.Length + values.Length);

		foreach (char c in values)
			thisValue.Append(c);

		return thisValue;
	}

	[NotNull]
	public static StringBuilder Append([NotNull] this StringBuilder thisValue, [NotNull] params string[] values)
	{
		int len = values.Where(s => s != null).Sum(s => s.Length);
		thisValue.EnsureCapacity(thisValue.Length + len);

		foreach (string value in values)
		{
			if (string.IsNullOrEmpty(value)) continue;
			thisValue.Append(value);
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder Append([NotNull] this StringBuilder thisValue, [NotNull] params object[] values)
	{
		foreach (object value in values)
		{
			if (value.IsNull()) continue;
			thisValue.Append(value);
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder Append<TKey, TValue>([NotNull] this StringBuilder thisValue, KeyValuePair<TKey, TValue> pair) { return thisValue.Append(pair.ToString('=')); }

	[NotNull]
	public static StringBuilder Append<TKey, TValue>([NotNull] this StringBuilder thisValue, KeyValuePair<TKey, TValue> pair, char group)
	{
		return thisValue.Separator(group).Append(pair.ToString('='));
	}

	[NotNull]
	public static StringBuilder Append<TKey, TValue>([NotNull] this StringBuilder thisValue, KeyValuePair<TKey, TValue> pair, char separator, char group)
	{
		return thisValue.Separator(group).Append(pair.ToString(separator));
	}

	[NotNull]
	public static StringBuilder Append<TKey, TValue>([NotNull] this StringBuilder thisValue, KeyValuePair<TKey, TValue> pair, [NotNull] string group)
	{
		return thisValue.Separator(group).Append(pair.ToString('='));
	}

	[NotNull]
	public static StringBuilder Append<TKey, TValue>([NotNull] this StringBuilder thisValue, KeyValuePair<TKey, TValue> pair, [NotNull] string separator, [NotNull] string group)
	{
		return thisValue.Separator(group).Append(pair.ToString(separator));
	}

	[NotNull]
	public static StringBuilder Append([NotNull] this StringBuilder thisValue, DictionaryEntry pair) { return thisValue.Append(pair.ToString('=')); }

	[NotNull]
	public static StringBuilder Append([NotNull] this StringBuilder thisValue, DictionaryEntry pair, char group)
	{
		return thisValue.Separator(group).Append(pair.ToString('='));
	}

	[NotNull]
	public static StringBuilder Append([NotNull] this StringBuilder thisValue, DictionaryEntry pair, char separator, char group)
	{
		return thisValue.Separator(group).Append(pair.ToString(separator));
	}

	[NotNull]
	public static StringBuilder Append([NotNull] this StringBuilder thisValue, DictionaryEntry pair, [NotNull] string group)
	{
		return thisValue.Separator(group).Append(pair.ToString('='));
	}

	[NotNull]
	public static StringBuilder Append([NotNull] this StringBuilder thisValue, DictionaryEntry pair, [NotNull] string separator, [NotNull] string group)
	{
		return thisValue.Separator(group).Append(pair.ToString(separator));
	}

	[NotNull]
	public static StringBuilder AppendIfDoesNotEndWith([NotNull] this StringBuilder thisValue, char value)
	{
		if (thisValue.Length > 0 && thisValue[thisValue.Length - 1] == value) return thisValue;
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendIfDoesNotEndWith([NotNull] this StringBuilder thisValue, string value)
	{
		if (string.IsNullOrEmpty(value)) return thisValue;
		return EndsWith(thisValue, value) 
					? thisValue 
					: thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendIfNotNullOrEmpty([NotNull] this StringBuilder thisValue, string value)
	{
		return string.IsNullOrEmpty(value)
					? thisValue
					: thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, bool value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, char value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, char value, int repeatCount)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value, repeatCount);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, sbyte value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, byte value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, short value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, ushort value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, int value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, uint value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, long value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, ulong value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, float value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, double value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, decimal value)
	{
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, string value)
	{
		if (string.IsNullOrEmpty(value)) return thisValue;
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, [NotNull] string value, int startIndex, int count)
	{
		value.Length.ValidateRange(startIndex, ref count);
		if (count == 0 || value.Length == 0) return thisValue;
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static unsafe StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, [NotNull] char* value, int count)
	{
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
		if (count == 0) return thisValue;
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value, count);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, [NotNull] char[] value)
	{
		if (value.Length == 0) return thisValue;
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, [NotNull] char[] value, int startIndex, int count)
	{
		value.Length.ValidateRange(startIndex, ref count);
		if (count == 0 || value.Length == 0) return thisValue;
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value, startIndex, count);
	}

	[NotNull]
	public static StringBuilder AppendWithLine([NotNull] this StringBuilder thisValue, object value)
	{
		if (value.IsNull()) return thisValue;
		if (thisValue.Length > 0) thisValue.AppendLine();
		return thisValue.Append(value);
	}

	[NotNull]
	public static StringBuilder AppendFormat<TKey, TValue>([NotNull] this StringBuilder thisValue, KeyValuePair<TKey, TValue> pair, [NotNull] string format)
	{
		return thisValue.Append(pair.Format(format));
	}

	[NotNull]
	public static StringBuilder AppendFormat<TKey, TValue>([NotNull] this StringBuilder thisValue, KeyValuePair<TKey, TValue> pair, [NotNull] string format, char group)
	{
		return thisValue.Separator(group).Append(pair.Format(format));
	}

	[NotNull]
	public static StringBuilder AppendFormat<TKey, TValue>([NotNull] this StringBuilder thisValue, KeyValuePair<TKey, TValue> pair, [NotNull] string format, char separator,
		char group)
	{
		return thisValue.Separator(group).Append(pair.Format(format, separator));
	}

	[NotNull]
	public static StringBuilder AppendFormat<TKey, TValue>([NotNull] this StringBuilder thisValue, KeyValuePair<TKey, TValue> pair, [NotNull] string format, [NotNull] string group)
	{
		return thisValue.Separator(group).Append(pair.Format(format));
	}

	[NotNull]
	public static StringBuilder AppendFormat<TKey, TValue>([NotNull] this StringBuilder thisValue, KeyValuePair<TKey, TValue> pair, [NotNull] string format, [NotNull] string separator,
		[NotNull] string group)
	{
		return thisValue.Separator(group).Append(pair.Format(format, separator));
	}

	[NotNull]
	public static StringBuilder AppendFormat([NotNull] this StringBuilder thisValue, DictionaryEntry pair, [NotNull] string format)
	{
		return thisValue.Append(pair.Format(format));
	}

	[NotNull]
	public static StringBuilder AppendFormat([NotNull] this StringBuilder thisValue, DictionaryEntry pair, [NotNull] string format, char group)
	{
		return thisValue.Separator(group).Append(pair.Format(format));
	}

	[NotNull]
	public static StringBuilder AppendFormat([NotNull] this StringBuilder thisValue, DictionaryEntry pair, [NotNull] string format, char separator, char group)
	{
		return thisValue.Separator(group).Append(pair.Format(format, separator));
	}

	[NotNull]
	public static StringBuilder AppendFormat([NotNull] this StringBuilder thisValue, DictionaryEntry pair, [NotNull] string format, [NotNull] string group)
	{
		return thisValue.Separator(group).Append(pair.Format(format));
	}

	[NotNull]
	public static StringBuilder AppendFormat([NotNull] this StringBuilder thisValue, DictionaryEntry pair, [NotNull] string format, [NotNull] string separator, [NotNull] string group)
	{
		return thisValue.Separator(group).Append(pair.Format(format, separator));
	}

	[NotNull]
	public static StringBuilder AppendLine([NotNull] this StringBuilder thisValue, [NotNull] string format, object arg0)
	{
		return thisValue.AppendLine(string.Format(format, arg0));
	}

	[NotNull]
	public static StringBuilder AppendLine([NotNull] this StringBuilder thisValue, [NotNull] string format, object arg0, object arg1)
	{
		return thisValue.AppendLine(string.Format(format, arg0, arg1));
	}

	[NotNull]
	public static StringBuilder AppendLine([NotNull] this StringBuilder thisValue, [NotNull] string format, object arg0, object arg1, object arg2)
	{
		return thisValue.AppendLine(string.Format(format, arg0, arg1, arg2));
	}

	[NotNull]
	public static StringBuilder AppendLine([NotNull] this StringBuilder thisValue, [NotNull] string format, [NotNull] params object[] args)
	{
		return thisValue.AppendLine(string.Format(format, args));
	}

	[NotNull]
	public static StringBuilder AppendLine([NotNull] this StringBuilder thisValue, IFormatProvider provider, [NotNull] string format, [NotNull] params object[] args)
	{
		return thisValue.AppendLine(string.Format(provider, format, args));
	}

	public static bool InBounds([NotNull] this StringBuilder thisValue, int index) { return !IsNullOrEmpty(thisValue) && index.InRangeRx(0, thisValue.Length); }

	public static int IndexOf([NotNull] this StringBuilder thisValue, char value, int startIndex = 0, int count = -1)
	{
		thisValue.Length.ValidateRange(startIndex, ref count);
		if (IsNullOrEmpty(thisValue) || count == 0) return -1;

		int lastPos = startIndex + count;

		for (int i = startIndex; i < lastPos; i++)
		{
			if (thisValue[i] != value) continue;
			return i;
		}

		return -1;
	}

	public static int IndexOf([NotNull] this StringBuilder thisValue, [NotNull] string strB, int startIndex = 0, int count = -1)
	{
		if (IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(strB) || strB.Length > thisValue.Length) return -1;
		thisValue.Length.ValidateRange(startIndex, ref count);
		if (count == 0 || thisValue.Length == 0) return -1;
		if (strB.Length == 1) return IndexOf(thisValue, strB[0], startIndex, count);

		int f = -1, l = 0;
		int lastPos = startIndex + count;

		for (; startIndex < lastPos && l < strB.Length; startIndex++)
		{
			if (thisValue[startIndex] != strB[l])
			{
				f = -1;
				l = 0;
				continue;
			}

			if (f == -1) f = startIndex;
			l++;
		}

		return f;
	}

	public static int LastIndexOf([NotNull] this StringBuilder thisValue, char value, int startIndex = 0, int count = -1)
	{
		thisValue.Length.ValidateRange(startIndex, ref count);
		if (IsNullOrEmpty(thisValue) || count == 0) return -1;

		int lastPos = startIndex + count;

		for (int i = lastPos - 1; i > -1; i--)
		{
			if (thisValue[i] != value) continue;
			return i;
		}

		return -1;
	}

	public static int LastIndexOf([NotNull] this StringBuilder thisValue, string strB, int startIndex = 0, int count = -1)
	{
		thisValue.Length.ValidateRange(startIndex, ref count);
		if (IsNullOrEmpty(thisValue) || count == 0) return -1;
		if (strB.Length == 1) return LastIndexOf(thisValue, strB[0], startIndex, count);

		int lastPos = startIndex + count - strB.Length;
		int x = -1;

		for (int i = lastPos - 1; i > -1; i--)
		{
			for (int n = 0; n < strB.Length; n++)
			{
				if (thisValue[i] != strB[n]) break;
				if (n == thisValue.Length - 1) x = i;
			}

			if (x < 0) continue;
			return x;
		}

		return x;
	}

	public static int IndexOfAny([NotNull] this StringBuilder thisValue, [NotNull] params char[] value) { return IndexOfAny(thisValue, 0, thisValue.Length, value); }

	public static int IndexOfAny([NotNull] this StringBuilder thisValue, int startIndex, [NotNull] params char[] value)
	{
		return IndexOfAny(thisValue, startIndex, thisValue.Length - startIndex, value);
	}

	public static int IndexOfAny([NotNull] this StringBuilder thisValue, int startIndex, int count, [NotNull] params char[] value)
	{
		thisValue.Length.ValidateRange(startIndex, ref count);
		if (IsNullOrEmpty(thisValue) || count == 0) return -1;

		while (startIndex < count)
		{
			char v = thisValue[startIndex];
			if (value.Any(c => v == c)) return startIndex;
			++startIndex;
		}

		return -1;
	}

	public static int LastIndexOfAny([NotNull] this StringBuilder thisValue, [NotNull] params char[] value)
	{
		return LastIndexOfAny(thisValue, thisValue.Length - 1, thisValue.Length, StringComparison.Ordinal);
	}

	public static int LastIndexOfAny([NotNull] this StringBuilder thisValue, int startIndex, [NotNull] params char[] value)
	{
		return LastIndexOfAny(thisValue, startIndex, startIndex + 1, StringComparison.Ordinal);
	}

	public static int LastIndexOfAny([NotNull] this StringBuilder thisValue, int startIndex, int count, [NotNull] params char[] value)
	{
		return LastIndexOfAny(thisValue, startIndex, startIndex + 1, StringComparison.Ordinal);
	}

	public static int LastIndexOfAny([NotNull] this StringBuilder thisValue, StringComparison comparison, [NotNull] params char[] value)
	{
		return LastIndexOfAny(thisValue, thisValue.Length - 1, thisValue.Length, comparison);
	}

	public static int LastIndexOfAny([NotNull] this StringBuilder thisValue, int startIndex, StringComparison comparison, [NotNull] params char[] value)
	{
		return LastIndexOfAny(thisValue, startIndex, startIndex + 1, comparison, value);
	}

	public static int LastIndexOfAny([NotNull] this StringBuilder thisValue, int startIndex, int count, StringComparison comparison, [NotNull] params char[] value)
	{
		thisValue.Length.ValidateRange(startIndex, ref count);
		if (IsNullOrEmpty(thisValue) || count == 0) return -1;

		int lastPos = startIndex - count + 1;

		while (startIndex > lastPos)
		{
			char v = thisValue[startIndex];
			if (value.Any(c => v == c)) return startIndex;
			--startIndex;
		}

		return -1;
	}

	public static bool Contains([NotNull] this StringBuilder thisValue, char value) { return IndexOf(thisValue, value) > -1; }

	public static bool Contains([NotNull] this StringBuilder thisValue, [NotNull] string value) { return IndexOf(thisValue, value) > -1; }

	public static bool Equals(this StringBuilder thisValue, string value)
	{
		if (thisValue == null && value == null) return true;
		if (thisValue == null || value == null) return false;
		if (thisValue.Length != value.Length) return false;
		if (value.Length == 1) return thisValue[0] == value[0];

		for (int i = 0; i < thisValue.Length; i++)
		{
			if (thisValue[i] != value[i]) return false;
		}

		return true;
	}

	public static bool Equals(this StringBuilder thisValue, string value, StringComparison comparison)
	{
		if (thisValue == null && value == null) return true;
		if (thisValue == null || value == null) return false;
		return thisValue.Length == value.Length && thisValue.ToString().Equals(value, comparison);
	}

	public static bool Equals(this StringBuilder thisValue, int indexA, string value, int indexB, int length)
	{
		if (thisValue == null && value == null) return true;
		if (thisValue == null || value == null) return false;

		int maxLen = Math.Min(thisValue.Length, value.Length);
		if (length == -1) length = maxLen;
		if (length < 0 || length > maxLen) throw new ArgumentOutOfRangeException(nameof(length));

		if (!indexA.InRangeRx(0, thisValue.Length)) throw new ArgumentOutOfRangeException(nameof(indexA));
		if (indexA + length > thisValue.Length) throw new ArgumentOutOfRangeException(nameof(length));

		if (!indexB.InRangeRx(0, value.Length)) throw new ArgumentOutOfRangeException(nameof(indexB));
		if (indexB + length > value.Length) throw new ArgumentOutOfRangeException(nameof(length));

		switch (length)
		{
			case 0:
				return false;
			case 1:
				return thisValue[indexA] != value[indexB];
			default:
				for (int i = 0; i < length; i++, indexA++, indexB++)
				{
					if (thisValue[indexA] != value[indexB]) return false;
				}
				return true;
		}
	}

	public static bool Equals(this StringBuilder thisValue, int indexA, string value, int indexB, int length, StringComparison comparison)
	{
		if (thisValue == null && value == null) return true;
		if (thisValue == null || value == null) return false;

		int maxLen = Math.Min(thisValue.Length, value.Length);
		if (length == -1) length = maxLen;
		if (length < 0 || length > maxLen) throw new ArgumentOutOfRangeException(nameof(length));

		if (!indexA.InRangeRx(0, thisValue.Length)) throw new ArgumentOutOfRangeException(nameof(indexA));
		if (indexA + length > thisValue.Length) throw new ArgumentOutOfRangeException(nameof(length));

		if (!indexB.InRangeRx(0, value.Length)) throw new ArgumentOutOfRangeException(nameof(indexB));
		if (indexB + length > value.Length) throw new ArgumentOutOfRangeException(nameof(length));

		if (length == 0) return false;

		string strA = thisValue.ToString(indexA, length);
		if (indexB != 0 || length != value.Length) value = value.Substring(indexB, length);
		return string.Equals(strA, value, comparison);
	}

	public static bool StartsWith(this StringBuilder thisValue, char value) { return !IsNullOrEmpty(thisValue) && thisValue[0] == value; }

	public static bool StartsWith(this StringBuilder thisValue, string value)
	{
		if (thisValue == null && value == null) return true;
		if (thisValue == null || value == null) return false;
		if (thisValue.Length == 0 && value.Length == 0) return true;
		if (thisValue.Length == 0 || value.Length == 0 || thisValue.Length < value.Length) return false;
		return value.Length == 1 
					? thisValue[0] == value[0] 
					: Equals(thisValue, 0, value, 0, value.Length);
	}
	public static bool StartsWith(this StringBuilder thisValue, string value, StringComparison comparison)
	{
		if (thisValue == null && value == null) return true;
		if (thisValue == null || value == null) return false;
		if (thisValue.Length == 0 && value.Length == 0) return true;
		if (thisValue.Length == 0 || value.Length == 0) return false;
		return thisValue.Length >= value.Length && Equals(thisValue, 0, value, 0, value.Length, comparison);
	}

	public static bool EndsWith(this StringBuilder thisValue, char value) { return !IsNullOrEmpty(thisValue) && thisValue[thisValue.Length - 1] == value; }

	public static bool EndsWith(this StringBuilder thisValue, string value)
	{
		if (thisValue == null && value == null) return true;
		if (thisValue == null || value == null) return false;
		if (thisValue.Length == 0 && value.Length == 0) return true;
		if (thisValue.Length == 0 || value.Length == 0 || thisValue.Length < value.Length) return false;
		return value.Length == 1
					? thisValue[thisValue.Length - 1] == value[0]
					: Equals(thisValue, thisValue.Length - value.Length, value, 0, value.Length);
	}
	public static bool EndsWith(this StringBuilder thisValue, string value, StringComparison comparison)
	{
		if (thisValue == null && value == null) return true;
		if (thisValue == null || value == null) return false;
		if (thisValue.Length == 0 && value.Length == 0) return true;
		if (thisValue.Length == 0 || value.Length == 0 || thisValue.Length < value.Length) return false;
		return Equals(thisValue, thisValue.Length - value.Length, value, 0, value.Length, comparison);
	}

	[NotNull]
	public static StringBuilder Substring([NotNull] this StringBuilder thisValue, int startIndex = 0, int length = -1)
	{
		if (!startIndex.InRangeRx(0, thisValue.Length)) throw new ArgumentOutOfRangeException(nameof(startIndex));
		if (length == -1) length = thisValue.Length - startIndex;
		if (length < 0 || startIndex > thisValue.Length - length) throw new ArgumentOutOfRangeException(nameof(length));
		return length == 0
					? new StringBuilder()
					: new StringBuilder(thisValue.ToString(startIndex, length));
	}

	public static bool IsDigits(this StringBuilder thisValue)
	{
		if (IsNullOrEmpty(thisValue)) return false;

		for (int i = 0; i < thisValue.Length; i++)
		{
			if (thisValue[i].IsDigit()) continue;
			return false;
		}

		return true;
	}

	public static bool IsLetterOrDigit(this StringBuilder thisValue)
	{
		if (IsNullOrEmpty(thisValue)) return false;

		for (int i = 0; i < thisValue.Length; i++)
		{
			if (thisValue[i].IsLetterOrDigit()) continue;
			return false;
		}

		return true;
	}

	public static bool IsLetters(this StringBuilder thisValue)
	{
		if (IsNullOrEmpty(thisValue)) return false;

		for (int i = 0; i < thisValue.Length; i++)
		{
			if (thisValue[i].IsLetter()) continue;
			return false;
		}

		return true;
	}

	public static bool IsLower(this StringBuilder thisValue)
	{
		if (IsNullOrEmpty(thisValue)) return false;

		for (int i = 0; i < thisValue.Length; i++)
		{
			if (thisValue[i].IsLower()) continue;
			return false;
		}

		return true;
	}

	public static bool IsNullOrEmpty(this StringBuilder thisValue) { return thisValue is not { Length: not 0 }; }

	public static string IfNullOrEmpty(this StringBuilder thisValue, string returnValue)
	{
		return IsNullOrEmpty(thisValue)
					? returnValue
					: thisValue.ToString();
	}

	public static bool IsNullOrWhiteSpace(this StringBuilder thisValue)
	{
		if (IsNullOrEmpty(thisValue)) return true;

		for (int i = 0; i < thisValue.Length; i++)
		{
			if (thisValue[i].IsWhiteSpace()) continue;
			return false;
		}

		return true;
	}

	public static string IfNullOrWhiteSpace([NotNull] this StringBuilder thisValue, string returnValue)
	{
		return IsNullOrWhiteSpace(thisValue)
					? returnValue
					: thisValue.ToString();
	}

	public static bool IsNumbers(this StringBuilder thisValue)
	{
		if (IsNullOrEmpty(thisValue)) return false;

		for (int i = 0; i < thisValue.Length; i++)
		{
			if (thisValue[i].IsNumber()) continue;
			return false;
		}

		return true;
	}

	public static bool IsUpper(this StringBuilder thisValue)
	{
		if (IsNullOrEmpty(thisValue)) return false;

		for (int i = 0; i < thisValue.Length; i++)
		{
			if (thisValue[i].IsUpper()) continue;
			return false;
		}

		return true;
	}

	[NotNull]
	public static StringBuilder Concat([NotNull] this StringBuilder thisValue, [NotNull] params object[] objects) { return Concat(thisValue, (IEnumerable)objects); }

	[NotNull]
	public static StringBuilder Concat([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable enumerable)
	{
		foreach (object value in enumerable)
		{
			switch (value)
			{
				case null:
					break;
				case string s:
					thisValue.Append(s);
					break;
				case IEnumerable e:
					Concat(thisValue, e);
					break;
				default:
					thisValue.Append(value);
					break;
			}
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder Concat<T>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<T> enumerable)
	{
		Type type = typeof(T);

		if (type == typeof(string))
		{
			foreach (T value in enumerable)
			{
				thisValue.Append(value);
			}
		}
		else if (type.Implements<IEnumerable>())
		{
			foreach (T value in enumerable)
			{
				Concat(thisValue, value);
			}
		}
		else
		{
			foreach (T value in enumerable)
			{
				thisValue.Append(value);
			}
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder Concat<TKey, TValue>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> enumerable, char group = '|')
	{
		foreach (KeyValuePair<TKey, TValue> pair in enumerable)
		{
			thisValue.Append(pair, group);
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder Concat<TKey, TValue>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> enumerable, string group)
	{
		if (string.IsNullOrEmpty(group)) return Concat(thisValue, enumerable);

		foreach (KeyValuePair<TKey, TValue> pair in enumerable)
		{
			thisValue.Append(pair, group);
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder ConcatFormat([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable enumerable, string format)
	{
		if (string.IsNullOrEmpty(format)) return Concat(thisValue, enumerable);

		foreach (object value in enumerable)
		{
			switch (value)
			{
				case null:
					break;
				case string s:
					thisValue.AppendFormat(format, s);
					break;
				case IEnumerable e:
					ConcatFormat(thisValue, e, format);
					break;
				default:
					thisValue.AppendFormat(format, value);
					break;
			}
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder ConcatFormat([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable enumerable, string format, char group)
	{
		if (string.IsNullOrEmpty(format)) return Join(thisValue, enumerable, group);

		foreach (object value in enumerable)
		{
			switch (value)
			{
				case null:
					break;
				case string s:
					thisValue.Separator(group).AppendFormat(format, s);
					break;
				case IEnumerable e:
					ConcatFormat(thisValue, e, format, group);
					break;
				default:
					thisValue.Separator(group).AppendFormat(format, value);
					break;
			}
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder ConcatFormat([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable enumerable, string format, string group)
	{
		if (string.IsNullOrEmpty(format)) return Join(thisValue, enumerable, group);

		foreach (object value in enumerable)
		{
			switch (value)
			{
				case null:
					break;
				case string s:
					thisValue.Separator(group).AppendFormat(format, s);
					break;
				case IEnumerable e:
					ConcatFormat(thisValue, e, format, group);
					break;
				default:
					thisValue.Separator(group).AppendFormat(format, value);
					break;
			}
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder ConcatFormat<T>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<T> enumerable, string format)
	{
		if (string.IsNullOrEmpty(format)) return Concat(thisValue, enumerable);

		foreach (T value in enumerable)
		{
			switch (value)
			{
				case string s:
					thisValue.AppendFormat(format, s);
					break;
				case IEnumerable<T> e:
					ConcatFormat(thisValue, e, format);
					break;
				case IEnumerable e:
					ConcatFormat(thisValue, e, format);
					break;
				default:
					thisValue.AppendFormat(format, value);
					break;
			}
		}
			
		return thisValue;
	}

	[NotNull]
	public static StringBuilder ConcatFormat<T>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<T> enumerable, string format, char group)
	{
		if (string.IsNullOrEmpty(format)) return Join(thisValue, enumerable, group);

		foreach (T value in enumerable)
		{
			switch (value)
			{
				case string s:
					thisValue.Separator(group).AppendFormat(format, s);
					break;
				case IEnumerable<T> e:
					ConcatFormat(thisValue, e, format, group);
					break;
				case IEnumerable e:
					ConcatFormat(thisValue, e, format, group);
					break;
				default:
					thisValue.Separator(group).AppendFormat(format, value);
					break;
			}
		}
			
		return thisValue;
	}

	[NotNull]
	public static StringBuilder ConcatFormat<T>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<T> enumerable, string format, string group)
	{
		if (string.IsNullOrEmpty(format)) return Join(thisValue, enumerable, group);

		foreach (T value in enumerable)
		{
			switch (value)
			{
				case string s:
					thisValue.Separator(group).AppendFormat(format, s);
					break;
				case IEnumerable<T> e:
					ConcatFormat(thisValue, e, format, group);
					break;
				case IEnumerable e:
					ConcatFormat(thisValue, e, format, group);
					break;
				default:
					thisValue.Separator(group).AppendFormat(format, value);
					break;
			}
		}
			
		return thisValue;
	}

	[NotNull]
	public static StringBuilder ConcatFormat<TKey, TValue>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> enumerable, string format, char group = '|')
	{
		if (string.IsNullOrEmpty(format)) return Concat(thisValue, enumerable, group);

		foreach (KeyValuePair<TKey, TValue> pair in enumerable)
		{
			thisValue.AppendFormat(pair, format, group);
		}
			
		return thisValue;
	}

	[NotNull]
	public static StringBuilder ConcatFormat<TKey, TValue>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> enumerable, string format, string group)
	{
		if (string.IsNullOrEmpty(format)) return Concat(thisValue, enumerable, group);

		foreach (KeyValuePair<TKey, TValue> pair in enumerable)
		{
			thisValue.AppendFormat(pair, format, group);
		}
			
		return thisValue;
	}

	[NotNull]
	public static StringBuilder Join([NotNull] this StringBuilder thisValue, char separator, [NotNull] params object[] objects) { return Join(thisValue, objects, separator); }

	[NotNull]
	public static StringBuilder Join([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable enumerable, char separator)
	{
		foreach (object value in enumerable)
		{
			switch (value)
			{
				case null:
					break;
				case string s:
					if (s.Length > 0) thisValue.Separator(separator).Append(s);
					break;
				case IEnumerable e:
					Join(thisValue, e, separator);
					break;
				default:
					thisValue.Separator(separator).Append(value);
					break;
			}
		}
			
		return thisValue;
	}

	[NotNull]
	public static StringBuilder Join([NotNull] this StringBuilder thisValue, string separator, [NotNull] params object[] objects)
	{
		return Join(thisValue, objects, separator);
	}

	[NotNull]
	public static StringBuilder Join([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable enumerable, string separator)
	{
		if (string.IsNullOrEmpty(separator)) return Concat(thisValue, enumerable);

		foreach (object value in enumerable)
		{
			switch (value)
			{
				case null:
					break;
				case string s:
					if (s.Length > 0) thisValue.Separator(separator).Append(s);
					break;
				case IEnumerable e:
					Join(thisValue, e, separator);
					break;
				default:
					thisValue.Separator(separator).Append(value);
					break;
			}
		}
			
		return thisValue;
	}

	[NotNull]
	public static StringBuilder Join<T>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<T> enumerable, char separator)
	{
		foreach (T value in enumerable)
		{
			switch (value)
			{
				case string s:
					thisValue.Separator(separator).Append(s);
					break;
				case IEnumerable<T> e:
					Join(thisValue, e, separator);
					break;
				case IEnumerable e:
					Join(thisValue, e, separator);
					break;
				default:
					thisValue.Separator(separator).Append(value);
					break;
			}
		}
			
		return thisValue;
	}

	[NotNull]
	public static StringBuilder Join<T>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<T> enumerable, string separator)
	{
		if (string.IsNullOrEmpty(separator)) return Concat(thisValue, enumerable);

		foreach (T value in enumerable)
		{
			switch (value)
			{
				case string s:
					thisValue.Separator(separator).Append(s);
					break;
				case IEnumerable<T> e:
					Join(thisValue, e, separator);
					break;
				case IEnumerable e:
					Join(thisValue, e, separator);
					break;
				default:
					thisValue.Separator(separator).Append(value);
					break;
			}
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder Join<TKey, TValue>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> enumerable, char separator, char group)
	{
		foreach (KeyValuePair<TKey, TValue> pair in enumerable)
		{
			thisValue.Append(pair, separator, group);
		}
			
		return thisValue;
	}

	[NotNull]
	public static StringBuilder Join<TKey, TValue>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> enumerable, string separator, string group)
	{
		if (string.IsNullOrEmpty(separator)) return Concat(thisValue, enumerable, group);

		foreach (KeyValuePair<TKey, TValue> pair in enumerable)
		{
			thisValue.Append(pair, separator, group);
		}
			
		return thisValue;
	}

	[NotNull]
	public static StringBuilder JoinFormat<TKey, TValue>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> enumerable, string format, char separator, char group)
	{
		if (string.IsNullOrEmpty(format)) return Join(thisValue, enumerable, separator, group);

		foreach (KeyValuePair<TKey, TValue> pair in enumerable)
		{
			thisValue.AppendFormat(pair, format, separator, group);
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder JoinFormat<TKey, TValue>([NotNull] this StringBuilder thisValue, [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> enumerable, string format, string separator, [NotNull] string group)
	{
		if (string.IsNullOrEmpty(format)) return Join(thisValue, enumerable, separator, group);
		if (string.IsNullOrEmpty(separator)) return ConcatFormat(thisValue, enumerable, format, group);

		foreach (KeyValuePair<TKey, TValue> pair in enumerable)
		{
			thisValue.AppendFormat(pair, format, separator, group);
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder Left([NotNull] this StringBuilder thisValue, int length = -1) { return thisValue.Substring(0, length); }

	[NotNull]
	public static StringBuilder Right([NotNull] this StringBuilder thisValue, int length = -1)
	{
		if (length == -1) length = thisValue.Length;
		if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
		return length == 0 
					? new StringBuilder() 
					: thisValue.Substring(thisValue.Length - length, length);
	}

	[NotNull]
	public static StringBuilder PadLeft([NotNull] this StringBuilder thisValue, int totalWidth) { return PadLeft(thisValue, totalWidth, ' '); }

	[NotNull]
	public static StringBuilder PadLeft([NotNull] this StringBuilder thisValue, int totalWidth, char paddingChar)
	{
		return totalWidth < thisValue.Length
					? thisValue
					: thisValue.Insert(0, new string(paddingChar, totalWidth - thisValue.Length));
	}

	[NotNull]
	public static StringBuilder PadRight([NotNull] this StringBuilder thisValue, int totalWidth) { return PadRight(thisValue, totalWidth, ' '); }

	[NotNull]
	public static StringBuilder PadRight([NotNull] this StringBuilder thisValue, int totalWidth, char paddingChar)
	{
		return totalWidth < thisValue.Length
					? thisValue
					: thisValue.Append(new string(paddingChar, totalWidth - thisValue.Length));
	}

	[NotNull]
	public static StringBuilder Remove([NotNull] this StringBuilder thisValue, char remove) { return Remove(thisValue, 0, thisValue.Length, remove); }

	[NotNull]
	public static StringBuilder Remove([NotNull] this StringBuilder thisValue, int startIndex, char remove)
	{
		return Remove(thisValue, startIndex, thisValue.Length - startIndex, remove);
	}

	[NotNull]
	public static StringBuilder Remove([NotNull] this StringBuilder thisValue, int startIndex, int count, char remove)
	{
		thisValue.Length.ValidateRange(startIndex, ref count);
		if (IsNullOrEmpty(thisValue) || count == 0) return thisValue;

		int lastPosition = startIndex + count;

		for (int i = lastPosition - 1; i >= startIndex; --i)
		{
			if (thisValue[i] != remove) continue;
			thisValue.Remove(i, 1);
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder Remove([NotNull] this StringBuilder thisValue, [NotNull] params char[] remove) { return Remove(thisValue, 0, thisValue.Length, remove); }

	[NotNull]
	public static StringBuilder Remove([NotNull] this StringBuilder thisValue, int startIndex, [NotNull] params char[] remove)
	{
		return Remove(thisValue, startIndex, thisValue.Length - startIndex, remove);
	}

	[NotNull]
	public static StringBuilder Remove([NotNull] this StringBuilder thisValue, int startIndex, int count, [NotNull] params char[] remove)
	{
		thisValue.Length.ValidateRange(startIndex, ref count);
		if (IsNullOrEmpty(thisValue) || remove.IsNullOrEmpty() || count == 0) return thisValue;

		int lastPosition = startIndex + count;

		for (int i = lastPosition - 1; i >= startIndex; --i)
		{
			char c = thisValue[i];
			if (!remove.Contains(c)) continue;
			thisValue.Remove(i, 1);
		}

		return thisValue;
	}

	[NotNull]
	public static StringBuilder Remove([NotNull] this StringBuilder thisValue, string remove, int startIndex = 0, int count = -1)
	{
		thisValue.Length.ValidateRange(startIndex, ref count);
		if (IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(remove) || count == 0) return thisValue;
		return thisValue.Replace(remove, string.Empty, startIndex, count);
	}

	public static StringBuilder Replace(this StringBuilder thisValue, char replace, [NotNull] params char[] anyOf)
	{
		if (IsNullOrEmpty(thisValue) || anyOf.Length == 0) return thisValue;

		foreach (char c in anyOf)
			thisValue.Replace(c, replace);

		return thisValue;
	}

	public static StringBuilder Replace(this StringBuilder thisValue, string replace, [NotNull] params string[] anyOf)
	{
		if (IsNullOrEmpty(thisValue) || anyOf.Length == 0) return thisValue;
		replace ??= string.Empty;

		foreach (string c in anyOf)
			thisValue.Replace(c, replace);

		return thisValue;
	}

	/// <summary>
	/// For regular expression, you can use the following to test this method:
	/// str => The capacity is adjusted as needed.
	/// pattern => \b(\w+)\s+\1\b
	/// replace => $1
	/// 
	/// This will replace the duplicated word 'adjusted' with only one
	/// The key to achieve this is by using the captured group $1 
	/// which is (\w+)
	/// </summary>
	/// <param name="thisValue"></param>
	/// <param name="pattern"></param>
	/// <param name="replace"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public static StringBuilder Replace(this StringBuilder thisValue, string pattern, string replace, RegexOptions options)
	{
		if (IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(pattern)) return thisValue;

		string value = Regex.Replace(thisValue.ToString(), pattern, replace ?? string.Empty, options);
		thisValue.Length = 0;
		thisValue.Append(value);
		return thisValue;
	}

	[NotNull]
	public static StringBuilder Replace([NotNull] this StringBuilder thisValue, string replace, [NotNull] Regex expression, int startIndex = 0, int count = -1)
	{
		thisValue.Length.ValidateRange(startIndex, ref count);
		if (IsNullOrEmpty(thisValue) || count == 0) return thisValue;

		string value = expression.Replace(thisValue.ToString(), replace ?? string.Empty, startIndex, count);
		thisValue.Length = 0;
		thisValue.Append(value);
		return thisValue;
	}

	public static StringBuilder Replace(this StringBuilder thisValue, string pattern, [NotNull] MatchEvaluator evaluator)
	{
		return Replace(thisValue, pattern, evaluator, RegexHelper.OPTIONS_I);
	}

	public static StringBuilder Replace(this StringBuilder thisValue, string pattern, [NotNull] MatchEvaluator evaluator, RegexOptions options)
	{
		if (IsNullOrEmpty(thisValue) || string.IsNullOrEmpty(pattern)) return thisValue;

		string value = Regex.Replace(thisValue.ToString(), pattern, evaluator, options);
		thisValue.Length = 0;
		thisValue.Append(value);
		return thisValue;
	}

	[NotNull]
	public static StringBuilder Replace([NotNull] this StringBuilder thisValue, [NotNull] Regex expression, [NotNull] MatchEvaluator evaluator, int startIndex = 0, int count = -1)
	{
		thisValue.Length.ValidateRange(startIndex, ref count);
		if (IsNullOrEmpty(thisValue) || count == 0) return thisValue;
		string value = expression.Replace(thisValue.ToString(), evaluator, startIndex, count);
		thisValue.Length = 0;
		thisValue.Append(value);
		return thisValue;
	}

	public static StringBuilder Reverse(this StringBuilder thisValue, [NotNull] params char[] separator)
	{
		if (IsNullOrEmpty(thisValue)) return thisValue;

		if (separator.IsNullOrEmpty())
		{
			int n = 0;
			string ch = thisValue.ToString();

			for (int i = ch.Length - 1; i >= 0; i--)
			{
				thisValue[n] = ch[i];
				n++;
			}

			return thisValue;
		}

		string pattern = string.Format(RegexHelper.RGX_PARTITIONS_P, Regex.Escape(separator.ToString(0)).Replace("-", @"\-"));
		List<string> parts = new List<string>();
		Match m = Regex.Match(thisValue.ToString(), pattern, RegexHelper.OPTIONS_I);

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

		thisValue.Length = 0;
		if (parts.Count == 0) return thisValue;

		thisValue.Append(parts[parts.Count - 1]);
		parts.RemoveAt(parts.Count - 1);
		parts.Reverse();

		foreach (string part in parts)
		{
			thisValue.Append(part[part.Length - 1]);
			if (part.Length > 1) thisValue.Append(part.Substring(0, part.Length - 1));
		}

		return thisValue;
	}

	public static StringBuilder Separator(this StringBuilder thisValue, char separator)
	{
		return IsNullOrEmpty(thisValue)
					? thisValue
					: thisValue.Append(separator);
	}

	public static StringBuilder Separator(this StringBuilder thisValue, [NotNull] string separator)
	{
		return IsNullOrEmpty(thisValue)
					? thisValue
					: thisValue.Append(separator);
	}

	[NotNull]
	public static StringBuilder Postfix([NotNull] this StringBuilder thisValue, char prefix)
	{
		return thisValue.Length == 0 || thisValue[0] == prefix
					? thisValue
					: thisValue.Insert(0, prefix);
	}

	public static StringBuilder Suffix(this StringBuilder thisValue, char suffix)
	{
		return IsNullOrEmpty(thisValue) || thisValue[0] == suffix
					? thisValue
					: thisValue.Append(suffix);
	}

	[NotNull]
	public static string[] Split(this StringBuilder thisValue, [NotNull] params char[] separator)
	{
		if (IsNullOrEmpty(thisValue)) return Array.Empty<string>();
		return separator.IsNullOrEmpty() ? new[] {thisValue.ToString()} : thisValue.ToString().Split(separator);
	}

	[NotNull]
	public static string[] Split(this StringBuilder thisValue, char[] separator, int count)
	{
		if (IsNullOrEmpty(thisValue)) return Array.Empty<string>();
		return separator.IsNullOrEmpty() ? new[] {thisValue.ToString()} : thisValue.ToString().Split(separator, count);
	}

	[NotNull]
	public static string[] Split(this StringBuilder thisValue, [NotNull] char[] separator, StringSplitOptions options)
	{
		if (IsNullOrEmpty(thisValue)) return Array.Empty<string>();
		return separator.IsNullOrEmpty() ? new[] {thisValue.ToString()} : thisValue.ToString().Split(separator, options);
	}

	[NotNull]
	public static string[] Split([NotNull] this StringBuilder thisValue, [NotNull] char[] separator, int count, StringSplitOptions options)
	{
		if (IsNullOrEmpty(thisValue)) return Array.Empty<string>();
		return separator.IsNullOrEmpty() ? new[] {thisValue.ToString()} : thisValue.ToString().Split(separator, count, options);
	}

	[NotNull]
	public static string[] Split([NotNull] this StringBuilder thisValue, [NotNull] string[] separator, StringSplitOptions options)
	{
		if (IsNullOrEmpty(thisValue)) return Array.Empty<string>();
		return separator.IsNullOrEmpty() ? new[] {thisValue.ToString()} : thisValue.ToString().Split(separator, options);
	}

	[NotNull]
	public static string[] Split([NotNull] this StringBuilder thisValue, [NotNull] string[] separator, int count, StringSplitOptions options)
	{
		if (IsNullOrEmpty(thisValue)) return Array.Empty<string>();
		return separator.IsNullOrEmpty() ? new[] {thisValue.ToString()} : thisValue.ToString().Split(separator, count, options);
	}

	[NotNull]
	public static string[] Split([NotNull] this StringBuilder thisValue, [NotNull] string pattern) { return Split(thisValue, pattern, RegexHelper.OPTIONS_I); }

	[NotNull]
	public static string[] Split([NotNull] this StringBuilder thisValue, [NotNull] string pattern, RegexOptions options)
	{
		if (IsNullOrEmpty(thisValue)) return Array.Empty<string>();

		string value = thisValue.ToString();
		return string.IsNullOrEmpty(pattern) ? new[] {value} : Regex.Split(value, pattern, options);
	}

	[NotNull]
	public static string[] SplitFind([NotNull] this StringBuilder thisValue, [NotNull] params char[] ch) { return SplitFind(thisValue, false, ch); }

	[NotNull]
	public static string[] SplitFind([NotNull] this StringBuilder thisValue, bool splitEach, [NotNull] params char[] ch)
	{
		if (IsNullOrEmpty(thisValue)) return Array.Empty<string>();

		int x = ch.IsNullOrEmpty() ? -1 : thisValue.IndexOfAny(ch);
		List<string> result = new List<string>();

		string str1 = null;
		string str2 = null;

		if (x < 0)
			str1 = thisValue.ToString();
		else if (x == 0)
		{
			if (thisValue.Length > 1) str2 = thisValue.Substring(1).ToString();
		}
		else if (x == thisValue.Length - 1)
			str1 = thisValue.Substring(0, x).ToString();
		else
		{
			str1 = thisValue.Substring(0, x).ToString();
			str2 = thisValue.Substring(++x).ToString();
		}

		result.Add(str1);

		if (!splitEach)
		{
			if (str2 != null) result.Add(str2);
		}
		else if (str2 != null)
		{
			string[] newSub = str2.Split(ch);

			if (newSub.Length > 0) result.AddRange(newSub);
			else result.Add(str2);
		}

		return result.ToArray();
	}

	[NotNull]
	public static char[] ToCharArray(this StringBuilder thisValue, int startIndex = 0, int length = -1)
	{
		if (IsNullOrEmpty(thisValue)) return Array.Empty<char>();
		if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
		if (length == -1) length = thisValue.Length - startIndex;
		if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
		if (startIndex + length > thisValue.Length) length = thisValue.Length - startIndex;
		if (length == 0) return Array.Empty<char>();

		char[] chArray = new char[length];
		if (length > 0) thisValue.CopyTo(startIndex, chArray, 0, length);
		return chArray;
	}

	public static StringBuilder ToLower(this StringBuilder thisValue)
	{
		if (IsNullOrEmpty(thisValue)) return thisValue;

		for (int i = 0; i < thisValue.Length; i++)
			thisValue[i] = thisValue[i].ToLower();

		return thisValue;
	}

	public static StringBuilder ToLower(this StringBuilder thisValue, CultureInfo culture)
	{
		if (IsNullOrEmpty(thisValue)) return thisValue;

		CultureInfo ci = culture ?? CultureInfoHelper.Default;

		for (int i = 0; i < thisValue.Length; i++)
			thisValue[i] = thisValue[i].ToLower(ci);

		return thisValue;
	}

	public static StringBuilder ToLowerInvariant(this StringBuilder thisValue)
	{
		if (IsNullOrEmpty(thisValue)) return thisValue;

		for (int i = 0; i < thisValue.Length; i++)
			thisValue[i] = thisValue[i].ToLowerInvariant();

		return thisValue;
	}

	public static StringBuilder ToUpper(this StringBuilder thisValue)
	{
		if (IsNullOrEmpty(thisValue)) return thisValue;

		for (int i = 0; i < thisValue.Length; i++)
			thisValue[i] = thisValue[i].ToUpper();

		return thisValue;
	}

	public static StringBuilder ToUpper(this StringBuilder thisValue, CultureInfo culture)
	{
		if (IsNullOrEmpty(thisValue)) return thisValue;

		CultureInfo ci = culture ?? CultureInfoHelper.Default;

		for (int i = 0; i < thisValue.Length; i++)
			thisValue[i] = thisValue[i].ToUpper(ci);

		return thisValue;
	}

	public static StringBuilder ToUpperInvariant(this StringBuilder thisValue)
	{
		if (IsNullOrEmpty(thisValue)) return thisValue;

		for (int i = 0; i < thisValue.Length; i++)
			thisValue[i] = thisValue[i].ToUpperInvariant();

		return thisValue;
	}

	public static StringBuilder Trim(this StringBuilder thisValue, params char[] trimChars)
	{
		if (IsNullOrEmpty(thisValue)) return thisValue;

		int cnts = 0, cnte = 0, start = 0, end = thisValue.Length - 1;

		if (trimChars.IsNullOrEmpty())
		{
			while (start < thisValue.Length && thisValue[start].IsWhiteSpace())
			{
				++cnts;
				++start;
			}

			while (end >= start && thisValue[end].IsWhiteSpace())
			{
				++cnte;
				--end;
			}
		}
		else
		{
			while (start < thisValue.Length)
			{
				int n = start;
				if (!trimChars.Any(c => c == thisValue[n])) break;
				++cnts;
				++start;
			}

			while (end >= start)
			{
				int n = end;
				if (!trimChars.Any(c => c == thisValue[n])) break;
				++cnte;
				--end;
			}
		}

		if (cnts > 0)
		{
			thisValue.Remove(0, cnts);
			end -= cnts;
		}

		if (cnte > 0) thisValue.Remove(end, cnte);
		return thisValue;
	}

	public static StringBuilder TrimEnd(this StringBuilder thisValue, params char[] trimChars)
	{
		if (IsNullOrEmpty(thisValue)) return thisValue;

		int cnt = 0, end = thisValue.Length - 1;

		if (trimChars.IsNullOrEmpty())
		{
			while (end > 0 && thisValue[end].IsWhiteSpace())
			{
				++cnt;
				--end;
			}
		}
		else
		{
			while (end > 0)
			{
				int n = end;
				if (!trimChars.Any(c => c == thisValue[n])) break;
				++cnt;
				--end;
			}
		}

		return cnt == 0 ? thisValue : thisValue.Remove(end, cnt);
	}

	public static StringBuilder TrimStart(this StringBuilder thisValue, params char[] trimChars)
	{
		if (IsNullOrEmpty(thisValue)) return thisValue;

		int cnt = 0, start = 0;

		if (trimChars.IsNullOrEmpty())
		{
			while (start < thisValue.Length && thisValue[start].IsWhiteSpace())
			{
				++cnt;
				++start;
			}
		}
		else
		{
			while (start < thisValue.Length)
			{
				int n = start;
				if (!trimChars.Any(c => c == thisValue[n])) break;
				++cnt;
				++start;
			}
		}

		return cnt == 0 ? thisValue : thisValue.Remove(0, cnt);
	}

	public static int Count(this StringBuilder thisValue, char value)
	{
		if (IsNullOrEmpty(thisValue)) return 0;

		int count = 0;

		for (int i = 0; i < thisValue.Length; i++)
		{
			if (thisValue[i] != value) continue;
			count++;
		}

		return count;
	}

	[NotNull]
	public static Stream ToStream(this StringBuilder thisValue, Encoding encoding = null)
	{
		string value = thisValue?.ToString();
		return value.ToStream(encoding);
	}
}