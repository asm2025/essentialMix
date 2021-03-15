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
    internal class UInt16NumericProvider : INumericProvider<ushort>
    {
        [NotNull]
        public string ToHexadecimalString(ushort value) { return value.ToString("X4"); }

		public string ToDecimalString(ushort value) { return value.ToString(); }

		public ushort One => 1;

		public ushort Zero => 0;

		public ushort And(ushort left, ushort right) { return (ushort)(left & right); }

		public int BitCount(ushort value) { return Number.BitCount(value); }

		public ushort Create(ulong value) { return (ushort)value; }

		public ushort Create(long value) { return (ushort)value; }

		public bool IsInValueRange(ulong value) { return value <= ushort.MaxValue; }

		public bool IsInValueRange(long value) { return value >= ushort.MinValue && value <= ushort.MaxValue; }

		public ushort LeftShift(ushort value, int amount) { return (ushort)(value << amount); }

		public bool LessThan(ushort left, ushort right) { return left < right; }

		public ushort Not(ushort value) { return (ushort)~value; }

		public ushort Or(ushort left, ushort right) { return (ushort)(left | right); }

		public ushort Subtract(ushort left, ushort right) { return (ushort)(left - right); }

		public bool TryParseNumber(string s, NumberStyles style, IFormatProvider provider, out ushort result) { return ushort.TryParse(s, style, provider, out result); }

		public bool TryParseNative(string s, out ushort result) { return ushort.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result); }

		public ushort Xor(ushort left, ushort right) { return (ushort)(left ^ right); }
	}
}
