// Copyright (c) 2016 Tyler Brinkley
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Globalization;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Other.TylerBrinkley.Enumeration.Numeric
{
    internal class Int32NumericProvider : INumericProvider<int>
    {
        [NotNull]
        public string ToHexadecimalString(int value) { return value.ToString("X8"); }

		[NotNull]
		public string ToDecimalString(int value) { return value.ToString(); }

		public int One => 1;

		public int Zero => 0;

		public int And(int left, int right) { return left & right; }

		public int BitCount(int value) { return Number.BitCount(value); }

		public int Create(ulong value) { return (int)value; }

		public int Create(long value) { return (int)value; }

		public bool IsInValueRange(ulong value) { return value <= int.MaxValue; }

		public bool IsInValueRange(long value) { return value is >= int.MinValue and <= int.MaxValue; }

		public int LeftShift(int value, int amount) { return value << amount; }

		public bool LessThan(int left, int right) { return left < right; }

		public int Not(int value) { return ~value; }

		public int Or(int left, int right) { return left | right; }

		public int Subtract(int left, int right) { return left - right; }

		public bool TryParseNumber(string s, NumberStyles style, IFormatProvider provider, out int result) { return int.TryParse(s, style, provider, out result); }

		public bool TryParseNative(string s, out int result) { return int.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result); }

		public int Xor(int left, int right) { return left ^ right; }
	}
}
