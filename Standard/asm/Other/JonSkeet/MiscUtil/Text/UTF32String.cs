using System;
using System.Collections;
using System.Linq;
using asm.Extensions;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Other.JonSkeet.MiscUtil.Text
{
	/// <summary>
	/// String of UTF-32 characters (ints). This class is immutable, and so is thread-safe
	/// after copying, but copies must be originally made in a thread-safe manner so as
	/// to avoid seeing invalid data.
	/// </summary>
	public sealed class UTF32String : IEnumerable, IComparable, ICloneable
	{
		private const int HIGH_SURROGATE_START = 0xd800;
		private const int HIGH_SURROGATE_END = 0xdbff;
		private const int LOW_SURROGATE_START = 0xdc00;
		private const int LOW_SURROGATE_END = 0xdfff;
		private const int MAX_UTF32_CHARACTER = 0x10ffff;

		/// <summary>
		/// Number of samples to take (at most) to form a hash code.
		/// </summary>
		private const int HASHCODE_SAMPLE_SIZE = 20;

		/// <summary>
		/// An empty UTF-32 string.
		/// </summary>
		public static readonly UTF32String EMPTY = new UTF32String(new int[0]);

		/// <summary>
		/// UTF-32 characters making up the string.
		/// </summary>
		private readonly int[] _characters;

		/// <summary>
		/// Creates a UTF-32 string from an array of integers, all of which must
		/// be less than 0x10ffff and non-negative. A copy of the array is taken so that
		/// further changes to the array afterwards are ignored.
		/// </summary>
		/// <param name="characters">The array of characters to copy. Must not be null.</param>
		public UTF32String(int[] characters)
			: this(characters, true)
		{
		}

		private UTF32String(int[] characters, bool validate)
		{
			if (validate)
			{
				characters = (int[])characters.Clone();

				foreach (int character in characters)
					if (!IsValidUtf32Char(character))
						throw new ArgumentException($"Invalid character in array: {character}", nameof(characters));
			}

			_characters = characters;
		}

		/// <summary>
		/// Creates a UTF-32 string from a global::System.String (UTF-16), converting surrogates
		/// where they are present.
		/// </summary>
		/// <param name="utf16">The string in UTF-16 format.</param>
		public UTF32String([NotNull] string utf16)
		{
			if (utf16 == null) throw new ArgumentNullException(nameof(utf16));
			// Assume no surrogates to start with
			_characters = new int[utf16.Length];
			int highSurrogate = -1;
			int outputIndex = 0;

			foreach (char c in utf16)
			{
				if (c >= HIGH_SURROGATE_START && c <= HIGH_SURROGATE_END)
				{
					if (highSurrogate != -1) throw new ArgumentException("Invalid string: two high surrogates in a row", nameof(utf16));
					highSurrogate = (c - HIGH_SURROGATE_START) * 0x400;
				}
				else if (c >= LOW_SURROGATE_START && c <= LOW_SURROGATE_END)
				{
					if (highSurrogate == -1) throw new ArgumentException("Invalid string: low surrogate not preceded by high surrogate");
					_characters[outputIndex++] = highSurrogate + (c - LOW_SURROGATE_START) + 0x10000;
					highSurrogate = -1;
				}
				else
				{
					if (highSurrogate != -1) throw new ArgumentException("Invalid string: high surrogates with no following low surrogate", nameof(utf16));
					_characters[outputIndex++] = c;
				}
			}

			if (highSurrogate != -1) throw new ArgumentException("Invalid string: final character is a high surrogate");
			// Trim array if necessary
			if (outputIndex != _characters.Length)
			{
				int[] tmp = new int[outputIndex];
				Array.Copy(_characters, 0, tmp, 0, outputIndex);
				_characters = tmp;
			}
		}

		/// <summary>
		/// Returns a concatenation of the given strings.
		/// </summary>
		/// <param name="strA">The first string</param>
		/// <param name="strB">The second string</param>
		/// <returns>A string consisting of the first string followed by the second</returns>
		public static UTF32String operator +(UTF32String strA, UTF32String strB) { return Concat(strA, strB); }

		/// <summary>
		/// Determines whether two specified String objects have the same value.
		/// </summary>
		/// <param name="strA">A string or a null reference</param>
		/// <param name="strB">A string or a null reference</param>
		/// <returns>true if the value of strA is the same as the value of strB; otherwise, false</returns>
		public static bool operator ==(UTF32String strA, UTF32String strB) { return Equals(strA, strB); }

		/// <summary>
		/// Determines whether two specified String objects have different values.
		/// </summary>
		/// <param name="strA">A string or a null reference</param>
		/// <param name="strB">A string or a null reference</param>
		/// <returns>true if the value of strA is different from the value of strB; otherwise, false</returns>
		public static bool operator !=(UTF32String strA, UTF32String strB) { return !Equals(strA, strB); }

		/// <summary>
		/// Converts the UTF-32 string into a UTF-16 string,
		/// creating surrogates if necessary.
		/// </summary>
		/// <returns>
		/// A UTF-16 string (global::System.String) representing the same
		/// character data as this UTF-32 string.
		/// </returns>
		[NotNull]
		public override string ToString()
		{
			int surrogates = _characters.Count(character => character > 0xffff);
			// Count surrogates to start with
			char[] utf16 = new char[Length + surrogates];
			int outputIndex = 0;

			foreach (int character in _characters)
			{
				if (character < 0x10000)
				{
					utf16[outputIndex++] = (char)character;
				}
				else
				{
					utf16[outputIndex++] = (char)((character - 0x10000) / 0x400 + HIGH_SURROGATE_START);
					utf16[outputIndex++] = (char)((character - 0x10000) % 0x400 + LOW_SURROGATE_START);
				}
			}

			return new string(utf16);
		}

		/// <summary>
		/// Returns whether or not this UTF-32 string is equal to another object.
		/// </summary>
		/// <param name="obj">The object to compare this UTF-32 string to.</param>
		/// <returns>Whether or not this object is equal to the other one.</returns>
		public override bool Equals(object obj)
		{
			UTF32String other = obj as UTF32String;
			return other != null && Equals(other);
		}

		/// <summary>
		/// Returns a hashcode formed from sampling some of the characters in this
		/// UTF-32 string. This gives a good balance between performance and hash
		/// collisions.
		/// </summary>
		/// <returns>A hashcode for this UTF-32 string.</returns>
		public override int GetHashCode()
		{
			int hash = 0;
			int step = Math.Max(Length / HASHCODE_SAMPLE_SIZE, 1);
			
			for (int i = 0; i < Length; i += step)
			{
				hash ^= _characters[i];
			}
			
			return hash;
		}

		/// <summary>
		/// The character at the specified index.
		/// </summary>
		public int this[int index] => _characters[index];

		/// <summary>
		/// The number of UTF-32 characters in this string.
		/// </summary>
		public int Length => _characters.Length;

		/// <summary>
		/// Takes a substring of this string, starting at the given index.
		/// </summary>
		/// <param name="start">Starting index of desired substring in this string</param>
		/// <returns>A substring of this string</returns>
		public UTF32String Substring(int start)
		{
			if (start < 0)
				throw new ArgumentOutOfRangeException(nameof(start), "start must be non-negative");
			if (start > Length)
				throw new ArgumentOutOfRangeException(nameof(start), "start must be less than or equal to the length of the string");
			return start == Length
						? EMPTY
						: Substring(start, Length - start);
		}

		/// <summary>
		/// Takes a substring of this string, starting at the given index
		/// and containing the given number of characters.
		/// </summary>
		/// <param name="start">Starting index of desired substring in this string</param>
		/// <param name="count">The number of characters in the desired substring</param>
		/// <returns>A substring of this string</returns>
		public UTF32String Substring(int start, int count)
		{
			_characters.Length.ValidateRange(start, ref count);
			if (count == 0 || _characters.Length == 0) return EMPTY;

			int[] tmp = new int[count];
			Array.Copy(_characters, start, tmp, 0, count);
			return new UTF32String(tmp);
		}

		/// <summary>
		/// Finds the index of another UTF32String within this one.
		/// </summary>
		/// <param name="value">Value to find</param>
		/// <returns>The index of value within this string, or -1 if it isn't found</returns>
		public int IndexOf(UTF32String value) { return IndexOf(value, 0, Length); }

		/// <summary>
		/// Finds the index of another UTF32String within this one,
		/// starting at the specified position.
		/// </summary>
		/// <param name="value">Value to find</param>
		/// <param name="start">First position to consider when finding value within this UTF32String</param>
		/// <returns>The index of value within this string, or -1 if it isn't found</returns>
		public int IndexOf(UTF32String value, int start)
		{
			if (start < 0 || start > Length) throw new ArgumentOutOfRangeException(nameof(start), "start must lie within the string bounds");
			return IndexOf(value, start, Length - start);
		}

		/// <summary>
		/// Finds the index of another UTF32String within this one,
		/// starting at the specified position and considering the
		/// specified number of positions.
		/// </summary>
		/// <param name="value">Value to find</param>
		/// <param name="start">First position to consider when finding value within this UTF32String</param>
		/// <param name="count">Number of positions to consider</param>
		/// <returns>The index of value within this string, or -1 if it isn't found</returns>
		public int IndexOf(UTF32String value, int start, int count)
		{
			_characters.Length.ValidateRange(start, ref count);
			if (_characters.Length == 0 || count == 0) return -1;

			for (int i = start; i < start + count; i++)
			{
				if (i + value.Length > Length) return -1;
				int j;

				for (j = 0; j < value.Length; j++)
				{
					if (_characters[i + j] != value._characters[j]) break;
				}

				if (j == value.Length) return i;
			}
			return -1;
		}

		/// <summary>
		/// Finds the first index of the specified character within this string.
		/// </summary>
		/// <param name="character">Character to find</param>
		/// <returns>
		/// The index of the first occurrence of the specified character, or -1
		/// if it is not found.
		/// </returns>
		public int IndexOf(int character) { return IndexOf(character, 0, Length); }

		/// <summary>
		/// Finds the first index of the specified character within this string, starting
		/// at the specified position.
		/// </summary>
		/// <param name="character">Character to find</param>
		/// <param name="start">First position to consider</param>
		/// <returns>
		/// The index of the first occurrence of the specified character, or -1
		/// if it is not found.
		/// </returns>
		public int IndexOf(int character, int start)
		{
			if (start < 0 || start > Length)
				throw new ArgumentOutOfRangeException(nameof(start), "start must lie within the string bounds");
			return IndexOf(character, start, Length - start);
		}

		/// <summary>
		/// Finds the first index of the specified character within this string, starting
		/// at the specified position and considering the specified number of positions.
		/// </summary>
		/// <param name="character">Character to find</param>
		/// <param name="start">First position to consider</param>
		/// <param name="count">Number of positions to consider</param>
		/// <returns>
		/// The index of the first occurrence of the specified character, or -1
		/// if it is not found.
		/// </returns>
		public int IndexOf(int character, int start, int count)
		{
			_characters.Length.ValidateRange(start, ref count);
			if (_characters.Length == 0 || count == 0) return -1;

			for (int i = start; i < start + count; i++)
			{
				if (_characters[i] == character)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Compares two UTF-32 strings (in a culture-insensitive manner) for equality.
		/// </summary>
		/// <param name="other">The other string to compare this one to.</param>
		/// <returns>Whether or not this string is equal to the other one.</returns>
		public bool Equals(UTF32String other) { return ReferenceEquals(this, other) || CompareTo(other) == 0; }

		/// <summary>
		/// Copies the UTF-32 characters in this string to an int array.
		/// </summary>
		/// <returns>An array of integers representing the characters in this array.</returns>
		[NotNull]
		public int[] ToInt32Array() { return (int[])_characters.Clone(); }

		/// <summary>
		/// Enumerates the characters in the string.
		/// </summary>
		/// <returns>The enumerator for </returns>
		public IEnumerator GetEnumerator() { return _characters.GetEnumerator(); }

		/// <summary>
		/// Compares this string to another UTF32String.
		/// </summary>
		/// <param name="obj">The other UTF32String to compare this string to.</param>
		/// <returns>
		/// &lt;0 if this string &lt;> obj; 0 if this==object; &gt;0 if this string &gt; obj,
		/// with the relation defines in a culture-insensitive way in lexicographic order.
		/// </returns>
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;
			UTF32String other = obj as UTF32String;
			if (other == null)
				throw new ArgumentException("Can only compare Utf32Strings", nameof(obj));

			int minLength = Math.Min(Length, other.Length);
			
			for (int i = 0; i < minLength; i++)
			{
				int result = this[i] - other[i];
				if (result != 0)
					return result;
			}
			// Both strings are the same for all the characters in the shorter
			// one. The longer one is now greater, or they're the same.
			return Length - other.Length;
		}

		/// <summary>
		/// Creates a shallow copy of this string.
		/// </summary>
		/// <returns>A shallow copy of this string.</returns>
		public object Clone() { return MemberwiseClone(); }

		/// <summary>
		/// Returns whether or not an integer value is a valid
		/// UTF-32 character, that is, whether it is non-negative
		/// and less than or equal to 0x10FFFF.
		/// </summary>
		/// <param name="value">The value to test.</param>
		/// <returns>Whether or not the given value is a valid UTF-32 character.</returns>
		public static bool IsValidUtf32Char(int value) { return value >= 0 && value <= MAX_UTF32_CHARACTER; }

		/// <summary>
		/// Compares one string with another for equality.
		/// </summary>
		/// <param name="strA">The first string to compare</param>
		/// <param name="strB">The second string to compare</param>
		/// <returns>true if the strings are equivalent; false otherwise</returns>
		public static bool Equals(UTF32String strA, UTF32String strB) { return Compare(strA, strB) == 0; }

		/// <summary>
		/// Compares the two specified strings.
		/// </summary>
		/// <param name="strA">The first string to compare</param>
		/// <param name="strB">The second string to compare</param>
		/// <returns>
		/// 0 if both strings are null or they are equal; a negative number if strA == null or
		/// is lexicographically before strB; a positive number otherwise
		/// </returns>
		public static int Compare(UTF32String strA, UTF32String strB)
		{
			return ReferenceEquals(strA, strB)
						? 0
						: (object)strA == null || (object)strB == null
							? (object)strA == null
								? -1
								: 1
							: strA.CompareTo(strB);
		}

		/// <summary>
		/// Concatenates an array of strings together.
		/// </summary>
		/// <param name="strings">The array of strings to concatenate.</param>
		/// <returns></returns>
		public static UTF32String Concat([NotNull] params UTF32String[] strings)
		{
			if (strings == null)
				throw new ArgumentNullException(nameof(strings));
			int size = strings.Where(s => s != null).Sum(s => s.Length);
			if (size == 0)
				return EMPTY;

			int[] tmp = new int[size];
			int index = 0;

			foreach (UTF32String s in strings)
			{
				if (s == null) continue;
				Array.Copy(s._characters, 0, tmp, index, s.Length);
				index += s.Length;
			}
			return new UTF32String(tmp);
		}

		/// <summary>
		/// Returns a concatenation of the given strings.
		/// </summary>
		/// <param name="strA">The first string</param>
		/// <param name="strB">The second string</param>
		/// <returns>A string consisting of the first string followed by the second</returns>
		public static UTF32String Concat(UTF32String strA, UTF32String strB)
		{
			return Concat(new[]
						{
							strA,
							strB
						});
		}

		/// <summary>
		/// Returns a concatenation of the given strings.
		/// </summary>
		/// <param name="strA">The first string</param>
		/// <param name="strB">The second string</param>
		/// <param name="strC">The third string</param>
		/// <returns>
		/// A string consisting of the first string
		/// followed by the second, followed by the third
		/// </returns>
		public static UTF32String Concat(UTF32String strA, UTF32String strB, UTF32String strC)
		{
			return Concat(new[]
						{
							strA,
							strB,
							strC
						});
		}

		/// <summary>
		/// Returns a concatenation of the given strings.
		/// </summary>
		/// <param name="strA">The first string</param>
		/// <param name="strB">The second string</param>
		/// <param name="strC">The third string</param>
		/// <param name="strD">The fourth string</param>
		/// <returns>
		/// A string consisting of the first string
		/// followed by the second, followed by the third,
		/// followed by the fourth
		/// </returns>
		public static UTF32String Concat(UTF32String strA, UTF32String strB, UTF32String strC, UTF32String strD)
		{
			return Concat(new[]
						{
							strA,
							strB,
							strC,
							strD
						});
		}
	}
}