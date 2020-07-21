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
		[NotNull]
		public static IEnumerable<string> SkipNullOrEmpty([NotNull] params string[] values) { return values.SkipNullOrEmpty(); }
		[NotNull]
		public static IEnumerable<string> SkipNullOrEmptyTrim([NotNull] params string[] values) { return values.SkipNullOrEmptyTrim(); }
		[NotNull]
		public static IEnumerable<string> SkipNullOrWhitespace([NotNull] params string[] values) { return values.SkipNullOrWhitespace(); }

		/// <summary>
		/// Groups <see href="https://en.wikipedia.org/wiki/Anagram">Anagrams</see> together.
		/// </summary>
		/// <param name="words">the strings representing possible anagrams</param>
		/// <returns>Array of Anagrams grouped together.</returns>
		public static IReadOnlyCollection<IReadOnlyList<string>> GroupAnagrams(IEnumerable<string> words)
		{
			// AlgoExpert - Become An Expert In Algorithms
			/*
			 * Write a function that takes in an array of strings and returns a list of groups of anagrams.
			 * Anagrams are strings made up of exactly the same letters, where order doesn't matter. For example,
			 * "cinema" and "iceman" are anagrams; similarly, "foo" and "ofo" are anagrams. Note that the groups of
			 * anagrams don't need to be ordered in any particular way.
			 */
			if (words == null) return null;

			Dictionary<string, List<string>> result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

			foreach (string word in words)
			{
				if (string.IsNullOrWhiteSpace(word)) continue;
				
				string sorted = string.Concat(word.OrderBy(e => e));

				if (!result.TryGetValue(sorted, out List<string> list))
				{
					list = new List<string>();
					result.Add(sorted, list);
				}

				list.Add(word);
			}

			return result.Values;
		}

		/// <summary>
		/// Finds <see href="https://en.wikipedia.org/wiki/Levenshtein_distance">Levenshtein distance</see>, the minimum number of edit
		/// operations that need to be performed on the first string to obtain the second string.
		///
		/// <para>
		/// the objective is to find matches for short strings in many longer texts, in situations where a small number of differences
		/// is to be expected. for instance, spell checkers, correction systems for optical character recognition, and software to assist
		/// natural language translation based on translation memory.
		/// </para>
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		public static int LevenshteinDistance([NotNull] string first, [NotNull] string second)
		{
			// https://www.eximiaco.tech/en/2019/11/17/computing-the-levenshtein-edit-distance-of-two-strings-using-c/
			/*
			 * Write a function that takes in two strings and returns the minimum number of
			 * edit operations that need to be performed on the first string to obtain the
			 * second string. There are three edit operations: insertion of a character,
			 * deletion of a character, and substitution of a character for another.
			 */
			if (first.Length == 0) return second.Length;
			if (second.Length == 0) return first.Length;

			int current = 1;
			int[,] rows = new int[2, second.Length + 1];

			for (int i = 0; i <= second.Length; i++) 
				rows[0, i] = i;

			int previous = 0;

			for (int i = 0; i < first.Length; i++)
			{
				rows[current, 0] = i + 1;

				for (int j = 1; j <= second.Length; j++)
				{
					int cost = second[j - 1] == first[i]
									? 0
									: 1;
					rows[current, j] = Math.Min(Math.Min(rows[previous, j] + 1, rows[current, j - 1] + 1), rows[previous, j - 1] + cost);
				}

				previous = (previous + 1) % 2;
				current = (current + 1) % 2;
			}

			return rows[previous, second.Length];
		}
	}
}