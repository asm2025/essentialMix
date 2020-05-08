using JetBrains.Annotations;
using asm.Collections;

namespace asm.Extensions
{
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
	}
}