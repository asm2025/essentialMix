using System;
using System.Runtime.CompilerServices;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Helpers;

public static class ArrayHelper
{
	[NotNull]
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static T[] ValidateAndGetRange<T>([NotNull] T[] array, ref int startIndex, ref int count)
	{
		if (array == null) throw new ArgumentNullException(nameof(array));
		array.Length.ValidateRange(startIndex, ref count);
		if (startIndex == 0 && count == array.Length) return array;
		T[] range = new T[count];
		Array.Copy(array, startIndex, range, 0, count);
		return range;
	}
}