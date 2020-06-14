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

namespace asm.Other.TylerBrinkley.Enumeration.Numeric
{
    internal class UInt32NumericProvider : INumericProvider<uint>
    {
        public string ToHexadecimalString(uint value) { return value.ToString("X8"); }

		public string ToDecimalString(uint value) { return value.ToString(); }

		public uint One => 1U;

		public uint Zero => 0U;

		public uint And(uint left, uint right) { return left & right; }

		public int BitCount(uint value) { return Number.BitCount((int)value); }

		public uint Create(ulong value) { return (uint)value; }

		public uint Create(long value) { return (uint)value; }

		public bool IsInValueRange(ulong value) { return value <= uint.MaxValue; }

		public bool IsInValueRange(long value) { return value >= uint.MinValue && value <= uint.MaxValue; }

		public uint LeftShift(uint value, int amount) { return value << amount; }

		public bool LessThan(uint left, uint right) { return left < right; }

		public uint Not(uint value) { return ~value; }

		public uint Or(uint left, uint right) { return left | right; }

		public uint Subtract(uint left, uint right) { return left - right; }

		public bool TryParseNumber(string s, NumberStyles style, IFormatProvider provider, out uint result) { return uint.TryParse(s, style, provider, out result); }

		public bool TryParseNative(string s, out uint result) { return uint.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result); }

		public uint Xor(uint left, uint right) { return left ^ right; }
	}
}
