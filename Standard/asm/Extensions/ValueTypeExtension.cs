using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;
using asm.Collections;
using asm.Helpers;
using asm.Numeric;
using asm.Text;
using SysMath = System.Math;

namespace asm.Extensions
{
	public static class ValueTypeExtension
	{
		private const string NUM_SELF_FORMAT = "#.##############################";
		private const byte SBYTE_DIFF = sbyte.MinValue * -1;

		public static int Value(this bool thisValue)
		{
			return thisValue
						? 1
						: 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string JSValue(this bool thisValue) { return thisValue.ToString().ToLower(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Iif<T>(this bool thisValue, T trueResponse, T falseResponse = default)
		{
			return thisValue
						? trueResponse
						: falseResponse;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IifNot<T>(this bool thisValue, T trueResponse, T falseResponse = default)
		{
			return !thisValue
						? trueResponse
						: falseResponse;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Iif<T>(this bool thisValue, [NotNull] Func<T> trueFunc, Func<T> falseFunc = null)
		{
			return thisValue
						? trueFunc()
						: falseFunc == null
							? default
							: falseFunc();
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IifNot<T>(this bool thisValue, [NotNull] Func<T> trueFunc, Func<T> falseFunc = null)
		{
			return !thisValue
						? trueFunc()
						: falseFunc == null
							? default
							: falseFunc();
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool HasFlag(this sbyte thisValue, sbyte flag) { return HasFlag(thisValue.ToByte(), flag.ToByte()); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool HasFlag(this byte thisValue, byte flag) { return (thisValue & flag) == flag; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool HasFlag(this short thisValue, short flag) { return HasFlag((ushort)thisValue, (ushort)flag); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool HasFlag(this ushort thisValue, ushort flag) { return (thisValue & flag) == flag; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool HasFlag(this int thisValue, int flag) { return HasFlag((uint)thisValue, (uint)flag); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool HasFlag(this uint thisValue, uint flag) { return (thisValue & flag) == flag; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool HasFlag(this long thisValue, long flag) { return HasFlag((ulong)thisValue, (ulong)flag); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool HasFlag(this ulong thisValue, ulong flag) { return (thisValue & flag) == flag; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void ValidateRange(this int thisValue, int startIndex, ref int count)
		{
			if (!startIndex.InRangeRx(0, thisValue)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (count == -1) count = thisValue - startIndex;
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (startIndex + count > thisValue) count = thisValue.Subtract(startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void ValidateRange(this long thisValue, long startIndex, ref long count)
		{
			if (!startIndex.InRangeRx(0, thisValue)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (count == -1) count = thisValue - startIndex;
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (startIndex + count > thisValue) count = thisValue.Subtract(startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void ValidateRange(this int thisValue, int startIndex) { ValidateRange(thisValue, startIndex, thisValue); }
		
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void ValidateRange(this int thisValue, int startIndex, int count)
		{
			if (!startIndex.InRangeRx(0, thisValue)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (startIndex + count > thisValue) throw new ArgumentOutOfRangeException(nameof(count));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void ValidateRange(this long thisValue, long startIndex) { ValidateRange(thisValue, startIndex, thisValue); }
		
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void ValidateRange(this long thisValue, long startIndex, long count)
		{
			if (!startIndex.InRangeRx(0, thisValue)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (startIndex + count > thisValue) throw new ArgumentOutOfRangeException(nameof(count));
		}

		[NotNull]
		public static byte[] ToBytes(this bool thisValue, Endianness endianness = Endianness.Default)
		{
			byte[] bytes = BitConverter.GetBytes(thisValue);

			if (bytes.Length > 1)
			{
				switch (endianness)
				{
					case Endianness.LittleEndian:
						if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
						break;
					case Endianness.BigEndian:
						if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
						break;
				}
			}

			return bytes;
		}

		[NotNull]
		public static byte[] ToBytes(this char thisValue, Endianness endianness = Endianness.Default)
		{
			byte[] bytes = BitConverter.GetBytes(thisValue);

			if (bytes.Length > 1)
			{
				switch (endianness)
				{
					case Endianness.LittleEndian:
						if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
						break;
					case Endianness.BigEndian:
						if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
						break;
				}
			}

			return bytes;
		}

		[NotNull]
		public static byte[] ToBytes(this sbyte thisValue)
		{
			return new[]
					{
						thisValue.ToByte()
					};
		}

		[NotNull]
		public static byte[] ToBytes(this byte thisValue)
		{
			return new[]
					{
						thisValue
					};
		}

		[NotNull]
		public static byte[] ToBytes(this short thisValue, Endianness endianness = Endianness.Default)
		{
			byte[] bytes = BitConverter.GetBytes(thisValue);

			switch (endianness)
			{
				case Endianness.LittleEndian:
					if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
				case Endianness.BigEndian:
					if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
			}

			return bytes;
		}

		[NotNull]
		public static byte[] ToBytes(this ushort thisValue, Endianness endianness = Endianness.Default)
		{
			byte[] bytes = BitConverter.GetBytes(thisValue);

			switch (endianness)
			{
				case Endianness.LittleEndian:
					if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
				case Endianness.BigEndian:
					if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
			}

			return bytes;
		}

		[NotNull]
		public static byte[] ToBytes(this int thisValue, Endianness endianness = Endianness.Default)
		{
			byte[] bytes = BitConverter.GetBytes(thisValue);

			switch (endianness)
			{
				case Endianness.LittleEndian:
					if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
				case Endianness.BigEndian:
					if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
			}

			return bytes;
		}

		[NotNull]
		public static byte[] ToBytes(this uint thisValue, Endianness endianness = Endianness.Default)
		{
			byte[] bytes = BitConverter.GetBytes(thisValue);

			switch (endianness)
			{
				case Endianness.LittleEndian:
					if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
				case Endianness.BigEndian:
					if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
			}

			return bytes;
		}

		[NotNull]
		public static byte[] ToBytes(this long thisValue, Endianness endianness = Endianness.Default)
		{
			byte[] bytes = BitConverter.GetBytes(thisValue);

			switch (endianness)
			{
				case Endianness.LittleEndian:
					if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
				case Endianness.BigEndian:
					if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
			}

			return bytes;
		}

		[NotNull]
		public static byte[] ToBytes(this ulong thisValue, Endianness endianness = Endianness.Default)
		{
			byte[] bytes = BitConverter.GetBytes(thisValue);

			switch (endianness)
			{
				case Endianness.LittleEndian:
					if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
				case Endianness.BigEndian:
					if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
			}

			return bytes;
		}

		[NotNull]
		public static byte[] ToBytes(this float thisValue, Endianness endianness = Endianness.Default)
		{
			byte[] bytes = BitConverter.GetBytes(thisValue);

			switch (endianness)
			{
				case Endianness.LittleEndian:
					if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
				case Endianness.BigEndian:
					if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
			}

			return bytes;
		}

		[NotNull]
		public static byte[] ToBytes(this double thisValue, Endianness endianness = Endianness.Default)
		{
			byte[] bytes = BitConverter.GetBytes(thisValue);

			switch (endianness)
			{
				case Endianness.LittleEndian:
					if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
				case Endianness.BigEndian:
					if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
					break;
			}

			return bytes;
		}

		[NotNull]
		public static byte[] ToBytes(this decimal thisValue, Endianness endianness = Endianness.Default)
		{
			byte[] bytes = new byte[Constants.DECIMAL_SIZE];
			int[] bits = decimal.GetBits(thisValue);

			for (int i = 0; i < bits.Length; i++)
			{
				byte[] buffer = BitConverter.GetBytes(bits[i]);

				switch (endianness)
				{
					case Endianness.LittleEndian:
						if (!BitConverter.IsLittleEndian) Array.Reverse(buffer);
						break;
					case Endianness.BigEndian:
						if (BitConverter.IsLittleEndian) Array.Reverse(buffer);
						break;
				}

				Array.Copy(buffer, 0, bytes, i * Constants.INT_SIZE, Constants.INT_SIZE);
			}

			return bytes;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static long ToLong(this double thisValue) { return BitConverter.DoubleToInt64Bits(thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double ToDouble(this long thisValue) { return BitConverter.Int64BitsToDouble(thisValue); }

		public static NumericRelationship Relationship([NotNull] this ValueType thisValue, [NotNull] ValueType value)
		{
			// Use BigInteger as common integral type
			if (thisValue.IsIntegral() && value.IsIntegral())
			{
				BigInteger bigint1 = (BigInteger)thisValue;
				BigInteger bigint2 = (BigInteger)value;
				return (NumericRelationship)BigInteger.Compare(bigint1, bigint2);
			}

			if (!thisValue.IsNumeric()) throw new ArgumentException("thisValue is not a number.");
			if (!value.IsNumeric()) throw new ArgumentException("value is not a number.");

			// At least one value is floating point; use Double.
			double dbl1 = thisValue.To(double.NaN);
			double dbl2 = value.To(double.NaN);
			return (NumericRelationship)dbl1.CompareTo(dbl2);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static DateTime ToOADateTime(this double thisValue)
		{
			if (thisValue < 0.0D) throw new ArgumentOutOfRangeException(nameof(thisValue));
			return DateTimeHelper.OaEpoch + TimeSpan.FromTicks(Convert.ToInt64(thisValue * TimeSpan.TicksPerDay));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static byte ToByte(this sbyte thisValue) { return (byte)(thisValue + SBYTE_DIFF); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static sbyte ToSByte(this byte thisValue) { return (sbyte)(thisValue - SBYTE_DIFF); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool ToBoolean([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length - sizeof(bool))) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return BitConverter.ToBoolean(thisValue, startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static char ToChar([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length - sizeof(char))) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return BitConverter.ToChar(thisValue, startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static sbyte ToSByte([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length - sizeof(sbyte))) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return ToSByte(thisValue[startIndex]);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static byte ToByte([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length - sizeof(byte))) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return thisValue[startIndex];
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static short ToInt16([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length - sizeof(short))) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return BitConverter.ToInt16(thisValue, startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ushort ToUInt16([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length - sizeof(ushort))) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return BitConverter.ToUInt16(thisValue, startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int ToInt32([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length - Constants.INT_SIZE)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return BitConverter.ToInt32(thisValue, startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static uint ToUInt32([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length - sizeof(uint))) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return BitConverter.ToUInt32(thisValue, startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static long ToInt64([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length - sizeof(long))) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return BitConverter.ToInt64(thisValue, startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ulong ToUInt64([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length - sizeof(ulong))) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return BitConverter.ToUInt64(thisValue, startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static float ToSingle([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length - sizeof(float))) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return BitConverter.ToSingle(thisValue, startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double ToDouble([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length - sizeof(double))) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return BitConverter.ToDouble(thisValue, startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static decimal ToDecimal([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			const byte DECIMAL_SIGN_BIT = 128;

			if (!startIndex.InRangeRx(0, thisValue.Length - sizeof(decimal))) throw new ArgumentOutOfRangeException(nameof(startIndex));
			return new decimal(BitConverter.ToInt32(thisValue, startIndex),
				BitConverter.ToInt32(thisValue, startIndex + 4),
				BitConverter.ToInt32(thisValue, startIndex + 8),
				thisValue[startIndex + 15] == DECIMAL_SIGN_BIT,
				thisValue[startIndex + 14]);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string ToBase64String([NotNull] this byte[] thisValue, int startIndex, int count)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			return count == 0 || thisValue.Length == 0
				? string.Empty 
				: Convert.ToBase64String(thisValue, startIndex, count);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string ToHexString(this byte[] thisValue)
		{
			return thisValue is null
						? null
						: thisValue.Length == 0
						? string.Empty
						: string.Concat(thisValue.Select(e => e.ToString("X2")));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Guid ToGuid(this byte[] thisValue)
		{
			return thisValue is null || thisValue.Length != 16
						? Guid.Empty
						: new Guid(thisValue);
		}

		public static unsafe void Swap(this byte[] thisValue)
		{
			if (thisValue == null || thisValue.Length == 0) return;

			fixed (byte* pThisValue = thisValue)
			{
#if X86
				long length = *((uint*)(thisValue - 4)) & 0xFFFFFFFEU;
#elif X64
				long length = *((uint*)(thisValue - 8)) & 0xFFFFFFFEU;
#else
				long length = thisValue.Length & 0xFFFFFFFE;
#endif
				
				while (length > 7)
				{
					length -= 8;
					ulong* pul = (ulong*)(pThisValue + length);
					*pul = ((*pul >> 24) & 0x000000FF000000FFUL) |
							((*pul >> 8) & 0x0000FF000000FF00UL) |
							((*pul << 8) & 0x00FF000000FF0000UL) |
							((*pul << 24) & 0xFF000000FF000000UL);
				}

				if (length > 0)
				{
					uint* pui = (uint*)pThisValue;
					*pui = (*pui >> 24) | 
							((*pui >> 8) & 0x0000FF00U) | 
							((*pui << 8) & 0x00FF0000U) | 
							(*pui << 24);
				}
			}
		}

		public static object Deserialize([NotNull] this byte[] thisValue, int startIndex = 0, int count = -1)
		{
			if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (count == -1) count = thisValue.Length - startIndex;
			if (startIndex > thisValue.Length - count) throw new ArgumentOutOfRangeException(nameof(count));

			byte[] buffer;
			
			if (startIndex == 0 && count == thisValue.Length)
				buffer = thisValue;
			else
			{
				buffer = new byte[count];
				Array.Copy(thisValue, startIndex, buffer, 0, count);
			}
			
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				IFormatter formatter = new BinaryFormatter();
				return formatter.Deserialize(stream);
			}
		}

		public static T Deserialize<T>([NotNull] this byte[] thisValue, int startIndex = 0)
		{
			Type type = typeof(T);
			int cb = Marshal.SizeOf(type);
			if (cb > thisValue.Length - startIndex) throw new ArgumentException("Not enough data to fill the requested type.", nameof(thisValue));
			return (T)Deserialize(thisValue, startIndex, cb);
		}

		public static T DeserializeRaw<T>([NotNull] this byte[] thisValue, int startIndex = 0)
			where T : struct
		{
			Type type = typeof(T);
			int rawSize = Marshal.SizeOf(type);
			if (rawSize > thisValue.Length - startIndex) throw new ArgumentException("Not enough data to fill the requested type.", nameof(thisValue));

			IntPtr buffer = Marshal.AllocHGlobal(rawSize);
			Marshal.Copy(thisValue, startIndex, buffer, rawSize);
			T result = (T)Marshal.PtrToStructure(buffer, type);
			Marshal.FreeHGlobal(buffer);
			return result;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DigitCount(this sbyte thisValue) { return DigitCount((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DigitCount(this byte thisValue) { return DigitCount((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DigitCount(this short thisValue) { return DigitCount((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DigitCount(this ushort thisValue) { return DigitCount((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DigitCount(this int thisValue) { return DigitCount((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DigitCount(this uint thisValue) { return DigitCount((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DigitCount(this long thisValue) { return DigitCount((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DigitCount(this ulong thisValue) { return DigitCount((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DigitCount(this float thisValue) { return DigitCount((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DigitCount(this double thisValue) { return DigitCount((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DigitCount(this decimal thisValue)
		{
			thisValue = SysMath.Abs(thisValue);
			ulong d = (ulong)SysMath.Floor(thisValue);
			decimal f = Fraction(thisValue);
			int n;
	
			for (n = 0; d > 0UL; n++, d /= 10UL)
			{
			}

			if (f > decimal.Zero) n++;

			while (f > decimal.Zero)
			{
				n++;
				f *= 10.0m;
				f -= SysMath.Floor(f);
			}

			return n;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DecimalPlaces(this sbyte thisValue) { return DecimalPlaces((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DecimalPlaces(this byte thisValue) { return DecimalPlaces((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DecimalPlaces(this short thisValue) { return DecimalPlaces((decimal)thisValue); }
		
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DecimalPlaces(this ushort thisValue) { return DecimalPlaces((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DecimalPlaces(this int thisValue) { return DecimalPlaces((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DecimalPlaces(this uint thisValue) { return DecimalPlaces((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DecimalPlaces(this long thisValue) { return DecimalPlaces((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DecimalPlaces(this ulong thisValue) { return DecimalPlaces((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DecimalPlaces(this float thisValue) { return DecimalPlaces((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DecimalPlaces(this double thisValue) { return DecimalPlaces((decimal)thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int DecimalPlaces(this decimal thisValue)
		{
			string s = SysMath.Abs(thisValue).ToString(NUM_SELF_FORMAT);
			int n = s.IndexOf('.') + 1;
			return n == 0 || n == s.Length
						? 0
						: s.Length - n;
		}

		[NotNull]
		public static byte[] GetBytes(this decimal thisValue)
		{
			int[] bits = decimal.GetBits(thisValue);
			byte[] bytes = new byte[bits.Length * Constants.INT_SIZE];

			for (int i = 0; i < bits.Length; i++)
				Array.Copy(BitConverter.GetBytes(bits[i]), 0, bytes, i * Constants.INT_SIZE, Constants.INT_SIZE);

			return bytes;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static object ToEnum(this bool thisValue, [NotNull] Type type) { return Enum.ToObject(type, thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static object ToEnum(this char thisValue, [NotNull] Type type) { return Enum.ToObject(type, thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static object ToEnum(this sbyte thisValue, [NotNull] Type type) { return Enum.ToObject(type, thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static object ToEnum(this byte thisValue, [NotNull] Type type) { return Enum.ToObject(type, thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static object ToEnum(this short thisValue, [NotNull] Type type) { return Enum.ToObject(type, thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static object ToEnum(this ushort thisValue, [NotNull] Type type) { return Enum.ToObject(type, thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static object ToEnum(this int thisValue, [NotNull] Type type) { return Enum.ToObject(type, thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static object ToEnum(this uint thisValue, [NotNull] Type type) { return Enum.ToObject(type, thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static object ToEnum(this long thisValue, [NotNull] Type type) { return Enum.ToObject(type, thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static object ToEnum(this ulong thisValue, [NotNull] Type type) { return Enum.ToObject(type, thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static float Fraction(this float thisValue) { return thisValue % 1.0f; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Fraction(this double thisValue) { return thisValue % 1.0d; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static decimal Fraction(this decimal thisValue) { return thisValue % 1.0m; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static float Supplement(this float thisValue)
		{
			float fraction = SysMath.Abs(Fraction(thisValue));
			return fraction < float.Epsilon ? 0.0f : 1.0f - fraction;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Supplement(this double thisValue)
		{
			double fraction = SysMath.Abs(Fraction(thisValue));
			return fraction < double.Epsilon ? 0.0d : 1.0d - fraction;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static decimal Supplement(this decimal thisValue)
		{
			decimal fraction = SysMath.Abs(Fraction(thisValue));
			return fraction == decimal.Zero ? 0.0m : 1.0m - fraction;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static BitVectorList AsBitVectorList([NotNull] this byte[] thisValue) { return (BitVectorList)thisValue; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static BitVectorList AsBitVectorList([NotNull] this bool[] thisValue) { return (BitVectorList)thisValue; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static DwordList AsDwordList([NotNull] this byte[] thisValue) { return (DwordList)thisValue; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static WordList AsWordList([NotNull] this byte[] thisValue) { return (WordList)thisValue; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static BitVector AsBitVector(this byte thisValue) { return (BitVector)thisValue; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static BitVector AsBitVector([NotNull] this bool[] thisValue) { return (BitVector)thisValue; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static UnionShort AsUnionShort(this short thisValue) { return (UnionShort)thisValue; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static UnionUnsignedShort AsUnionUShort(this ushort thisValue) { return (UnionUnsignedShort)thisValue; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static UnionInteger AsUnionInt(this int thisValue) { return (UnionInteger)thisValue; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static UnionUnsignedInteger AsUnionUInt(this uint thisValue) { return (UnionUnsignedInteger)thisValue; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static UnionLong AsUnionLong(this long thisValue) { return (UnionLong)thisValue; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static UnionUnsignedLong AsUnionULong(this ulong thisValue) { return (UnionUnsignedLong)thisValue; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static UnionBigInteger AsUnionBigInteger(this BigInteger thisValue) { return (UnionBigInteger)thisValue; }

		/// <summary>
		/// Converts the given double to a string representation of its
		/// exact decimal value.
		/// </summary>
		/// <param name="thisValue">The double to convert.</param>
		/// <returns>A string representation of the double's exact decimal value.</returns>
		[NotNull]
		public static string ToExactString(this double thisValue)
		{
			if (double.IsPositiveInfinity(thisValue)) return "+Infinity";
			if (double.IsNegativeInfinity(thisValue)) return "-Infinity";
			if (double.IsNaN(thisValue)) return "NaN";

			// Translate the double into sign, exponent and mantissa.
			long bits = BitConverter.DoubleToInt64Bits(thisValue);
			bool negative = bits < 0;
			int exponent = (int)((bits >> 52) & 0x7ffL);
			long mantissa = bits & 0xfffffffffffffL;

			// Subnormal numbers; exponent is effectively one higher,
			// but there's no extra normalization bit in the mantissa
			if (exponent == 0) exponent++;
			else mantissa = mantissa | (1L << 52);

			// Bias the exponent. It's actually biased by 1023, but we're
			// treating the mantissa as m.0 rather than 0.m, so we need
			// to subtract another 52 from it.
			exponent -= 1075;
			if (mantissa == 0) return "0";

			/* Normalize */
			while ((mantissa & 1) == 0)
			{    /*  i.e., Mantissa is even */
				mantissa >>= 1;
				exponent++;
			}

			// Construct a new decimal expansion with the mantissa
			ArbitraryDecimal ad = new ArbitraryDecimal(mantissa);

			// If the exponent is less than 0, we need to repeatedly
			// divide by 2 - which is the equivalent of multiplying
			// by 5 and dividing by 10.
			if (exponent < 0)
			{
				for (int i = 0; i < -exponent; i++)
					ad.MultiplyBy(5);
				ad.Shift(-exponent);
			}
			else
			{
				// Otherwise, we need to repeatedly multiply by 2
				for (int i = 0; i < exponent; i++)
					ad.MultiplyBy(2);
			}

			// Finally, return the string with an appropriate sign
			return negative ? "-" + ad : ad.ToString();
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsEqual(this float thisValue, float value) { return SysMath.Abs(thisValue - value) < float.Epsilon; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsEqual(this double thisValue, double value) { return SysMath.Abs(thisValue - value) < double.Epsilon; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsEqual(this decimal thisValue, decimal value) { return SysMath.Abs(thisValue - value) == decimal.Zero; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static char Previous(this char thisValue, byte maximum) { return (char)Previous((byte)thisValue, maximum); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static sbyte Previous(this sbyte thisValue, sbyte maximum)
		{
			sbyte n = (sbyte)(thisValue % maximum);
			return (sbyte)(thisValue - n);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static byte Previous(this byte thisValue, byte maximum)
		{
			byte n = (byte)(thisValue % maximum);
			return (byte)(thisValue - n);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static short Previous(this short thisValue, short maximum)
		{
			short n = (short)(thisValue % maximum);
			return (short)(thisValue - n);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ushort Previous(this ushort thisValue, ushort maximum)
		{
			ushort n = (ushort)(thisValue % maximum);
			return (ushort)(thisValue - n);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int Previous(this int thisValue, int maximum)
		{
			int n = thisValue % maximum;
			return thisValue - n;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static uint Previous(this uint thisValue, uint maximum)
		{
			uint n = thisValue % maximum;
			return thisValue - n;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static long Previous(this long thisValue, long maximum)
		{
			long n = thisValue % maximum;
			return thisValue - n;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ulong Previous(this ulong thisValue, ulong maximum)
		{
			ulong n = thisValue % maximum;
			return thisValue - n;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static char Next(this char thisValue, byte maximum) { return (char)Next((byte)thisValue, maximum); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static sbyte Next(this sbyte thisValue, sbyte maximum)
		{
			sbyte n = (sbyte)(thisValue % maximum);
			return (sbyte)(thisValue + n);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static byte Next(this byte thisValue, byte maximum)
		{
			byte n = (byte)(thisValue % maximum);
			return (byte)(thisValue + n);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static short Next(this short thisValue, short maximum)
		{
			short n = (short)(thisValue % maximum);
			return (short)(thisValue + n);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ushort Next(this ushort thisValue, ushort maximum)
		{
			ushort n = (ushort)(thisValue % maximum);
			return (ushort)(thisValue + n);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int Next(this int thisValue, int maximum)
		{
			int n = thisValue % maximum;
			return thisValue + n;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static uint Next(this uint thisValue, uint maximum)
		{
			uint n = thisValue % maximum;
			return thisValue + n;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static long Next(this long thisValue, long maximum)
		{
			long n = thisValue % maximum;
			return thisValue + n;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ulong Next(this ulong thisValue, ulong maximum)
		{
			ulong n = thisValue % maximum;
			return thisValue + n;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsEmpty(this Guid thisValue) { return thisValue == Guid.Empty; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty(this Guid? thisValue) { return thisValue == null || thisValue == Guid.Empty; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string ToHexString(this Guid thisValue) { return thisValue.ToString("N").ToUpperInvariant(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string ToHexString(this Guid? thisValue) { return thisValue?.ToString("N").ToUpperInvariant(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this char thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this bool thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue
										? 1
										: 0, (int)toBase)
						.PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this sbyte thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this byte thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this short thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this ushort thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this int thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this uint thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this long thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this ulong thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString((long)thisValue, (int)toBase).PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this float thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString((long)SysMath.Floor(thisValue), (int)toBase).PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this double thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString((long)SysMath.Floor(thisValue), (int)toBase).PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadLeft(this decimal thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString((long)SysMath.Floor(thisValue), (int)toBase).PadLeft(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this char thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this bool thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue
										? 1
										: 0, (int)toBase)
						.PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this sbyte thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this byte thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this short thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this ushort thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this int thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this uint thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this long thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString(thisValue, (int)toBase).PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this ulong thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString((long)thisValue, (int)toBase).PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this float thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString((long)SysMath.Floor(thisValue), (int)toBase).PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this double thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString((long)SysMath.Floor(thisValue), (int)toBase).PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string PadRight(this decimal thisValue, NumericBase toBase, ushort totalWidth, char paddingChar = ' ')
		{
			return Convert.ToString((long)SysMath.Floor(thisValue), (int)toBase).PadRight(totalWidth, paddingChar);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static byte Lo(this short thisValue)
		{
			return (byte)(thisValue & 0xff);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static byte Hi(this short thisValue)
		{
			return (byte)(thisValue >> 8);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static short Lo(this int thisValue)
		{
			return (short)(thisValue & 0xffff);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static short Hi(this int thisValue)
		{
			return (short)(thisValue >> 16);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int Lo(this long thisValue)
		{
			return (int)(thisValue & 0xffffffff);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int Hi(this long thisValue)
		{
			return (int)(thisValue >> 32);
		}

		public static double Rate(this char thisValue)
		{
			return thisValue / (double)char.MaxValue * 100.0d;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static byte Progress(this byte thisValue, byte total)
		{
			return (byte)Progress((double)thisValue, total);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static short Progress(this short thisValue, short total)
		{
			return (short)Progress((double)thisValue, total);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ushort Progress(this ushort thisValue, ushort total)
		{
			return (ushort)Progress((double)thisValue, total);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int Progress(this int thisValue, int total)
		{
			return (int)Progress((double)thisValue, total);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static uint Progress(this uint thisValue, uint total)
		{
			return (uint)Progress((double)thisValue, total);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static long Progress(this long thisValue, long total)
		{
			return (long)Progress((double)thisValue, total);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ulong Progress(this ulong thisValue, ulong total)
		{
			return (ulong)Progress((double)thisValue, total);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static float Progress(this float thisValue, float total)
		{
			return (float)Progress((double)thisValue, total);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Progress(this double thisValue, double total) { return thisValue / total * 100.0d; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static sbyte Multiplier(this sbyte thisValue, sbyte value) { return (sbyte)(value * SysMath.Ceiling(thisValue / (double)value)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static byte Multiplier(this byte thisValue, byte value) { return (byte)(value * SysMath.Ceiling(thisValue / (double)value)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static short Multiplier(this short thisValue, short value) { return (short)(value * SysMath.Ceiling(thisValue / (double)value)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ushort Multiplier(this ushort thisValue, ushort value) { return (ushort)(value * SysMath.Ceiling(thisValue / (double)value)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int Multiplier(this int thisValue, int value) { return (int)(value * SysMath.Ceiling(thisValue / (double)value)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static uint Multiplier(this uint thisValue, uint value) { return (uint)(value * SysMath.Ceiling(thisValue / (double)value)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static long Multiplier(this long thisValue, long value) { return (long)(value * SysMath.Ceiling(thisValue / (double)value)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static ulong Multiplier(this ulong thisValue, ulong value) { return (ulong)(value * SysMath.Ceiling(thisValue / (double)value)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static float Multiplier(this float thisValue, float value) { return (float)(value * SysMath.Ceiling(thisValue / value)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static double Multiplier(this double thisValue, double value) { return value * SysMath.Ceiling(thisValue / value); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static decimal Multiplier(this decimal thisValue, decimal value) { return value * SysMath.Ceiling(thisValue / value); }
	}
}