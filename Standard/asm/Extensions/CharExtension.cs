using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm.Extensions
{
	public static class CharExtension
	{
		private static readonly ISet<UnicodeCategory> NON_PRINTABLE_UNICODE = new HashSet<UnicodeCategory>
		{
			UnicodeCategory.Control,
			UnicodeCategory.Format,
			UnicodeCategory.LineSeparator,
			UnicodeCategory.ParagraphSeparator
		};

		public static char Abs(this char thisValue) { return char.ToUpper(thisValue); }

		public static char Abs(this char thisValue, CultureInfo culture) { return char.ToUpper(thisValue, culture ?? CultureInfoHelper.Default); }

		public static int AbsInvariant(this char thisValue) { return char.ToUpperInvariant(thisValue); }

		public static int CompareOrdinal(this char thisValue, char other) { return thisValue.CompareTo(other); }

		public static int CompareOrdinalIgnoreCase(this char thisValue, char other)
		{
			if (!IsAscii(thisValue) || !IsAscii(other)) return ToUpperInvariant(thisValue) - ToUpperInvariant(other);
			
			int vc = thisValue, vb = other;
			if (vc - 97 <= 25U) vc -= 32;
			if (vb - 97 <= 25U) vb -= 32;
			return vc - vb;
		}

		public static bool IsEqual(this char thisValue, char other) { return CompareOrdinalIgnoreCase(thisValue, other) == 0; }

		public static bool In(this char thisValue, IEnumerable<char> enumerable, IEqualityComparer<char> comparer = null, int startIndex = 0, int count = -1)
		{
			return enumerable?.IndexOf(thisValue, comparer, startIndex, count) > -1;
		}

		public static bool IsAscii(this char thisValue) { return thisValue <= 127; }

		public static bool IsControl(this char thisValue) { return char.IsControl(thisValue); }

		public static bool IsDigit(this char thisValue) { return char.IsDigit(thisValue); }

		public static bool IsHighSurrogate(this char thisValue) { return char.IsHighSurrogate(thisValue); }

		public static bool IsLetter(this char thisValue) { return char.IsLetter(thisValue); }

		public static bool IsLetterOrDigit(this char thisValue) { return char.IsLetterOrDigit(thisValue); }

		public static bool IsLowSurrogate(this char thisValue) { return char.IsLowSurrogate(thisValue); }

		public static bool IsNumber(this char thisValue) { return char.IsNumber(thisValue); }

		public static bool IsPunctuation(this char thisValue) { return char.IsPunctuation(thisValue); }

		public static bool IsPrint(this char thisValue)
		{
			if (char.IsControl(thisValue)) return false;

			UnicodeCategory category = char.GetUnicodeCategory(thisValue);
			return category == UnicodeCategory.OtherNotAssigned || !NON_PRINTABLE_UNICODE.Contains(category);
		}

		public static bool IsSeparator(this char thisValue) { return char.IsSeparator(thisValue); }

		public static bool IsSurrogate(this char thisValue) { return char.IsSurrogate(thisValue); }

		public static bool IsSurrogatePair(this char thisValue, char other) { return char.IsSurrogatePair(thisValue, other); }

		public static bool IsSymbol(this char thisValue) { return char.IsSymbol(thisValue); }

		public static bool IsLower(this char thisValue) { return char.IsLower(thisValue); }

		public static bool IsUpper(this char thisValue) { return char.IsUpper(thisValue); }

		public static bool IsUtf(this char thisValue) { return thisValue > 127; }

		public static bool IsWhiteSpace(this char thisValue) { return char.IsWhiteSpace(thisValue); }

		public static bool IsBinary(this char thisValue) { return thisValue.InRange('0', '1'); }

		public static bool IsOctal(this char thisValue) { return thisValue.InRange('0', '8'); }

		public static bool IsHexadecimal(this char thisValue) { return thisValue.InRange('0', '9') || thisValue.InRange('A', 'Z') || thisValue.InRange('a', 'z'); }

		public static char ToLower(this char thisValue) { return char.ToLower(thisValue); }

		public static char ToLower(this char thisValue, CultureInfo culture) { return char.ToLower(thisValue, culture ?? CultureInfoHelper.Default); }

		public static char ToLowerInvariant(this char thisValue) { return char.ToLowerInvariant(thisValue); }

		public static int GetByteCount([NotNull] this char[] thisValue, [NotNull] Encoding encoding)
		{
			return thisValue.IsNullOrEmpty()
						? 0
						: encoding.GetByteCount(thisValue);
		}

		public static byte[] ToBytes([NotNull] this char[] thisValue, [NotNull] Encoding encoding)
		{
			return thisValue.IsNullOrEmpty()
						? null
						: encoding.GetBytes(thisValue);
		}

		public static string ToString(this char thisValue, int count)
		{
			return count < 1
						? null
						: new string(thisValue, count);
		}

		public static string ToString([NotNull] this IList<char> thisValue, string prefix) { return ToString(thisValue, prefix, null); }

		public static string ToString([NotNull] this IList<char> thisValue, string prefix, string suffix) { return ToString(thisValue, -1, -1, prefix, suffix); }

		public static string ToString([NotNull] this IList<char> thisValue, int startIndex) { return ToString(thisValue, startIndex, -1); }

		public static string ToString([NotNull] this IList<char> thisValue, int startIndex, int count) { return ToString(thisValue, startIndex, count, null); }

		public static string ToString([NotNull] this IList<char> thisValue, int startIndex, int count, string prefix)
		{
			return ToString(thisValue, startIndex, count, prefix, null);
		}

		public static string ToString([NotNull] this IList<char> thisValue, int startIndex, int count, string prefix, string suffix)
		{
			if (thisValue.IsNullOrEmpty()) return null;

			bool hasPrefix = !string.IsNullOrEmpty(prefix);
			bool hasSuffix = !string.IsNullOrEmpty(suffix);

			if (thisValue.Count == 0) return null;
			int x = startIndex.Within(0, thisValue.Count - 1);
			int n = count.Within(0, thisValue.Count);
			if (n < 1) n = thisValue.Count;
			if (x + n > thisValue.Count) n = thisValue.Count - x;
			int m = n;
			if (hasPrefix) m += prefix.Length;
			if (hasSuffix) m += suffix.Length;
			
			StringBuilder sblist = new StringBuilder(m);
			if (hasPrefix) sblist.Append(prefix);

			for (int i = x; i < n; i++) sblist.Append(thisValue[i]);

			if (hasSuffix) sblist.Append(suffix);
			return sblist.ToString();
		}

		public static char ToUpper(this char thisValue) { return char.ToUpper(thisValue); }

		public static char ToUpper(this char thisValue, CultureInfo culture) { return char.ToUpper(thisValue, culture ?? CultureInfoHelper.Default); }

		public static char ToUpperInvariant(this char thisValue) { return char.ToUpperInvariant(thisValue); }

		public static int ToUtf32(this char thisValue, char lowSurrogate) { return char.ConvertToUtf32(thisValue, lowSurrogate); }

		[NotNull] public static string ToBase64(this char thisValue) { return Convert.ToBase64String(BitConverter.GetBytes(thisValue)); }
	}
}