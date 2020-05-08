using System;
using System.Globalization;
using JetBrains.Annotations;

namespace asm.Numeric
{
	internal class BooleanNumericProvider : INumericProvider<bool>
    {
        public bool One => true;

		public bool Zero => false;

		public bool And(bool left, bool right) { return left & right; }

		public int BitCount(bool value) { return Number.BitCount(Convert.ToByte(value)); }

		public bool Create(ulong value) { return value != 0UL; }

		public bool Create(long value) { return value != 0L; }

		public bool IsInValueRange(ulong value) { return value <= byte.MaxValue; }

		public bool IsInValueRange(long value) { return value >= 0L && value <= byte.MaxValue; }

		public bool LeftShift(bool value, int amount) { return !value & amount == 1; }

		public bool LessThan(bool left, bool right) { return !left & right; }

		public bool Not(bool value) { return !value; }

		public bool Or(bool left, bool right) { return left | right; }

		public bool Subtract(bool left, bool right) { return left ^ right; }

		[NotNull] public string ToHexadecimalString(bool value) { return Convert.ToByte(value).ToString("X2"); }

		public string ToDecimalString(bool value) { return Convert.ToByte(value).ToString(); }

		public bool TryParseNumber(string s, NumberStyles style, IFormatProvider provider, out bool result)
        {
            bool success = byte.TryParse(s, style, provider, out byte resultAsByte);
            result = Convert.ToBoolean(resultAsByte);
            return success;
        }

        public bool TryParseNative(string s, out bool result) { return bool.TryParse(s, out result); }

		public bool Xor(bool left, bool right) { return left != right; }
	}
}
