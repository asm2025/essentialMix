using System;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class RandomExtension
{
	public static bool NextBool([NotNull] this Random thisValue)
	{
		lock(thisValue)
		{
			return thisValue.Next(0, 1) == 1;
		}
	}
}