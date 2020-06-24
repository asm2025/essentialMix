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

		public static T GetRandom<T>([NotNull] this IReadOnlyList<T> thisValue)
		{
			int max;
			int n;

			if (thisValue is ICollection collection)
			{
				lock (collection.SyncRoot)
				{
					max = thisValue.Count - 1;
					if (max < 0) throw new InvalidOperationException("List is empty.");
					n = RNGRandomHelper.Next(0, max);
					return thisValue[n];
				}
			}

			lock(thisValue)
			{
				max = thisValue.Count - 1;
				n = RNGRandomHelper.Next(0, max);
				return thisValue[n];
			}
		}
	}
}