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
    internal class Int64NumericProvider : INumericProvider<long>
    {
        [NotNull]
        public string ToHexadecimalString(long value) { return value.ToString("X16"); }

		public string ToDecimalString(long value) { return value.ToString(); }

		public long One => 1L;

		public long Zero => 0L;

		public long And(long left, long right) { return left & right; }

		public int BitCount(long value) { return Number.BitCount(value); }

		public long Create(ulong value) { return (long)value; }

		public long Create(long value) { return value; }

		public bool IsInValueRange(ulong value) { return value <= long.MaxValue; }

		public bool IsInValueRange(long value) { return true; }

		public long LeftShift(long value, int amount) { return value << amount; }

		public bool LessThan(long left, long right) { return left < right; }

		public long Not(long value) { return ~value; }

		public long Or(long left, long right) { return left | right; }

		public long Subtract(long left, long right) { return left - right; }

		public bool TryParseNumber(string s, NumberStyles style, IFormatProvider provider, out long result) { return long.TryParse(s, style, provider, out result); }

		public bool TryParseNative(string s, out long result) { return long.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result); }

		public long Xor(long left, long right) { return left ^ right; }
	}
}
