using System;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class RandomHelper
	{
		public static Random Default { get; } = new Random(RNGRandomHelper.Next());

		/// <summary>
		/// Returns a nonnegative random number. 
		/// </summary>		
		/// <returns>A 32-bit signed integer greater than or equal to zero and less than Int32.MaxValue.</returns>
		public static int Next()
		{
			return Default.Next();
		}

		/// <summary>
		/// Returns a nonnegative random number less than the specified maximum. 
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer greater than or equal to zero, and less than maxValue; 
		/// that is, the range of return values includes zero but not maxValue.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">maxValue is less than zero.</exception>
		public static int Next(int max)
		{
			return Default.Next(max);
		}

		/// <summary>
		/// Returns a random number within a specified range. 
		/// </summary>
		/// <param name="min">The inclusive lower bound of the random number returned. </param>
		/// <param name="max">
		/// The exclusive upper bound of the random number returned. 
		/// maxValue must be greater than or equal to minValue.
		/// </param>
		/// <returns>
		/// A 32-bit signed integer greater than or equal to minValue and less than maxValue;
		/// that is, the range of return values includes minValue but not maxValue.
		/// If minValue equals maxValue, minValue is returned.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">minValue is greater than maxValue.</exception>
		public static int Next(int min, int max)
		{
			return Default.Next(min, max);
		}

		/// <summary>
		/// Returns a random number between 0.0 and 1.0.
		/// </summary>
		/// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
		public static double NextDouble()
		{
			return Default.NextDouble();
		}

		/// <summary>
		/// Fills the elements of a specified array of bytes with random numbers.
		/// </summary>
		/// <param name="buffer">An array of bytes to contain random numbers.</param>
		/// <exception cref="ArgumentNullException">buffer is a null reference (Nothing in Visual Basic).</exception>
		public static void NextBytes([NotNull] byte[] buffer)
		{
			Default.NextBytes(buffer);
		}

		/// <summary>
		/// Fills the elements of a specified array of bytes with random non-zero numbers.
		/// </summary>
		/// <param name="buffer">An array of bytes to contain random numbers.</param>
		/// <exception cref="ArgumentNullException">buffer is a null reference (Nothing in Visual Basic).</exception>
		public static void NextNonZeroBytes([NotNull] byte[] buffer)
		{
			if (buffer.Length == 0) return;

			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = (byte)Default.Next(1, byte.MaxValue);
		}
	}
}