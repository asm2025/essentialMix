using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Numeric;
using asm.Other.Microsoft.Collections;
using SysMath = System.Math;

namespace asm.Collections
{
	[Serializable]
	public class BitVectorList : ListBase<BitVector>, IList<byte>, IReadOnlyList<byte>
	{
		private const BitVectorMode MODE_DEF = BitVectorMode.Hexadecimal;

		private string _toString;
		private BitVectorMode _mode;

		public BitVectorList()
			: this(MODE_DEF)
		{
		}

		public BitVectorList(int capacity)
			: base(capacity)
		{
			_mode = MODE_DEF;
		}

		public BitVectorList(BitVectorMode mode)
		{
			_mode = mode;
		}

		public BitVectorList(int capacity, BitVectorMode mode)
			: base(capacity)
		{
			_mode = mode;
		}

		public BitVectorList(BitVector bitVector)
			: this(MODE_DEF)
		{
			base.Add(bitVector);
		}

		public BitVectorList([NotNull] IEnumerable<BitVector> enumerable)
			: base(enumerable)
		{
			_mode = MODE_DEF;
		}

		public BitVectorList([NotNull] IEnumerable<byte> enumerable)
			: this(MODE_DEF)
		{
			AddRange(enumerable);
		}

		public BitVectorList([NotNull] BitArray data, int startIndex = 0, int count = -1)
			: this(MODE_DEF)
		{
			AddRange(data, startIndex, count);
		}

		public BitVectorList(BitVector32 data, int startIndex = 0, int count = -1)
			: this(MODE_DEF)
		{
			AddRange(data, startIndex, count);
		}

		public BitVectorList([NotNull] IEnumerable<bool> data, int startIndex = 0, int count = -1)
			: this(MODE_DEF)
		{
			AddRange(data, startIndex, count);
		}

		public new BitVector this[int index]
		{
			get => base[index];
			set
			{
				base[index] = value;
				_toString = null;
			}
		}

		[NotNull]
		public override string ToString() { return _toString ??= ToString(this); }

		[NotNull]
		public string ToString(char separator) { return ToString(this, Mode, separator); }

		[NotNull]
		public string ToString(BitVectorMode mode, char separator = '\0') { return ToString(this, mode, separator); }

		/// <inheritdoc />
		protected override void Insert(int index, BitVector item, bool add)
		{
			base.Insert(index, item, add);
			_toString = null;
		}

		/// <inheritdoc />
		protected override void RangeInserted(int index, int count)
		{
			base.RangeInserted(index, count);
			_toString = null;
		}

		/// <inheritdoc />
		public override void RemoveAt(int index)
		{
			base.RemoveAt(index);
			_toString = null;
		}

		/// <inheritdoc />
		public override void Clear()
		{
			base.Clear();
			_toString = null;
		}

		byte IList<byte>.this[int index]
		{
			get => this[index].Data;
			set
			{
				if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
				RemoveAt(index);
				Insert(index, value);
			}
		}

		byte IReadOnlyList<byte>.this[int index] => this[index].Data;

		public BitVectorMode Mode
		{
			get => _mode;
			set
			{
				if (_mode == value) return;
				_mode = value;
				_toString = null;
			}
		}

		bool ICollection<byte>.IsReadOnly => ((ICollection<BitVector>)this).IsReadOnly;

		public bool IsValid(string value) { return IsValid(value, Mode); }

		[NotNull]
		public byte[] ToByteArray()
		{
			if (Count == 0) return Array.Empty<byte>();

			byte[] bytes = new byte[Count];
			CopyTo(bytes);
			return bytes;
		}

		[NotNull]
		public BitVectorList GetBits(int index, int position, int count)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			int bitPosition = index * 8 + position;
			return GetBits(bitPosition, count);
		}

		[NotNull]
		public BitVectorList GetBits(int position = 0, int count = -1)
		{
			int total = Count * 8;
			total.ValidateRange(position, ref count);
			if (position == 0 && count == total) return this;

			BitVectorList list = new BitVectorList();
			if (count == 0 || Count == 0) return list;

			int firstByte = position / 8;
			int c = 0;
			byte v = 0;
			byte data = this[firstByte].Data;
			int firstPosition = position % 8;
			int lastCount = 8 - firstPosition;
			byte mask = (byte)(1 << (lastCount - 1));
			count -= lastCount;

			// compute the first few bits of the byte where firstPosition occurs
			for (int i = firstPosition + lastCount - 1; i >= firstPosition; i--)
			{
				v >>= 1;
				if ((data & mask) != 0) v |= 0x80;
				c++;
				if (c % 8 == 0) break;
				mask >>= 1;
			}

			// right shift remaining bits
			if (c % 8 != 0) v >>= 8 - c % 8;
			list.Add(new BitVector(v));

			// add whatever bytes in the middle
			while (count > 8)
			{
				firstByte++;
				list.Add(this[firstByte]);
				count -= 8;
			}

			// compute the last few bits of the byte where count > 0
			if (count > 0)
			{
				c = 0;
				v = 0;
				firstByte++;
				data = this[firstByte].Data;
				mask = (byte)(1 << (count - 1));

				for (int i = count - 1; i >= 0; i--)
				{
					v >>= 1;
					if ((data & mask) != 0) v |= 0x80;
					c++;
					if (c % 8 == 0) break;
					mask >>= 1;
				}

				// right shift remaining bits
				if (c % 8 != 0) v >>= 8 - c % 8;
				list.Add(new BitVector(v));
			}

			return list;
		}

		public void AddRange([NotNull] IEnumerable<byte> enumerable)
		{
			foreach (byte b in enumerable)
				Add(new BitVector(b));
		}

		public void AddRange([NotNull] IEnumerable<bool> enumerable, int startIndex = 0, int count = -1)
		{
			int collectionCount = enumerable.FastCount();

			if (collectionCount > -1)
			{
				collectionCount.ValidateRange(startIndex, ref count);
			}
			else
			{
				if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
			}

			if (collectionCount == 0 || count == 0) return;

			int index = 0;
			int c = 0;
			byte v = 0;
			List<BitVector> bitVectors = new List<BitVector>((int)SysMath.Ceiling(count / 8.0d));

			foreach (bool b in enumerable.Skip(startIndex).Take(count).Reverse())
			{
				v >>= 1;
				if (b) v |= 0x80;
				c++;
				if (c % 8 != 0) continue;
				bitVectors.Add(new BitVector(v));
				v = 0;
				index++;
			}

			// right shift remaining bits
			if (c % 8 != 0) v >>= 8 - c % 8;

			// if last set of bits is less than 8 bits then it didn't get stored yet.
			if (index < count) bitVectors.Add(new BitVector(v));
			bitVectors.Reverse();
			AddRange(bitVectors);
		}

		public void AddRange([NotNull] BitArray data, int startIndex = 0, int count = -1)
		{
			data.Count.ValidateRange(startIndex, ref count);
			if (data.Count == 0 || count == 0) return;

			int index = 0;
			int c = 0;
			byte v = 0;
			BitVector[] bitVectors = new BitVector[(count + (8 - count) % 8) / 8];

			for (int i = startIndex + count - 1; i >= startIndex; i--)
			{
				v >>= 1;
				if (data[i]) v |= 0x80;
				c++;
				if (c % 8 != 0) continue;
				bitVectors[index] = new BitVector(v);
				v = 0;
				index++;
			}

			// right shift remaining bits
			if (c % 8 != 0) v >>= 8 - c % 8;

			// if last set of bits is less than 8 bits then it didn't get stored yet.
			if (index < count) bitVectors[index] = new BitVector(v);
			Array.Reverse(bitVectors);
			AddRange(bitVectors);
		}

		public void AddRange(BitVector32 data, int startIndex = 0, int count = -1)
		{
			if (!startIndex.InRangeRx(0, Constants.INT_BIT_SIZE)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (count == -1) count = Constants.INT_BIT_SIZE;
			if (!count.InRange(0, Constants.INT_BIT_SIZE)) throw new ArgumentOutOfRangeException(nameof(count));
			if (startIndex + count > Constants.INT_BIT_SIZE) count = Constants.INT_BIT_SIZE - startIndex;
			if (count == 0) return;
			
			int c = 0;
			int index = 0;
			byte v = 0;
			BitVector[] bitVectors = new BitVector[(count + (8 - count) % 8) / 8];

			for (int i = startIndex + count - 1; i >= startIndex; i--)
			{
				v >>= 1;
				if (data[i]) v |= 0x80;
				c++;
				if (c % 8 != 0) continue;
				bitVectors[index] = new BitVector(v);
				v = 0;
				index++;
			}

			// right shift remaining bits
			if (c % 8 != 0) v >>= 8 - c % 8;

			// if last set of bits is less than 8 bits then it didn't get stored yet.
			if (index < count) bitVectors[index] = new BitVector(v);
			Array.Reverse(bitVectors);
			AddRange(bitVectors);
		}

		public void Add(byte value) { base.Add(new BitVector(value)); }
		public void Add(bool value) { AddRange(BitConverter.GetBytes(value)); }
		public void Add(char value) { AddRange(BitConverter.GetBytes(value)); }
		public void Add(short value) { AddRange(BitConverter.GetBytes(value)); }
		public void Add(ushort value) { AddRange(BitConverter.GetBytes(value)); }
		public void Add(int value) { AddRange(BitConverter.GetBytes(value)); }
		public void Add(uint value) { AddRange(BitConverter.GetBytes(value)); }
		public void Add(long value) { AddRange(BitConverter.GetBytes(value)); }
		public void Add(ulong value) { AddRange(BitConverter.GetBytes(value)); }
		public void Add(float value) { AddRange(BitConverter.GetBytes(value)); }
		public void Add(double value) { AddRange(BitConverter.GetBytes(value)); }
		public void Add(decimal value) { AddRange(value.GetBytes()); }
		public void Add(BigInteger value) { AddRange(value.ToByteArray()); }

		public void Add([NotNull] string value, int startIndex = 0, int count = -1) { Add(value, Mode, startIndex, count); }
		public void Add([NotNull] string value, BitVectorMode mode, int startIndex = 0, int count = -1)
		{
			value.Length.ValidateRange(startIndex, ref count);
			if (value.Length == 0 || count == 0) return;

			Predicate<char> validationFunction = BitVector.GetValidationFunction(mode);

			foreach (string part in value.Partition(startIndex, count, BitVector.GetUnitLength(mode), PartitionSize.PerPartition, '0'))
			{
				if (!part.All(c => validationFunction(c))) throw new FormatException("String is not in the correct format");
				Add(new BitVector(Convert.ToByte(part, (int)mode)));
			}
		}

		public void Insert(int index, byte item) { base.Insert(index, new BitVector(item)); }

		public bool Remove(byte item)
		{
			int index = IndexOf(item);
			if (index < 0) return false;
			RemoveAt(index);
			return true;
		}

		public bool Contains(byte item) { return FindIndex(bv => bv.Data == item) != -1; }

		public void CopyTo([NotNull] byte[] array) { CopyTo(0, array, 0, Count); }
		public void CopyTo(byte[] array, int arrayIndex) { CopyTo(0, array, arrayIndex, Count); }
		public void CopyTo(int index, [NotNull] byte[] array, int arrayIndex, int count)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			Count.ValidateRange(index, ref count);
			array.Length.ValidateRange(arrayIndex, ref count);
			if (count == 0 || Count == 0) return;

			int lastIndex = index + count - 1;
			int lastArrayIndex = arrayIndex + count - 1;

			for (int i = index, j = arrayIndex; i <= lastIndex && j <= lastArrayIndex; i++, j++)
				array[j] = this[i].Data;
		}

		public int IndexOf(byte item) { return FindIndex(bv => bv.Data == item); }

		IEnumerator<byte> IEnumerable<byte>.GetEnumerator() { return this.CastEnumerator<BitVector, byte>(); }

		public static bool IsValid(string value, BitVectorMode mode)
		{
			if (string.IsNullOrEmpty(value)) return true;

			Predicate<char> validationFunction = BitVector.GetValidationFunction(mode);
			return value.All(c => validationFunction(c));
		}

		[NotNull]
		public static string ToString([NotNull] BitVectorList list, char? separator = null) { return ToString(list, list.Mode, separator); }

		[NotNull]
		public static string ToString([NotNull] ICollection<BitVector> collection, BitVectorMode mode, char? separator = null)
		{
			if (collection.Count == 0) return string.Empty;

			Func<BitVector, string> convertFunction = BitVector.GetToFunction(mode);
			return separator.HasValue
				? string.Join(separator.ToString(), collection.Select(convertFunction))
				: string.Concat(collection.Select(convertFunction));
		}

		[NotNull]
		public static BitVectorList FromString([NotNull] string value, BitVectorMode mode)
		{
			if (value.Length == 0) return new BitVectorList();

			int unitLength = BitVector.GetUnitLength(mode);
			int capacity = value.Length + (unitLength - value.Length % unitLength) / unitLength;
			return new BitVectorList(capacity, mode) {value};
		}

		[NotNull]
		public static BitVectorList FromBytes([NotNull] byte[] buffer, BitVectorMode mode)
		{
			return new BitVectorList(buffer)
					{
						Mode = mode
					};
		}

		public static explicit operator BitVectorList(byte[] value)
		{
			return value == null
						? null
						: new BitVectorList(value);
		}

		[NotNull]
		public static explicit operator byte[](BitVectorList value) { return value?.ToByteArray() ?? Array.Empty<byte>(); }

		public static explicit operator BitVectorList(bool[] value)
		{
			return value == null
						? null
						: new BitVectorList(value);
		}

		[NotNull]
		public static explicit operator bool[](BitVectorList value)
		{
			if (value == null || value.Count == 0) return Array.Empty<bool>();

			bool[] bits = new bool[value.Count * 8];

			for (int i = 0; i < value.Count; i++)
				Array.Copy((bool[])value[i], 0, bits, i * 8, 8);

			return bits;
		}

		[NotNull]
		public static explicit operator string (BitVectorList value) { return value?.ToString() ?? string.Empty; }
	}
}