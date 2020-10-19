using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Other.MarcGravell;

namespace asm.Collections
{
	/// <inheritdoc cref="IEnumerable{T}" />
	/// <summary>
	/// Represents a range of values. An IComparer{T} is used to compare specific
	/// values with a start and end point. A range may be include or exclude each end
	/// individually.
	/// A range which is half-open but has the same start and end point is deemed to be empty,
	/// e.g. [3,3) doesn't include 3. To create a range with a single value, use an inclusive
	/// range, e.g. [3,3].
	/// Ranges are always immutable - calls such as IncludeEnd() and ExcludeEnd() return a new
	/// range without modifying this one.
	/// </summary>
	[Serializable]
	[TypeConverter(typeof(DisplayNameExpandableObjectConverter))]
	public class LambdaRange<T> : IEnumerable<T>, IEnumerable
		where T : struct, IComparable
	{
		/// <inheritdoc />
		/// <summary>
		/// Constructs a new inclusive range using the default comparer
		/// </summary>
		public LambdaRange([NotNull] IReadOnlyRange<T> range)
			: this(range.Minimum, range.Maximum, Comparer<T>.Default, true, true)
		{
		}

		public LambdaRange([NotNull] LambdaRange<T> range)
			: this(range.Minimum, range.Maximum, Comparer<T>.Default, true, true)
		{
		}

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new inclusive range using the default comparer
		/// </summary>
		public LambdaRange(T minimum, T maximum)
			: this(minimum, maximum, Comparer<T>.Default, true, true)
		{
		}

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new range including both ends using the specified comparer
		/// </summary>
		public LambdaRange(T minimum, T maximum, [NotNull] IComparer<T> comparer)
			: this(minimum, maximum, comparer, true, true)
		{
		}

		/// <summary>
		/// Constructs a new range, including or excluding each end as specified,
		/// with the given comparer.
		/// </summary>
		public LambdaRange(T minimum, T maximum, [NotNull] IComparer<T> comparer, bool includeStart, bool includeEnd)
		{
			if (comparer.Compare(minimum, maximum) > 0) throw new ArgumentOutOfRangeException(nameof(maximum), "Minimum must be lower than maximum.");
			Minimum = minimum;
			Maximum = maximum;
			Comparer = comparer;
			IncludesStart = includeStart;
			IncludesEnd = includeEnd;
		}

		/// <summary>
		/// The start of the range.
		/// </summary>
		public T Minimum { get; }

		/// <summary>
		/// The end of the range.
		/// </summary>
		public T Maximum { get; }

		/// <summary>
		/// Comparer to use for comparisons
		/// </summary>
		public IComparer<T> Comparer { get; }

		/// <summary>
		/// Whether or not this range includes the start point
		/// </summary>
		public bool IncludesStart { get; }

		/// <summary>
		/// Whether or not this range includes the end point
		/// </summary>
		public bool IncludesEnd { get; }

		/// <summary>
		/// Returns a range with the same boundaries as this, but excluding the end point.
		/// When called on a range already excluding the end point, the original range is returned.
		/// </summary>
		[NotNull]
		public LambdaRange<T> ExcludeEnd()
		{
			return !IncludesEnd
						? this
						: new LambdaRange<T>(Minimum, Maximum, Comparer, IncludesStart, false);
		}

		/// <summary>
		/// Returns a range with the same boundaries as this, but excluding the start point.
		/// When called on a range already excluding the start point, the original range is returned.
		/// </summary>
		[NotNull]
		public LambdaRange<T> ExcludeStart()
		{
			return !IncludesStart
						? this
						: new LambdaRange<T>(Minimum, Maximum, Comparer, false, IncludesEnd);
		}

		/// <summary>
		/// Returns a range with the same boundaries as this, but including the end point.
		/// When called on a range already including the end point, the original range is returned.
		/// </summary>
		[NotNull]
		public LambdaRange<T> IncludeEnd()
		{
			return IncludesEnd
						? this
						: new LambdaRange<T>(Minimum, Maximum, Comparer, IncludesStart, true);
		}

		/// <summary>
		/// Returns a range with the same boundaries as this, but including the start point.
		/// When called on a range already including the start point, the original range is returned.
		/// </summary>
		[NotNull]
		public LambdaRange<T> IncludeStart()
		{
			return IncludesStart
						? this
						: new LambdaRange<T>(Minimum, Maximum, Comparer, true, IncludesEnd);
		}

		/// <summary>
		/// Returns whether or not the range contains the given value
		/// </summary>
		public bool Contains(T value)
		{
			int lowerBound = Comparer.Compare(value, Minimum);
			if (lowerBound < 0 || lowerBound == 0 && !IncludesStart) return false;

			int upperBound = Comparer.Compare(value, Maximum);
			return upperBound < 0 || upperBound == 0 && IncludesEnd;
		}

		public IEnumerator<T> GetEnumerator() { return new LambdaRangeEnumerator<T>(this, Operator<T>.Increment); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <summary>
		/// Returns an iterator which begins at the start of this range,
		/// applying the given step delegate on each iteration until the
		/// end is reached or passed. The start and end points are included
		/// or excluded according to this range.
		/// </summary>
		/// <param name="step">Delegate to apply to the "current value" on each iteration</param>
		[NotNull]
		public LambdaRangeEnumerator<T> FromStart([NotNull] Func<T, T> step) { return new LambdaRangeEnumerator<T>(this, step); }

		/// <summary>
		/// Returns an iterator which begins at the end of this range,
		/// applying the given step delegate on each iteration until the
		/// start is reached or passed. The start and end points are included
		/// or excluded according to this range.
		/// </summary>
		/// <param name="step">Delegate to apply to the "current value" on each iteration</param>
		[NotNull]
		public LambdaRangeEnumerator<T> FromEnd([NotNull] Func<T, T> step) { return new LambdaRangeEnumerator<T>(this, step, false); }

		/// <summary>
		/// Returns an iterator which begins at the start of this range,
		/// adding the given step amount to the current value each iteration until the
		/// end is reached or passed. The start and end points are included
		/// or excluded according to this range. This method does not check for
		/// the availability of an addition operator at compile-time; if you use it
		/// on a range where there is no such operator, it will fail at execution time.
		/// </summary>
		/// <param name="stepAmount">Amount to add on each iteration</param>
		[NotNull]
		public LambdaRangeEnumerator<T> UpBy(T stepAmount)
		{
			return new LambdaRangeEnumerator<T>(this, t => Operator<T>.Add(t, stepAmount));
		}

		/// <summary>
		/// Returns an iterator which begins at the end of this range,
		/// subtracting the given step amount to the current value each iteration until the
		/// start is reached or passed. The start and end points are included
		/// or excluded according to this range. This method does not check for
		/// the availability of a subtraction operator at compile-time; if you use it
		/// on a range where there is no such operator, it will fail at execution time.
		/// </summary>
		/// <param name="stepAmount">
		/// Amount to subtract on each iteration. Note that
		/// this is subtracted, so in a range [0,10] you would pass +2 as this parameter
		/// to obtain the sequence (10, 8, 6, 4, 2, 0).
		/// </param>
		[NotNull]
		public LambdaRangeEnumerator<T> DownBy(T stepAmount)
		{
			return new LambdaRangeEnumerator<T>(this, t => Operator<T>.Subtract(t, stepAmount), false);
		}

		/// <summary>
		/// Returns an iterator which begins at the start of this range,
		/// adding the given step amount to the current value each iteration until the
		/// end is reached or passed. The start and end points are included
		/// or excluded according to this range. This method does not check for
		/// the availability of an addition operator at compile-time; if you use it
		/// on a range where there is no such operator, it will fail at execution time.
		/// </summary>
		/// <param name="stepAmount">Amount to add on each iteration</param>
		[NotNull]
		public LambdaRangeEnumerator<T> UpBy<TAmount>(TAmount stepAmount)
		{
			return new LambdaRangeEnumerator<T>(this, t => Operator<TAmount, T>.Add(t, stepAmount));
		}

		/// <summary>
		/// Returns an iterator which begins at the end of this range,
		/// subtracting the given step amount to the current value each iteration until the
		/// start is reached or passed. The start and end points are included
		/// or excluded according to this range. This method does not check for
		/// the availability of a subtraction operator at compile-time; if you use it
		/// on a range where there is no such operator, it will fail at execution time.
		/// </summary>
		/// <param name="stepAmount">
		/// Amount to subtract on each iteration. Note that
		/// this is subtracted, so in a range [0,10] you would pass +2 as this parameter
		/// to obtain the sequence (10, 8, 6, 4, 2, 0).
		/// </param>
		[NotNull]
		public LambdaRangeEnumerator<T> DownBy<TAmount>(TAmount stepAmount)
		{
			return new LambdaRangeEnumerator<T>(this, t => Operator<TAmount, T>.Subtract(t, stepAmount), false);
		}

		/// <summary>
		/// Returns an iterator which steps through the range, applying the specified
		/// step delegate on each iteration. The method determines whether to begin
		/// at the start or end of the range based on whether the step delegate appears to go
		/// "up" or "down". The step delegate is applied to the start point. If the result is
		/// more than the start point, the returned iterator begins at the start point; otherwise
		/// it begins at the end point.
		/// </summary>
		/// <param name="step">Delegate to apply to the "current value" on each iteration</param>
		[NotNull]
		public LambdaRangeEnumerator<T> Step([NotNull] Func<T, T> step)
		{
			bool ascending = Comparer.Compare(Minimum, step(Minimum)) < 0;
			return ascending ? FromStart(step) : FromEnd(step);
		}

		/// <summary>
		/// Returns an iterator which steps through the range, adding the specified amount
		/// on each iteration. If the step amount is logically negative, the returned iterator
		/// begins at the start point; otherwise it begins at the end point.
		/// This method does not check for
		/// the availability of an addition operator at compile-time; if you use it
		/// on a range where there is no such operator, it will fail at execution time.
		/// </summary>
		/// <param name="stepAmount">The amount to add on each iteration</param>
		[NotNull]
		public LambdaRangeEnumerator<T> Step(T stepAmount)
		{
			return Step(t => Operator<T>.Add(t, stepAmount));
		}

		/// <summary>
		/// Returns an iterator which steps through the range, adding the specified amount
		/// on each iteration. If the step amount is logically negative, the returned iterator
		/// begins at the end point; otherwise it begins at the start point. This method
		/// is equivalent to Step(T stepAmount), but allows an alternative type to be used.
		/// The most common example of this is likely to be stepping a range of DateTimes
		/// by a TimeSpan.
		/// This method does not check for
		/// the availability of a suitable addition operator at compile-time; if you use it
		/// on a range where there is no such operator, it will fail at execution time.
		/// </summary>
		/// <param name="stepAmount">The amount to add on each iteration</param>
		[NotNull]
		public LambdaRangeEnumerator<T> Step<TAmount>(TAmount stepAmount)
		{
			return Step(t => Operator<TAmount, T>.Add(t, stepAmount));
		}
	}

	public static class LambdaRangeExtension
	{
		/// <summary>
		/// Returns a RangeIterator over the given range, where the stepping function
		/// is to step by the given number of characters.
		/// </summary>
		/// <param name="range">The range to create an iterator for</param>
		/// <param name="step">How many characters to step each time</param>
		/// <returns>A RangeIterator with a suitable stepping function</returns>
		[NotNull]
		public static LambdaRangeEnumerator<char> StepChar([NotNull] this LambdaRange<char> range, int step)
		{
			return range.Step(c => (char)(c + step));
		}

		[NotNull]
		public static LambdaRange<T> AsLambdaRange<T>([NotNull] IReadOnlyRange<T> value)
			where T : struct, IComparable
		{
			return new LambdaRange<T>(value);
		}
	}
}