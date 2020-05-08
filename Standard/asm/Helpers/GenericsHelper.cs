using System;
using System.Collections.Generic;
using System.Reflection;
using asm.Extensions;
using asm.Internal;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class GenericsHelper
	{
		public static IEnumerable<T> Iterate<T>(T initial, [NotNull] Predicate<T> fnContinue, [NotNull] Func<T, T> fnNext)
		{
			for (T current = initial; fnContinue(current); current = fnNext(current))
				yield return current;
		}

		public static T CreateInstance<T>(params object[] args) { return (T)typeof(T).CreateInstance(args); }

		public static T CreateInstance<T>(BindingFlags bindingFlags, params object[] args) { return (T)typeof(T).CreateInstance(bindingFlags, args); }

		public static T[] ArrayOf<T>(params T[] values) { return values; }

		/// <summary>
		/// Increments the accumulator only
		/// if the value is non-null. If the accumulator
		/// is null, then the accumulator is given the new
		/// value; otherwise the accumulator and value
		/// are added.
		/// </summary>
		/// <param name="accumulator">The current total to be incremented (can be null)</param>
		/// <param name="value">The value to be tested and added to the accumulator</param>
		/// <returns>
		/// True if the value is non-null, else false - i.e.
		/// "has the accumulator been updated?"
		/// </returns>
		public static bool AddIfNotNull<T>(ref T accumulator, T value)
		{
			return Operator<T>.NullOp.AddIfNotNull(ref accumulator, value);
		}
	}
}