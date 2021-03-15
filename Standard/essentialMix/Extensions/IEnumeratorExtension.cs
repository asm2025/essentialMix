using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class IEnumeratorExtension
	{
		public static IEnumerator<T> CastTo<T>([NotNull] this IEnumerator thisValue)
		{
			while (thisValue.MoveNext())
			{
				yield return (T)thisValue.Current;
			}
		}
	}
}