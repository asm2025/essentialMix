using System;
using JetBrains.Annotations;

namespace essentialMix.Numeric;

public readonly struct Fraction<T>
	where T : struct, IComparable
{
	public Fraction(T numerator, T denominator)
	{
		Numerator = numerator;
		Denominator = denominator;
	}

	[NotNull]
	public override string ToString() { return $"{Numerator}:{Denominator}"; }

	public T Numerator { get; }
	public T Denominator { get; }
}