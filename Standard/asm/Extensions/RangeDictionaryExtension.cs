using System;
using asm.Collections;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class RangeDictionaryExtension
	{
		[NotNull]
		public static ReadOnlyRangeDictionary<TKey, TValue> AsReadOnly<TKey, TValue>([NotNull] this RangeDictionary<TKey, TValue> thisValue)
			where TKey : IComparable
		{
			return new ReadOnlyRangeDictionary<TKey, TValue>(thisValue);
		}
	}
}