using System;
using System.Globalization;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Other.TylerBrinkley.Enumeration.Numeric;

internal class CharNumericProvider : INumericProvider<char>
{
	public char One => (char)1;

	public char Zero => (char)0;

	public char And(char left, char right) { return (char)(left & right); }

	public int BitCount(char value) { return Number.BitCount(value); }

	public char Create(ulong value) { return (char)value; }

	public char Create(long value) { return (char)value; }

	public bool IsInValueRange(ulong value) { return value <= char.MaxValue; }

	public bool IsInValueRange(long value) { return value is >= 0L and <= char.MaxValue; }

	public char LeftShift(char value, int amount) { return (char)(value << amount); }

	public bool LessThan(char left, char right) { return left < right; }

	public char Not(char value) { return (char)~value; }

	public char Or(char left, char right) { return (char)(left | right); }

	public char Subtract(char left, char right) { return (char)(left - right); }

	[NotNull]
	public string ToHexadecimalString(char value) { return ((ushort)value).ToString("X4"); }

	public string ToDecimalString(char value) { return ((ushort)value).ToString(); }

	public bool TryParseNumber(string s, NumberStyles style, IFormatProvider provider, out char result)
	{
		bool success = ushort.TryParse(s, style, provider, out ushort resultAsUShort);
		result = (char)resultAsUShort;
		return success;
	}

	public bool TryParseNative(string s, out char result) { return char.TryParse(s, out result); }

	public char Xor(char left, char right) { return (char)(left ^ right); }
}