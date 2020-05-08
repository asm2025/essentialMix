using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using asm.Extensions;
using asm.Patterns.String;

namespace asm.Helpers
{
	public static class StringHelper
	{
		public static readonly char[] CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
		private static readonly Regex NUMERIC_STRING = new Regex(@"\A\s*\d*\.?\d+\s*\z", RegexHelper.OPTIONS_I);
		
		private static char[] InvalidKeyChar { get; }
		private static char[] SpecialChar { get; }
		private static char[] SafeSpecialChar { get; }

		static StringHelper()
		{
			List<char> chars = new List<char>(Path.GetInvalidFileNameChars());
			chars.AddRange(new[] { '.', ' ', '%', '&', '=', '#', '$', '-'});
			InvalidKeyChar = chars.Distinct().ToArray();

			StringBuilder sb = new StringBuilder();

			for (int i = char.MinValue; i <= char.MaxValue; i++)
			{
				char c = Convert.ToChar(i);

				if (char.IsControl(c) || char.IsLetterOrDigit(c)) continue;
				sb.Append(c);
			}

			SpecialChar = sb.ToString().ToCharArray();

			HashSet<char> invalid = new HashSet<char>(Path.GetInvalidFileNameChars()) { ' ' };
			SafeSpecialChar = SpecialChar.Where(c => !invalid.Contains(c)).ToArray();
		}

		public static string ToKey(string value) { return WebUtility.UrlEncode(value.Replace('_', InvalidKeyChar)); }

		public static string Random(int count, RandomStringType type = RandomStringType.Any)
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (count == 0) return string.Empty;

			List<Func<char>> types = new List<Func<char>>();
			if (type.HasFlag(RandomStringType.SmallLetters)) types.Add(RandomSmallLetter);
			if (type.HasFlag(RandomStringType.CapitalLetters)) types.Add(RandomCapitalLetter);
			if (type.HasFlag(RandomStringType.Numbers)) types.Add(RandomNumber);

			if (type.HasFlag(RandomStringType.SpecialCharacters))
			{
				if (type.HasFlag(RandomStringType.SafeCharacters))
					types.Add(RandomSafeSpecialCharacter);
				else
					types.Add(RandomSpecialCharacter);
			}

			if (types.Count == 0) return null;

			int n = types.Count - 1;
			StringBuilder sb = new StringBuilder(count);

			for (int i = 0; i < count; i++)
			{
				sb.Append(types[RandomHelper.Default.Next(0, n)].Invoke());
			}

			return sb.ToString();
		}

		[NotNull]
		public static string RandomKey(byte size)
		{
			if (size == 0) size = (byte)RandomHelper.Next(1, byte.MaxValue);

			byte[] data = new byte[size];
			
			using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
				crypto.GetNonZeroBytes(data);

			StringBuilder result = new StringBuilder(size);

			foreach (byte b in data)
				result.Append(CHARS[b % CHARS.Length]);

			return result.ToString();
		}

		public static char RandomSmallLetter() { return (char)RandomHelper.Default.Next('a', 'z'); }

		public static char RandomCapitalLetter() { return (char)RandomHelper.Default.Next('A', 'Z'); }

		public static char RandomNumber() { return (char)RandomHelper.Default.Next('0', '9'); }

		public static char RandomSpecialCharacter() { return SpecialChar[RandomHelper.Default.Next(SpecialChar.Length - 1)]; }

		public static char RandomSafeSpecialCharacter() { return SafeSpecialChar[RandomHelper.Default.Next(SpecialChar.Length - 1)]; }

		public static string[][] SplitGroups(string value) { return SplitGroups(value, ','); }

		public static string[][] SplitGroups(string value, int count) { return SplitGroups(value, ',', count); }

		public static string[][] SplitGroups(string value, char group) { return SplitGroups(value, group, '='); }

		public static string[][] SplitGroups(string value, char group, int count) { return SplitGroups(value, group, '=', count); }

		public static string[][] SplitGroups(string value, char group, char separator) { return SplitGroups(value, group, separator, -1); }

		public static string[][] SplitGroups(string value, char group, char separator, int count)
		{
			if (count < -1) throw new ArgumentOutOfRangeException(nameof(count));
			if (string.IsNullOrEmpty(value)) return null;

			string[] groups = count < 1 ? value.Split(group) : value.Split(count, group);
			IList<string[]> values = new List<string[]>();

			foreach (string s in groups)
			{
				if (string.IsNullOrEmpty(s)) continue;
				values.Add(s.Split(2, separator));
			}

			return values.ToArray();
		}

		public static KeyValuePair<TKey, TValue>[] SplitKeys<TKey, TValue>(string value) { return SplitKeys<TKey, TValue>(value, ','); }

		public static KeyValuePair<TKey, TValue>[] SplitKeys<TKey, TValue>(string value, int count) { return SplitKeys<TKey, TValue>(value, ',', count); }

		public static KeyValuePair<TKey, TValue>[] SplitKeys<TKey, TValue>(string value, char group) { return SplitKeys<TKey, TValue>(value, group, '='); }

		public static KeyValuePair<TKey, TValue>[] SplitKeys<TKey, TValue>(string value, char group, int count) { return SplitKeys<TKey, TValue>(value, group, '=', count); }

		public static KeyValuePair<TKey, TValue>[] SplitKeys<TKey, TValue>(string value, char group, char separator)
		{
			return SplitKeys<TKey, TValue>(value, group, separator, -1);
		}

		public static KeyValuePair<TKey, TValue>[] SplitKeys<TKey, TValue>(string value, char group, char separator, int count)
		{
			return SplitKeys<TKey, TValue>(value, group, separator, count, default);
		}

		public static KeyValuePair<TKey, TValue>[] SplitKeys<TKey, TValue>(string value, TValue defaultValue) { return SplitKeys<TKey, TValue>(value, ',', defaultValue); }

		public static KeyValuePair<TKey, TValue>[] SplitKeys<TKey, TValue>(string value, int count, TValue defaultValue)
		{
			return SplitKeys<TKey, TValue>(value, ',', count, defaultValue);
		}

		public static KeyValuePair<TKey, TValue>[] SplitKeys<TKey, TValue>(string value, char group, TValue defaultValue)
		{
			return SplitKeys<TKey, TValue>(value, group, '=', defaultValue);
		}

		public static KeyValuePair<TKey, TValue>[] SplitKeys<TKey, TValue>(string value, char group, int count, TValue defaultValue)
		{
			return SplitKeys<TKey, TValue>(value, group, '=', count, defaultValue);
		}

		public static KeyValuePair<TKey, TValue>[] SplitKeys<TKey, TValue>(string value, char group, char separator, TValue defaultValue)
		{
			return SplitKeys<TKey, TValue>(value, group, separator, -1, defaultValue);
		}

		public static KeyValuePair<TKey, TValue>[] SplitKeys<TKey, TValue>(string value, char group, char separator, int count, TValue defaultValue)
		{
			if (count < -1) throw new ArgumentOutOfRangeException(nameof(count));
			if (string.IsNullOrEmpty(value)) return null;

			string[] groups = count < 1 ? value.Split(group) : value.Split(count, group);
			IList<KeyValuePair<TKey, TValue>> values = new List<KeyValuePair<TKey, TValue>>();

			foreach (string s in groups)
			{
				if (string.IsNullOrEmpty(s)) continue;
				string[] key = s.Split(2, separator);
				values.Add(new KeyValuePair<TKey, TValue>(key[0].To(default(TKey)),
					key.Length > 1 ? key[1].To(defaultValue) : defaultValue));
			}

			return values.ToArray();
		}
		
		public static bool IsNumeric(string value) { return !string.IsNullOrEmpty(value) && NUMERIC_STRING.IsMatch(value); }

		public static int ToBase64Size(string value, Encoding encoding = null) { return ByteHelper.ToBase64Size(value.ByteSize(encoding)); }

		public static int FromBase64Size(string value, Encoding encoding = null) { return ByteHelper.FromBase64Size(value.ByteSize(encoding)); }

		public static string FirstNotNullOrEmptyOrDefault([NotNull] params string[] values) { return values.FirstNotNullOrEmptyOrDefault(); }
		public static string LastNotNullOrEmptyOrDefault([NotNull] params string[] values) { return values.LastNotNullOrEmptyOrDefault(); }

		public static string FirstNotNullOrEmpty([NotNull] params string[] values) { return values.FirstNotNullOrEmpty(); }
		public static string LastNotNullOrEmpty([NotNull] params string[] values) { return values.LastNotNullOrEmpty(); }

		public static string SingleNotNullOrEmptyOrDefault([NotNull] params string[] values) { return values.SingleNotNullOrEmptyOrDefault(); }

		public static string FirstNotNullOrWhiteSpaceOrDefault([NotNull] params string[] values) { return values.FirstNotNullOrWhiteSpaceOrDefault(); }
		public static string LastNotNullOrWhiteSpaceOrDefault([NotNull] params string[] values) { return values.LastNotNullOrWhiteSpaceOrDefault(); }

		public static string FirstNotNullOrWhiteSpace([NotNull] params string[] values) { return values.FirstNotNullOrWhiteSpace(); }
		public static string LastNotNullOrWhiteSpace([NotNull] params string[] values) { return values.LastNotNullOrWhiteSpace(); }

		public static string SingleNotNullOrWhiteSpaceOrDefault([NotNull] params string[] values) { return values.SingleNotNullOrWhiteSpaceOrDefault(); }
		
		public static IEnumerable<string> SkipNull([NotNull] params string[] values) { return values.SkipNull(); }
		[NotNull] public static IEnumerable<string> SkipNullOrEmpty([NotNull] params string[] values) { return values.SkipNullOrEmpty(); }
		[NotNull] public static IEnumerable<string> SkipNullOrEmptyTrim([NotNull] params string[] values) { return values.SkipNullOrEmptyTrim(); }
		[NotNull] public static IEnumerable<string> SkipNullOrWhitespace([NotNull] params string[] values) { return values.SkipNullOrWhitespace(); }
	}
}