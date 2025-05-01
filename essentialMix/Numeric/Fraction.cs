using System;
using JetBrains.Annotations;

namespace essentialMix.Numeric;

public readonly struct Fraction<T>(T numerator, T denominator)
	where T : struct, IComparable
{
	[NotNull]
	public override string ToString() { return $"{Numerator}:{Denominator}"; }

	public T Numerator { get; } = numerator;
	public T Denominator { get; } = denominator;
}