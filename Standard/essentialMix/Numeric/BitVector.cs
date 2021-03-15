using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JetBrains.Annotations;
using essentialMix.Extensions;

namespace essentialMix.Numeric
{
	[Serializable]
	public struct BitVector
	{
		private byte _data;

		public BitVector(BitVector bitVector)
			: this(bitVector.Data) { }

		public BitVector(byte data)
		{
			_data = data;
		}

		public BitVector([NotNull] BitArray data, int startIndex = 0, int count = -1)
		{
			if (!startIndex.InRangeRx(0, data.Count)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (count == -1) count = (data.Count - startIndex).NotAbove(8);
			if (!count.InRange(0, 8)) throw new ArgumentOutOfRangeException(nameof(count));
			if (startIndex + count > data.Count) count = data.Count - startIndex;

			if (data.Count == 0 || count == 0)
			{
				_data = 0;
				return;
			}

			int c = 0;
			byte v = 0;

			for (int i = startIndex + count - 1; i >= startIndex; i--)
			{
				v >>= 1;
				if (data[i]) v |= 0x80;
				c++;
				if (c % 8 == 0) break;
			}

			// right shift remaining bits
			if (c % 8 != 0) v >>= 8 - c % 8;
			_data = v;
		}

		public BitVector(BitVector32 data, int startIndex = 0, int count = -1)
		{
			if (!startIndex.InRangeRx(0, 8)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (count == -1) count = 8 - startIndex;
			if (!count.InRange(0, 8)) throw new ArgumentOutOfRangeException(nameof(count));
			if (startIndex + count > 8) count = 8 - startIndex;

			if (count == 0)
			{
				_data = 0;
				return;
			}

			int c = 0;
			byte v = 0;

			for (int i = startIndex + count - 1; i >= startIndex; i--)
			{
				v >>= 1;
				if (data[i]) v |= 0x80;
				c++;
				if (c % 8 == 0) break;
			}

			// right shift remaining bits
			if (c % 8 != 0) v >>= 8 - c % 8;
			_data = v;
		}

		public BitVector([NotNull] IEnumerable<bool> data, int startIndex = 0, int count = -1)
		{
			int collectionCount = (data as ICollection<bool>)?.Count ?? (data as IReadOnlyCollection<bool>)?.Count ?? -1;

			if (collectionCount > -1)
			{
				if (!startIndex.InRangeRx(0, collectionCount)) throw new ArgumentOutOfRangeException(nameof(startIndex));
				if (count == -1) count = (collectionCount - startIndex).NotAbove(8);
				if (!count.InRange(0, 8)) throw new ArgumentOutOfRangeException(nameof(count));
				if (startIndex + count > collectionCount) count = collectionCount - startIndex;

				if (collectionCount == 0)
				{
					_data = 0;
					return;
				}
			}
			else
			{
				if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
				if (count == -1) count = 8;
				if (!count.InRange(0, 8)) throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (count == 0)
			{
				_data = 0;
				return;
			}

			int c = 0;
			byte v = 0;

			foreach (bool b in data.Skip(startIndex).Take(count).Reverse())
			{
				v >>= 1;
				if (b) v |= 0x80;
				c++;
				if (c % 8 == 0) break;
			}

			// right shift remaining bits
			if (c % 8 != 0) v >>= 8 - c % 8;
			_data = v;
		}

		[NotNull]
		public static explicit operator bool[](BitVector value)
		{
			/*
			 * the mask initially is: 10000000 then we can test the 7th bit
			 * then it becomes      : 01000000 so we can test the 6th bit
			*/
			byte mask = 0x80;
			byte b = value.Data;
			bool[] bit = new bool[8];

			for (int i = 7; i >= 0; i--)
			{
				bit[i] = (b & mask) > 0;
				mask >>= 1;
			}

			return bit;
		}

		public static explicit operator BitVector([NotNull] BitArray value)
		{
			return new BitVector(value);
		}

		[NotNull]
		public static explicit operator BitArray(BitVector value)
		{
			return new BitArray(new[] { value.Data });
		}

		public static explicit operator BitVector(BitVector32 value) { return new BitVector(value); }

		public static explicit operator BitVector32(BitVector value) { return new BitVector32(value.Data); }

		public override bool Equals(object obj)
		{
			if (!(obj is BitVector)) return false;
			return Equals((BitVector)obj);
		}

		public bool Equals(BitVector other) { return _data == other._data; }

		public override int GetHashCode() { return Data; }

		[NotNull]
		public override string ToString() { return ToHexadecimalString(this); }

		public bool this[byte index]
		{
			get
			{
				if (!index.InRangeRx<byte>(0, 8)) throw new ArgumentOutOfRangeException(nameof(index));
				return (_data & (1 << (index - 1))) != 0;
			}
			set
			{
				if (!index.InRangeRx<byte>(0, 8)) throw new ArgumentOutOfRangeException(nameof(index));
				_data = value ? (byte)(_data | (1 << index)) : (byte)(_data & ~(1 << index));
			}
		}

		public byte Data
		{
			get => _data;
			set => _data = value;
		}

		public static byte CreateMask() { return CreateMask(0); }

		public static byte CreateMask(byte previous)
		{
			if (previous == 0) return 1;
			return (byte)(previous << 1);
		}

		public static string ToString(BitVector value, BitVectorMode mode) { return GetToFunction(mode)(value); }

		[NotNull]
		public static string ToBinaryString(BitVector value) { return Convert.ToString(value.Data, (int)BitVectorMode.Binary).PadLeft(8, '0'); }

		[NotNull]
		public static string ToOctalString(BitVector value) { return Convert.ToString(value.Data, (int)BitVectorMode.Octal).PadLeft(3, '0'); }

		[NotNull]
		public static string ToDecimalString(BitVector value) { return value.Data.ToString("D3"); }

		[NotNull]
		public static string ToHexadecimalString(BitVector value) { return value.Data.ToString("X2"); }

		public static BitVector FromString(string value, BitVectorMode mode) { return GetFromFunction(mode)(value); }

		public static BitVector FromBinaryString([NotNull] string value)
		{
			if (!value.All(c => c.IsBinary())) throw new FormatException("String was not in the correct format");
			if (value.Length != 8) value = value.PadLeft(8, '0');
			return new BitVector(Convert.ToByte(value, (int)BitVectorMode.Binary));
		}

		public static BitVector FromOctalString([NotNull] string value)
		{
			if (!value.All(c => c.IsOctal())) throw new FormatException("String was not in the correct format");
			if (value.Length != 3) value = value.PadLeft(3, '0');
			return new BitVector(Convert.ToByte(value, (int)BitVectorMode.Octal));
		}

		public static BitVector FromDecimalString([NotNull] string value)
		{
			if (!value.All(c => c.IsDigit())) throw new FormatException("String was not in the correct format");
			if (value.Length != 3) value = value.PadLeft(3, '0');
			return new BitVector(Convert.ToByte(value, (int)BitVectorMode.Decimal));
		}

		public static BitVector FromHexadecimalString([NotNull] string value)
		{
			if (!value.All(c => c.IsHexadecimal())) throw new FormatException("String was not in the correct format");
			if (value.Length != 2) value = value.PadLeft(2, '0');
			return new BitVector(Convert.ToByte(value, (int)BitVectorMode.Hexadecimal));
		}

		public static int GetUnitLength(BitVectorMode mode)
		{
			switch (mode)
			{
				case BitVectorMode.Binary:
					return 8;
				case BitVectorMode.Octal:
				case BitVectorMode.Decimal:
					return 3;
				case BitVectorMode.Hexadecimal:
					return 2;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}

		[NotNull]
		public static Predicate<char> GetValidationFunction(BitVectorMode mode)
		{
			switch (mode)
			{
				case BitVectorMode.Binary:
					return CharExtension.IsBinary;
				case BitVectorMode.Octal:
					return CharExtension.IsOctal;
				case BitVectorMode.Decimal:
					return CharExtension.IsDigit;
				case BitVectorMode.Hexadecimal:
					return CharExtension.IsHexadecimal;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}

		[NotNull]
		public static Func<BitVector, string> GetToFunction(BitVectorMode mode)
		{
			switch (mode)
			{
				case BitVectorMode.Binary:
					return ToBinaryString;
				case BitVectorMode.Octal:
					return ToOctalString;
				case BitVectorMode.Decimal:
					return ToDecimalString;
				case BitVectorMode.Hexadecimal:
					return ToHexadecimalString;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}

		[NotNull]
		public static Func<string, BitVector> GetFromFunction(BitVectorMode mode)
		{
			switch (mode)
			{
				case BitVectorMode.Binary:
					return FromBinaryString;
				case BitVectorMode.Octal:
					return FromOctalString;
				case BitVectorMode.Decimal:
					return FromDecimalString;
				case BitVectorMode.Hexadecimal:
					return FromHexadecimalString;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}
	}
}