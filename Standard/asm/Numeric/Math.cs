using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using asm.Extensions;
using JetBrains.Annotations;
using SysMath = System.Math;

namespace asm.Numeric
{
	public static class Math
	{
		/*
		 * used to cache results instead of calculating them every time.
		 * this technique is used in dynamic programming (DP) which caches the
		 * results in something called memoize.
		 * see https://en.wikipedia.org/wiki/Memoization
		 */
		private static readonly Lazy<ISet<ulong>> __primes = new Lazy<ISet<ulong>>(() => new SortedSet<ulong>
		{
			// the first two values are (1) a prime roughly half way between the previous value and int.MaxValue
			// and (2) the prime closest too, but not above, int.MaxValue.
			7,
			11,
			17,
			23,
			29,
			37,
			47,
			53,
			59,
			71,
			89,
			97,
			107,
			131,
			163,
			193,
			197,
			239,
			293,
			353,
			389,
			431,
			521,
			631,
			761,
			769,
			919,
			1103,
			1327,
			1543,
			1597,
			1931,
			2333,
			2801,
			3079,
			3371,
			4049,
			4861,
			5839,
			6151,
			7013,
			8419,
			10103,
			12143,
			12289,
			14591,
			17519,
			21023,
			24593,
			25229,
			30293,
			36353,
			43627,
			49157,
			52361,
			62851,
			75431,
			90523,
			98317,
			108631,
			130363,
			156437,
			187751,
			196613,
			225307,
			270371,
			324449,
			389357,
			393241,
			467237,
			560689,
			672827,
			786433,
			807403,
			968897,
			1162687,
			1395263,
			1572869,
			1674319,
			2009191,
			2411033,
			2893249,
			3145739,
			3471899,
			4166287,
			4999559,
			5999471,
			6291469,
			7199369,
			12582917,
			25165843,
			50331653,
			100663319,
			201326611,
			402653189,
			805306457,
			1610612741,
			1879048201,
			2147483629,
			int.MaxValue
		}, LazyThreadSafetyMode.ExecutionAndPublication);
		
		public static bool IsPrime(sbyte value) { return IsPrime((ulong)value); }
		public static bool IsPrime(byte value) { return IsPrime((ulong)value); }
		public static bool IsPrime(short value) { return IsPrime((ulong)value); }
		public static bool IsPrime(ushort value) { return IsPrime((ulong)value); }
		public static bool IsPrime(int value) { return IsPrime((ulong)value); }
		public static bool IsPrime(uint value) { return IsPrime((ulong)value); }
		public static bool IsPrime(long value) { return IsPrime((ulong)value); }
		public static bool IsPrime(ulong value)
		{
			if ((value & 1ul) == 0 || value < 2ul || value % 2ul == 0ul || value % 3ul == 0ul || value % 5ul == 0ul) return false;
			if (__primes.Value.Contains(value)) return true;

			ulong uMax = (ulong)SysMath.Sqrt(value);

			foreach (ulong prime in __primes.Value)
			{
				if (prime > uMax) break;
				if (value % prime == 0ul) return false;
			}

			for (ulong i = uMax; i * i <= uMax; i++)
			{
				if (value % i == 0ul)
					return false;
			}

			__primes.Value.Add(value);
			return true;
		}

		public static int CountPrimes(short minimum, short maximum) { return (int)CountPrimes((ulong)minimum, (ulong)maximum); }
		public static int CountPrimes(ushort minimum, ushort maximum) { return (int)CountPrimes(minimum, (ulong)maximum); }
		public static int CountPrimes(int minimum, int maximum) { return (int)CountPrimes((ulong)minimum, (ulong)maximum); }
		public static long CountPrimes(uint minimum, uint maximum) { return CountPrimes(minimum, (ulong)maximum); }
		public static long CountPrimes(long minimum, long maximum) { return CountPrimes((ulong)minimum, (ulong)maximum); }
		public static long CountPrimes(ulong minimum, ulong maximum)
		{
			long count = 0;

			foreach (ulong _ in GetPrimes(minimum, maximum)) 
				count++;

			return count;
		}

		public static short ExpandPrime(short value, short skip = 1) { return (short)ExpandPrime((ulong)value, (ulong)skip); }
		public static ushort ExpandPrime(ushort value, ushort skip = 1) { return (ushort)ExpandPrime(value, (ulong)skip); }
		public static int ExpandPrime(int value, int skip = 1) { return (int)ExpandPrime((ulong)value, (ulong)skip); }
		public static uint ExpandPrime(uint value, uint skip = 1u) { return (uint)ExpandPrime(value, (ulong)skip); }
		public static long ExpandPrime(long value, long skip = 1L) { return (long)ExpandPrime((ulong)value, (ulong)skip); }
		public static ulong ExpandPrime(ulong value, ulong skip = 1ul) { return GetPrime(value + skip); }

		public static short GetPrime(short value) { return (short)GetPrime((ulong)value); }
		public static ushort GetPrime(ushort value) { return (ushort)GetPrime((ulong)value); }
		public static int GetPrime(int value) { return (int)GetPrime((ulong)value); }
		public static uint GetPrime(uint value) { return (uint)GetPrime((ulong)value); }
		public static long GetPrime(long value) { return (long)GetPrime((ulong)value); }
		public static ulong GetPrime(ulong value)
		{
			if (value < 2ul) return 2ul;

			ulong prime = __primes.Value.FirstOrDefault(arg => arg >= value);
			if (prime > 0ul) return prime;

			// Outside of our predefined table. Compute the hard way.
			ulong max = (ulong)SysMath.Sqrt(value);

			for (ulong i = value; i <= max; i += 2ul)
			{
				if (!IsPrime(i)) continue;
				return i;
			}

			return value;
		}

		public static IEnumerable<ulong> GetPrimes(ulong maximum, ulong minimum = 1UL)
		{
			if (maximum < 1ul || !minimum.InRange(2ul, maximum)) yield break;

			ulong last = minimum;
			bool anyInList = false;

			foreach (ulong prime in __primes.Value.Where(arg => arg.InRange(minimum, maximum)))
			{
				anyInList = true;
				last = prime;
				yield return last;
			}

			if (last >= maximum) yield break;
			if (anyInList && last % 2ul == 0) last++;

			for (ulong i = last; i <= maximum; i += 2)
			{
				if (!IsPrime(i)) continue;
				yield return i;
			}
		}

		[NotNull]
		public static IEnumerable<ulong> GetPrimeFactors(ulong value) { return GetPrimes(value).Where(prime => value % prime == 0); }

		public static double Ratio(byte value, byte basis) { return value / (double)basis * 100.0d; }

		public static double Ratio(short value, short basis)
		{
			return basis == 0
						? 0.0d
						: SysMath.Abs(value) / (double)SysMath.Abs(basis) * 100.0d;
		}

		public static double Ratio(int value, int basis)
		{
			return basis == 0
						? 0.0d
						: SysMath.Abs(value) / (double)SysMath.Abs(basis) * 100.0d;
		}

		public static double Ratio(long value, long basis)
		{
			return basis == 0
						? 0.0d
						: SysMath.Abs(value) / (double)SysMath.Abs(basis) * 100.0d;
		}

		public static double Ratio(float value, float basis)
		{
			return SysMath.Abs(basis) < float.Epsilon
						? 0.0f
						: SysMath.Abs(value) / SysMath.Abs(basis) * 100.0f;
		}

		public static double Ratio(double value, double basis)
		{
			return SysMath.Abs(basis) < double.Epsilon
						? 0.0d
						: SysMath.Abs(value) / SysMath.Abs(basis) * 100.0d;
		}

		public static decimal Ratio(decimal value, decimal basis)
		{
			return SysMath.Abs(basis) < decimal.Zero
						? 0.0m
						: SysMath.Abs(value) / SysMath.Abs(basis) * 100.0m;
		}

		public static byte RatioValue(byte value, byte basis) { return (byte)(value / 100.0d * basis); }

		public static short RatioValue(short value, short basis)
		{
			return (short)(basis == 0
								? 0
								: SysMath.Abs(value) / 100.0d * SysMath.Abs(basis));
		}

		public static int RatioValue(int value, int basis)
		{
			return (int)(basis == 0
							? 0
							: SysMath.Abs(value) / 100.0d * SysMath.Abs(basis));
		}

		public static long RatioValue(long value, long basis)
		{
			return (long)(basis == 0
							? 0
							: SysMath.Abs(value) / 100.0d * SysMath.Abs(basis));
		}

		public static (byte x, byte y) AspectRatio(byte x, byte y, byte value, bool resizeToX = true)
		{
			if (x == 0) throw new ArgumentOutOfRangeException(nameof(x));
			if (y == 0) throw new ArgumentOutOfRangeException(nameof(y));
			if (value == 0) throw new ArgumentOutOfRangeException(nameof(value));

			if (resizeToX)
			{
				double ry = (double)value / x;
				return (value, (byte)(y * ry));
			}

			double rx = (double)value / y;
			return ((byte)(x * rx), value);
		}
		public static (short x, short y) AspectRatio(short x, short y, short value, bool resizeToX = true)
		{
			if (x == 0) throw new ArgumentOutOfRangeException(nameof(x));
			if (y == 0) throw new ArgumentOutOfRangeException(nameof(y));
			if (value == 0) throw new ArgumentOutOfRangeException(nameof(value));

			if (resizeToX)
			{
				double ry = (double)value / x;
				return (value, (short)(y * ry));
			}

			double rx = (double)value / y;
			return ((short)(x * rx), value);
		}
		public static (ushort x, ushort y) AspectRatio(ushort x, ushort y, ushort value, bool resizeToX = true)
		{
			if (x == 0) throw new ArgumentOutOfRangeException(nameof(x));
			if (y == 0) throw new ArgumentOutOfRangeException(nameof(y));
			if (value == 0) throw new ArgumentOutOfRangeException(nameof(value));

			if (resizeToX)
			{
				double ry = (double)value / x;
				return (value, (ushort)(y * ry));
			}

			double rx = (double)value / y;
			return ((ushort)(x * rx), value);
		}
		public static (int x, int y) AspectRatio(int x, int y, int value, bool resizeToX = true)
		{
			if (x == 0) throw new ArgumentOutOfRangeException(nameof(x));
			if (y == 0) throw new ArgumentOutOfRangeException(nameof(y));
			if (value == 0) throw new ArgumentOutOfRangeException(nameof(value));

			if (resizeToX)
			{
				double ry = (double)value / x;
				return (value, (int)(y * ry));
			}

			double rx = (double)value / y;
			return ((int)(x * rx), value);
		}
		public static (uint x, uint y) AspectRatio(uint x, uint y, uint value, bool resizeToX = true)
		{
			if (x == 0) throw new ArgumentOutOfRangeException(nameof(x));
			if (y == 0) throw new ArgumentOutOfRangeException(nameof(y));
			if (value == 0) throw new ArgumentOutOfRangeException(nameof(value));

			if (resizeToX)
			{
				double ry = (double)value / x;
				return (value, (uint)(y * ry));
			}

			double rx = (double)value / y;
			return ((uint)(x * rx), value);
		}
		public static (long x, long y) AspectRatio(long x, long y, long value, bool resizeToX = true)
		{
			if (x == 0) throw new ArgumentOutOfRangeException(nameof(x));
			if (y == 0) throw new ArgumentOutOfRangeException(nameof(y));
			if (value == 0) throw new ArgumentOutOfRangeException(nameof(value));

			if (resizeToX)
			{
				double ry = (double)value / x;
				return (value, (long)(y * ry));
			}

			double rx = (double)value / y;
			return ((long)(x * rx), value);
		}
		public static (ulong x, ulong y) AspectRatio(ulong x, ulong y, ulong value, bool resizeToX = true)
		{
			if (x == 0) throw new ArgumentOutOfRangeException(nameof(x));
			if (y == 0) throw new ArgumentOutOfRangeException(nameof(y));
			if (value == 0) throw new ArgumentOutOfRangeException(nameof(value));

			if (resizeToX)
			{
				double ry = (double)value / x;
				return (value, (ulong)(y * ry));
			}

			double rx = (double)value / y;
			return ((ulong)(x * rx), value);
		}

		/// <summary>
		/// Greatest Common Divisor for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>byte</returns>
		public static byte GCD(byte a, byte b)
		{
			while (a != 0 && b != 0)
			{
				byte a1 = a;
				a = SysMath.Min(a, b);
				b = (byte)(SysMath.Max(a1, b) % SysMath.Min(a1, b));
			}

			return (byte)(a | b);
		}
		/// <summary>
		/// Greatest Common Divisor for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>short</returns>
		public static short GCD(short a, short b)
		{
			while (a != 0 && b != 0)
			{
				short a1 = a;
				a = SysMath.Min(a, b);
				b = (short)(SysMath.Max(a1, b) % SysMath.Min(a1, b));
			}

			return (short)(a | b);
		}
		/// <summary>
		/// Greatest Common Divisor for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>ushort</returns>
		public static ushort GCD(ushort a, ushort b)
		{
			while (a != 0 && b != 0)
			{
				ushort a1 = a;
				a = SysMath.Min(a, b);
				b = (ushort)(SysMath.Max(a1, b) % SysMath.Min(a1, b));
			}

			return (ushort)(a | b);
		}
		/// <summary>
		/// Greatest Common Divisor for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>int</returns>
		public static int GCD(int a, int b)
		{
			while (a != 0 && b != 0)
			{
				int a1 = a;
				a = SysMath.Min(a, b);
				b = SysMath.Max(a1, b) % SysMath.Min(a1, b);
			}

			return a | b;
		}
		/// <summary>
		/// Greatest Common Divisor for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>uint</returns>
		public static uint GCD(uint a, uint b)
		{
			while (a != 0u && b != 0u)
			{
				uint a1 = a;
				a = SysMath.Min(a, b);
				b = SysMath.Max(a1, b) % SysMath.Min(a1, b);
			}

			return a | b;
		}
		/// <summary>
		/// Greatest Common Divisor for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>long</returns>
		public static long GCD(long a, long b)
		{
			while (a != 0 && b != 0)
			{
				long a1 = a;
				a = SysMath.Min(a, b);
				b = SysMath.Max(a1, b) % SysMath.Min(a1, b);
			}

			return a | b;
		}
		/// <summary>
		/// Greatest Common Divisor for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>ulong</returns>
		public static ulong GCD(ulong a, ulong b)
		{
			while (a != 0ul && b != 0ul)
			{
				ulong a1 = a;
				a = SysMath.Min(a, b);
				b = SysMath.Max(a1, b) % SysMath.Min(a1, b);
			}

			return a | b;
		}

		/// <summary>
		/// Least Common Multiple for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>byte</returns>
		public static byte LCM(byte a, byte b)
		{
			if (a == 0 || b == 0) return 0;
			if (a == 1 || b == 1) return 1;

			byte gcd = GCD(a, b);
			return gcd == 0
						? (byte)0
						: (byte)(a / gcd * b);
		}
		/// <summary>
		/// Least Common Multiple for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>short</returns>
		public static short LCM(short a, short b)
		{
			if (a == 0 || b == 0) return 0;
			if (a == 1 || b == 1) return 1;

			short gcd = GCD(a, b);
			return gcd == 0
						? (short)0
						: (short)(a / gcd * b);
		}
		/// <summary>
		/// Least Common Multiple for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>ushort</returns>
		public static ushort LCM(ushort a, ushort b)
		{
			if (a == 0 || b == 0) return 0;
			if (a == 1 || b == 1) return 1;

			ushort gcd = GCD(a, b);
			return gcd == 0
						? (ushort)0
						: (ushort)(a / gcd * b);
		}
		/// <summary>
		/// Least Common Multiple for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>int</returns>
		public static int LCM(int a, int b)
		{
			if (a == 0 || b == 0) return 0;
			if (a == 1 || b == 1) return 1;

			int gcd = GCD(a, b);
			return gcd == 0
						? 0
						: a / gcd * b;
		}
		/// <summary>
		/// Least Common Multiple for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>uint</returns>
		public static uint LCM(uint a, uint b)
		{
			if (a == 0 || b == 0) return 0;
			if (a == 1 || b == 1) return 1;

			uint gcd = GCD(a, b);
			return gcd == 0
						? 0
						: a / gcd * b;
		}
		/// <summary>
		/// Least Common Multiple for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>long</returns>
		public static long LCM(long a, long b)
		{
			if (a == 0 || b == 0) return 0;
			if (a == 1 || b == 1) return 1;

			long gcd = GCD(a, b);
			return gcd == 0
						? 0
						: a / gcd * b;
		}
		/// <summary>
		/// Least Common Multiple for a and b.
		/// </summary>
		/// <param name="a">a</param>
		/// <param name="b">b</param>
		/// <returns>ulong</returns>
		public static ulong LCM(ulong a, ulong b)
		{
			if (a == 0 || b == 0) return 0;
			if (a == 1 || b == 1) return 1;

			ulong gcd = GCD(a, b);
			return gcd == 0
						? 0
						: a / gcd * b;
		}

		public static Fraction<int> RationalApproximation(float value, float accuracy = (float)0.001)
		{
			if (accuracy <= (float)0.0 || accuracy >= (float)1.0) throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than 0 and less than 1.");

			int sign = SysMath.Sign(value);
			if (sign == -1) value = SysMath.Abs(value);

			// Accuracy is the maximum relative error; convert to absolute maxError
			float maxError = sign == 0
								? accuracy
								: value * accuracy;

			int n = (int)SysMath.Floor(value);
			value -= n;
			if (value < maxError) return new Fraction<int>(sign * n, 1);
			if (1 - maxError < value) return new Fraction<int>(sign * (n + 1), 1);

			float z = value;
			int previousDenominator = 0;
			int denominator = 1;
			int numerator;

			do
			{
				z = (float)1.0 / (z - (int)z);
				int temp = denominator;
				denominator = denominator * (int)z + previousDenominator;
				previousDenominator = temp;
				numerator = Convert.ToInt32(value * denominator);
			}
			while (SysMath.Abs(value - (float)numerator / denominator) > maxError && !z.Equals((int)z));

			return new Fraction<int>((n * denominator + numerator) * sign, denominator);
		}
		/// <summary>
		/// Returns two numbers that represent best approximation of a real number in fractions.
		/// Farey Sequence: https://www.johndcook.com/blog/2010/10/20/best-rational-approximation/
		/// Continued fraction: https://en.wikipedia.org/wiki/Continued_fraction
		/// But eventually the best algorithm wins, because it's the fastest and more accurate
		/// https://stackoverflow.com/questions/5124743/algorithm-for-simplifying-decimal-to-fractions/42085412#42085412
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="accuracy"> to specify the max. relative error, not the max. absolute error; 0.01 would find a fraction within 1% of the value.</param>
		/// <returns></returns>
		public static Fraction<int> RationalApproximation(double value, double accuracy = 0.001)
		{
			if (accuracy <= 0.0 || accuracy >= 1.0) throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than 0 and less than 1.");

			int sign = SysMath.Sign(value);
			if (sign == -1) value = SysMath.Abs(value);

			// Accuracy is the maximum relative error; convert to absolute maxError
			double maxError = sign == 0
								? accuracy
								: value * accuracy;

			int n = (int)SysMath.Floor(value);
			value -= n;
			if (value < maxError) return new Fraction<int>(sign * n, 1);
			if (1 - maxError < value) return new Fraction<int>(sign * (n + 1), 1);

			double z = value;
			int previousDenominator = 0;
			int denominator = 1;
			int numerator;

			do
			{
				z = 1.0 / (z - (int)z);
				int temp = denominator;
				denominator = denominator * (int)z + previousDenominator;
				previousDenominator = temp;
				numerator = Convert.ToInt32(value * denominator);
			}
			while (SysMath.Abs(value - (double)numerator / denominator) > maxError && !z.Equals((int)z));

			return new Fraction<int>((n * denominator + numerator) * sign, denominator);
		}
		/// <summary>
		/// Returns two numbers that represent best approximation of a real number in fractions.
		/// Farey Sequence: https://www.johndcook.com/blog/2010/10/20/best-rational-approximation/
		/// Continued fraction: https://en.wikipedia.org/wiki/Continued_fraction
		/// But eventually the best algorithm wins, because it's the fastest and more accurate
		/// https://stackoverflow.com/questions/5124743/algorithm-for-simplifying-decimal-to-fractions/42085412#42085412
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="accuracy"> to specify the max. relative error, not the max. absolute error; 0.01 would find a fraction within 1% of the value.</param>
		/// <returns></returns>
		public static Fraction<int> RationalApproximation(decimal value, decimal accuracy = (decimal)0.001)
		{
			if (accuracy <= (decimal)0.0 || accuracy >= (decimal)1.0) throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than 0 and less than 1.");

			int sign = SysMath.Sign(value);
			if (sign == -1) value = SysMath.Abs(value);

			// Accuracy is the maximum relative error; convert to absolute maxError
			decimal maxError = sign == 0
									? accuracy
									: value * accuracy;

			int n = (int)SysMath.Floor(value);
			value -= n;
			if (value < maxError) return new Fraction<int>(sign * n, 1);
			if (1 - maxError < value) return new Fraction<int>(sign * (n + 1), 1);

			decimal z = value;
			int previousDenominator = 0;
			int denominator = 1;
			int numerator;

			do
			{
				z = (decimal)1.0 / (z - (int)z);
				int temp = denominator;
				denominator = denominator * (int)z + previousDenominator;
				previousDenominator = temp;
				numerator = Convert.ToInt32(value * denominator);
			}
			while (SysMath.Abs(value - (decimal)numerator / denominator) > maxError && !z.Equals((int)z));

			return new Fraction<int>((n * denominator + numerator) * sign, denominator);
		}
		/// <summary>
		/// Returns two numbers that represent best approximation of a real number in fractions.
		/// Farey Sequence: https://www.johndcook.com/blog/2010/10/20/best-rational-approximation/
		/// Continued fraction: https://en.wikipedia.org/wiki/Continued_fraction
		/// Stern-Brocot tree http://en.wikipedia.org/wiki/Stern%E2%80%93Brocot_tree
		/// https://stackoverflow.com/questions/5124743/algorithm-for-simplifying-decimal-to-fractions/42085412#42085412
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="accuracy"> to specify the max. relative error, not the max. absolute error; 0.01 would find a fraction within 1% of the value.</param>
		/// <returns></returns>
		public static Fraction<int> RationalApproximationSternBrocot(float value, float accuracy = (float)0.001)
		{
			if (accuracy <= (float)0.0 || accuracy >= (float)1.0) throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than 0 and less than 1.");

			int sign = SysMath.Sign(value);
			if (sign == -1) value = SysMath.Abs(value);

			// Accuracy is the maximum relative error; convert to absolute maxError
			float maxError = sign == 0
								? accuracy
								: value * accuracy;

			int n = (int)SysMath.Floor(value);
			value -= n;
			if (value < maxError) return new Fraction<int>(sign * n, 1);
			if (1 - maxError < value) return new Fraction<int>(sign * (n + 1), 1);

			// The lower fraction is 0/1
			int lowerN = 0;
			int lowerD = 1;

			// The upper fraction is 1/1
			int upperN = 1;
			int upperD = 1;

			while (true)
			{
				// The middle fraction is (lower_n + upper_n) / (lower_d + upper_d)
				int middleN = lowerN + upperN;
				int middleD = lowerD + upperD;
				int lwrd = lowerD;
				int uprd = upperD;
				int lwrn = lowerN;
				int uprn = upperN;

				if (middleD * (value + maxError) < middleN)
				{
					// real + error < middle : middle is our new upper
					Seek(ref upperN, ref upperD, lowerN, lowerD, (un, ud) => (lwrd + ud) * (value + maxError) < lwrn + un);
				}
				else if (middleN < (value - maxError) * middleD)
				{
					// middle < real - error : middle is our new lower
					Seek(ref lowerN, ref lowerD, upperN, upperD, (ln, ld) => ln + uprn < (value - maxError) * (ld + uprd));
				}
				else
				{
					// Middle is our best fraction
					return new Fraction<int>((n * middleD + middleN) * sign, middleD);
				}
			}
		}
		/// <summary>
		/// Returns two numbers that represent best approximation of a real number in fractions.
		/// Farey Sequence: https://www.johndcook.com/blog/2010/10/20/best-rational-approximation/
		/// Continued fraction: https://en.wikipedia.org/wiki/Continued_fraction
		/// Stern-Brocot tree http://en.wikipedia.org/wiki/Stern%E2%80%93Brocot_tree
		/// https://stackoverflow.com/questions/5124743/algorithm-for-simplifying-decimal-to-fractions/42085412#42085412
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="accuracy"> to specify the max. relative error, not the max. absolute error; 0.01 would find a fraction within 1% of the value.</param>
		/// <returns></returns>
		public static Fraction<int> RationalApproximationSternBrocot(double value, double accuracy = 0.001)
		{
			if (accuracy <= 0.0 || accuracy >= 1.0) throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than 0 and less than 1.");

			int sign = SysMath.Sign(value);
			if (sign == -1) value = SysMath.Abs(value);

			// Accuracy is the maximum relative error; convert to absolute maxError
			double maxError = sign == 0
								? accuracy
								: value * accuracy;

			int n = (int)SysMath.Floor(value);
			value -= n;
			if (value < maxError) return new Fraction<int>(sign * n, 1);
			if (1 - maxError < value) return new Fraction<int>(sign * (n + 1), 1);

			// The lower fraction is 0/1
			int lowerN = 0;
			int lowerD = 1;

			// The upper fraction is 1/1
			int upperN = 1;
			int upperD = 1;

			while (true)
			{
				// The middle fraction is (lower_n + upper_n) / (lower_d + upper_d)
				int middleN = lowerN + upperN;
				int middleD = lowerD + upperD;
				int lwrd = lowerD;
				int uprd = upperD;
				int lwrn = lowerN;
				int uprn = upperN;

				if (middleD * (value + maxError) < middleN)
				{
					// real + error < middle : middle is our new upper
					Seek(ref upperN, ref upperD, lowerN, lowerD, (un, ud) => (lwrd + ud) * (value + maxError) < lwrn + un);
				}
				else if (middleN < (value - maxError) * middleD)
				{
					// middle < real - error : middle is our new lower
					Seek(ref lowerN, ref lowerD, upperN, upperD, (ln, ld) => ln + uprn < (value - maxError) * (ld + uprd));
				}
				else
				{
					// Middle is our best fraction
					return new Fraction<int>((n * middleD + middleN) * sign, middleD);
				}
			}
		}
		/// <summary>
		/// Returns two numbers that represent best approximation of a real number in fractions.
		/// Farey Sequence: https://www.johndcook.com/blog/2010/10/20/best-rational-approximation/
		/// Continued fraction: https://en.wikipedia.org/wiki/Continued_fraction
		/// Stern-Brocot tree http://en.wikipedia.org/wiki/Stern%E2%80%93Brocot_tree
		/// https://stackoverflow.com/questions/5124743/algorithm-for-simplifying-decimal-to-fractions/42085412#42085412
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="accuracy"> to specify the max. relative error, not the max. absolute error; 0.01 would find a fraction within 1% of the value.</param>
		/// <returns></returns>
		public static Fraction<int> RationalApproximationSternBrocot(decimal value, decimal accuracy = (decimal)0.001)
		{
			if (accuracy <= (decimal)0.0 || accuracy >= (decimal)1.0) throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than 0 and less than 1.");

			int sign = SysMath.Sign(value);
			if (sign == -1) value = SysMath.Abs(value);

			// Accuracy is the maximum relative error; convert to absolute maxError
			decimal maxError = sign == 0
									? accuracy
									: value * accuracy;

			int n = (int)SysMath.Floor(value);
			value -= n;
			if (value < maxError) return new Fraction<int>(sign * n, 1);
			if (1 - maxError < value) return new Fraction<int>(sign * (n + 1), 1);

			// The lower fraction is 0/1
			int lowerN = 0;
			int lowerD = 1;

			// The upper fraction is 1/1
			int upperN = 1;
			int upperD = 1;

			while (true)
			{
				// The middle fraction is (lower_n + upper_n) / (lower_d + upper_d)
				int middleN = lowerN + upperN;
				int middleD = lowerD + upperD;
				int lwrd = lowerD;
				int uprd = upperD;
				int lwrn = lowerN;
				int uprn = upperN;

				if (middleD * (value + maxError) < middleN)
				{
					// real + error < middle : middle is our new upper
					Seek(ref upperN, ref upperD, lowerN, lowerD, (un, ud) => (lwrd + ud) * (value + maxError) < lwrn + un);
				}
				else if (middleN < (value - maxError) * middleD)
				{
					// middle < real - error : middle is our new lower
					Seek(ref lowerN, ref lowerD, upperN, upperD, (ln, ld) => ln + uprn < (value - maxError) * (ld + uprd));
				}
				else
				{
					// Middle is our best fraction
					return new Fraction<int>((n * middleD + middleN) * sign, middleD);
				}
			}
		}

		/// <summary>
		/// Returns two numbers that represent best approximation of a real number in fractions.
		/// Continued fraction: https://en.wikipedia.org/wiki/Continued_fraction
		/// https://rosettacode.org/wiki/Convert_decimal_number_to_rational#C.23
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="maximumDenominator"> to specify the max. relative error, not the max. absolute error; 0.01 would find a fraction within 1% of the value.</param>
		/// <returns></returns>
		public static Fraction<long> RationalApproximationRosetta(double value, long maximumDenominator = 4096)
		{
			/* Translated from the C version. */
			/*  a: continued fraction coefficients. */
			long[] h =
			{
				0,
				1,
				0
			};
			long[] k =
			{
				1,
				0,
				0
			};
			long n = 1;
			int i, neg = 0;
			long numerator;
			long denominator;

			if (maximumDenominator <= 1)
			{
				denominator = 1;
				numerator = (long)value;
				return new Fraction<long>(numerator, denominator);
			}

			if (value < 0)
			{
				neg = 1;
				value = -value;
			}

			while (!value.Equals(SysMath.Floor(value)))
			{
				n <<= 1;
				value *= 2;
			}

			long d = (long)value;

			/* continued fraction and check denominator each step */
			for (i = 0; i < 64; i++)
			{
				long a = n != 0
							? d / n
							: 0;
				if (i != 0 && a == 0) break;

				long x = d;
				d = n;
				n = x % n;

				x = a;
				if (k[1] * a + k[0] >= maximumDenominator)
				{
					x = (maximumDenominator - k[0]) / k[1];
					if (x * 2 >= a || k[1] >= maximumDenominator) i = 65;
					else break;
				}

				h[2] = x * h[1] + h[0];
				h[0] = h[1];
				h[1] = h[2];
				k[2] = x * k[1] + k[0];
				k[0] = k[1];
				k[1] = k[2];
			}

			denominator = k[1];
			numerator = neg != 0
							? -h[1]
							: h[1];
			return new Fraction<long>(numerator, denominator);
		}

		public static sbyte Factorial(sbyte value)
		{
			sbyte result = 1;

			for (sbyte i = 1; i <= value; i++)
			{
				result *= i;
			}

			return result;
		}
		public static byte Factorial(byte value)
		{
			byte result = 1;

			for (byte i = 1; i <= value; i++)
			{
				result *= i;
			}

			return result;
		}
		public static short Factorial(short value)
		{
			short result = 1;

			for (short i = 1; i <= value; i++)
			{
				result *= i;
			}

			return result;
		}
		public static ushort Factorial(ushort value)
		{
			ushort result = 1;

			for (ushort i = 1; i <= value; i++)
			{
				result *= i;
			}

			return result;
		}
		public static int Factorial(int value)
		{
			int result = 1;

			for (int i = 1; i <= value; i++)
			{
				result *= i;
			}

			return result;
		}
		public static uint Factorial(uint value)
		{
			uint result = 1;

			for (uint i = 1; i <= value; i++)
			{
				result *= i;
			}

			return result;
		}
		public static long Factorial(long value)
		{
			long result = 1;

			for (long i = 1; i <= value; i++)
			{
				result *= i;
			}

			return result;
		}
		public static ulong Factorial(ulong value)
		{
			ulong result = 1;

			for (ulong i = 1; i <= value; i++) 
				result *= i;

			return result;
		}

		public static sbyte Fibonacci(sbyte value) { return (sbyte)Fibonacci((uint)value); }
		public static byte Fibonacci(byte value) { return (byte)Fibonacci((uint)value); }
		public static short Fibonacci(short value) { return (short)Fibonacci((uint)value); }
		public static ushort Fibonacci(ushort value) { return (ushort)Fibonacci((uint)value); }
		public static int Fibonacci(int value) { return (int)Fibonacci((uint)value); }
		public static uint Fibonacci(uint value)
		{
			// https://ocw.mit.edu/courses/electrical-engineering-and-computer-science/6-006-introduction-to-algorithms-fall-2011/
			// AlgoExpert - Become An Expert In Algorithms
			// fibonacci(n) = fibonacci(n - 1) + fibonacci(n - 2) where n >= 0
			if (value <= 2u) return 1u;

			// this can be done using Dynamic Programming as well by caching every calculated fib(n), fib(n - 1), fib(n - 2)
			// int/unit will run faster than ulong version on 64 bit systems and uint in particular will be faster than smaller data types because of the alignment.
			uint[] fib = { 0u, 1u };
			uint result = 0u;
			uint i = 3u;

			while (i <= value)
			{
				result = fib[0] + fib[1];
				fib[0] = fib[1];
				fib[1] = result;
				i++;
			}

			return result;
		}
		public static long Fibonacci(long value) { return (long)Fibonacci((ulong)value); }
		public static ulong Fibonacci(ulong value)
		{
			// https://ocw.mit.edu/courses/electrical-engineering-and-computer-science/6-006-introduction-to-algorithms-fall-2011/
			// AlgoExpert - Become An Expert In Algorithms
			// fibonacci(n) = fibonacci(n - 1) + fibonacci(n - 2) where n >= 0
			if (value <= 2ul) return 1ul;

			// this can be done using Dynamic Programming as well by caching every calculated fib(n), fib(n - 1), fib(n - 2)
			ulong[] fib = { 0ul, 1ul };
			ulong result = 0ul;
			ulong i = 3ul;

			while (i <= value)
			{
				result = fib[0] + fib[1];
				fib[0] = fib[1];
				fib[1] = result;
				i++;
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Min<T>(T value1, T value2)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			if (value1.CompareTo(value2) > 0) value1 = value2;
			return value1;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Min<T>(T value1, T value2, T value3)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			if (value1.CompareTo(value2) > 0) value1 = value2;
			if (value1.CompareTo(value3) > 0) value1 = value3;
			return value1;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Min<T>(T value1, T value2, T value3, T value4)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			if (value1.CompareTo(value2) > 0) value1 = value2;
			if (value1.CompareTo(value3) > 0) value1 = value3;
			if (value1.CompareTo(value4) > 0) value1 = value4;
			return value1;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Min<T>(T value1, T value2, T value3, T value4, T value5)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			if (value1.CompareTo(value2) > 0) value1 = value2;
			if (value1.CompareTo(value3) > 0) value1 = value3;
			if (value1.CompareTo(value4) > 0) value1 = value4;
			if (value1.CompareTo(value5) > 0) value1 = value5;
			return value1;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Max<T>(T value1, T value2)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			if (value1.CompareTo(value2) < 0) value1 = value2;
			return value1;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Max<T>(T value1, T value2, T value3)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			if (value1.CompareTo(value2) < 0) value1 = value2;
			if (value1.CompareTo(value3) < 0) value1 = value3;
			return value1;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Max<T>(T value1, T value2, T value3, T value4)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			if (value1.CompareTo(value2) < 0) value1 = value2;
			if (value1.CompareTo(value3) < 0) value1 = value3;
			if (value1.CompareTo(value4) < 0) value1 = value4;
			return value1;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Max<T>(T value1, T value2, T value3, T value4, T value5)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			if (value1.CompareTo(value2) < 0) value1 = value2;
			if (value1.CompareTo(value3) < 0) value1 = value3;
			if (value1.CompareTo(value4) < 0) value1 = value4;
			if (value1.CompareTo(value5) < 0) value1 = value5;
			return value1;
		}

		/// <summary>
		/// Binary seek for the value where f() becomes false.
		/// </summary>
		private static void Seek(ref int a, ref int b, int ainc, int binc, [NotNull] Func<int, int, bool> f)
		{
			a += ainc;
			b += binc;

			if (f(a, b))
			{
				int weight = 1;

				do
				{
					weight *= 2;
					a += ainc * weight;
					b += binc * weight;
				}
				while (f(a, b));

				do
				{
					weight /= 2;

					int adec = ainc * weight;
					int bdec = binc * weight;

					if (!f(a - adec, b - bdec))
					{
						a -= adec;
						b -= bdec;
					}
				}
				while (weight > 1);
			}
		}
	}
}
