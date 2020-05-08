using System;
using System.Runtime.CompilerServices;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class ArrayHelper
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static T[] ValidateAndGetRange<T>([NotNull] T[] value, ref int startIndex, ref int count)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			value.Length.ValidateRange(startIndex, ref count);
			if (startIndex == 0 && count == value.Length) return value;
			T[] range = new T[count];
			Array.Copy(value, startIndex, range, 0, count);
			return range;
		}
	}
}