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

namespace asm.Numeric
{
    internal class UInt64NumericProvider : INumericProvider<ulong>
    {
        [NotNull]
        public string ToHexadecimalString(ulong value) { return value.ToString("X16"); }

		public string ToDecimalString(ulong value) { return value.ToString(); }

		public ulong One => 1UL;

		public ulong Zero => 0UL;

		public ulong And(ulong left, ulong right) { return left & right; }

		public int BitCount(ulong value) { return Number.BitCount((long)value); }

		public ulong Create(ulong value) { return value; }

		public ulong Create(long value) { return (ulong)value; }

		public bool IsInValueRange(ulong value) { return true; }

		public bool IsInValueRange(long value) { return value >= 0L; }

		public ulong LeftShift(ulong value, int amount) { return value << amount; }

		public bool LessThan(ulong left, ulong right) { return left < right; }

		public ulong Not(ulong value) { return ~value; }

		public ulong Or(ulong left, ulong right) { return left | right; }

		public ulong Subtract(ulong left, ulong right) { return left - right; }

		public bool TryParseNumber(string s, NumberStyles style, IFormatProvider provider, out ulong result) { return ulong.TryParse(s, style, provider, out result); }

		public bool TryParseNative(string s, out ulong result) { return ulong.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result); }

		public ulong Xor(ulong left, ulong right) { return left ^ right; }
	}
}
