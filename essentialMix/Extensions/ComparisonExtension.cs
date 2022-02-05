using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Other.JonSkeet.MiscUtil.Collections;

namespace essentialMix.Extensions;

public static class ComparisonExtension
{
	[NotNull]
	public static IEqualityComparer<T> Create<T>([NotNull] this Comparison<T> comparison) { return new ComparisonComparer<T>(comparison); }
}