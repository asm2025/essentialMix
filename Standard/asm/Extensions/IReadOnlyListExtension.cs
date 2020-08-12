using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm.Extensions
{
	public static class IReadOnlyListExtension
	{
		[NotNull]
		public static Type[] Types([NotNull] this IReadOnlyList<object> thisValue)
		{
			return thisValue.Count == 0 ? Type.EmptyTypes : thisValue.Select(item => item.AsType()).ToArray();
		}

		public static T PickRandom<T>([NotNull] this IReadOnlyList<T> thisValue, int startIndex = 0, int count = -1)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0) throw new InvalidOperationException("List is empty.");

			int max;
			int n;

			if (thisValue is ICollection collection)
			{
				lock (collection.SyncRoot)
				{
					max = count - 1;
					if (max < 0) throw new InvalidOperationException("List is empty.");
					n = RNGRandomHelper.Next(startIndex, max);
					return thisValue[n];
				}
			}

			lock(thisValue)
			{
				max = count - 1;
				n = RNGRandomHelper.Next(startIndex, max);
				return thisValue[n];
			}
		}
	}
}