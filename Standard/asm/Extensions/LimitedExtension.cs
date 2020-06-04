using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using asm.Collections;

namespace asm.Extensions
{
	public static class LimitedExtension
	{
		[NotNull]
		public static LimitedList AsLockable([NotNull] this IList thisValue) { return new LimitedList(thisValue); }

		[NotNull]
		public static LimitedList<T> AsLockable<T>([NotNull] this IList<T> thisValue) { return new LimitedList<T>(thisValue); }
	}
}