using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Converters;

public abstract class GenericConverterBase<TSource, TTarget> : IValueConverter
{
	private static TSource __trueSourceValue;
	private static TSource __falseSourceValue;
	private static volatile IReadOnlySet<TSource> __trueValues;
	private static volatile IReadOnlySet<TSource> __falseValues;

	protected GenericConverterBase()
	{
	}

	[NotNull]
	protected TSource TrueSourceValue => __trueSourceValue ??= TrueValues.First();

	[NotNull]
	protected TSource FalseSourceValue => __falseSourceValue ??= FalseValues.First();

	[NotNull]
	protected abstract TTarget TrueValue { get; }

	[NotNull]
	protected abstract TTarget FalseValue { get; }

	[NotNull]
	protected IReadOnlySet<TSource> TrueValues
	{
		get
		{
			Thread.MemoryBarrier();
			if (__trueValues != null) return __trueValues;
			Interlocked.CompareExchange(ref __trueValues, GetTrueValues(), null);
			if (__trueValues!.Count == 0) throw new InvalidOperationException($"Values are set correctly for {GetType()}.");
			return __trueValues;
		}
	}

	[NotNull]
	protected IReadOnlySet<TSource> FalseValues
	{
		get
		{
			Thread.MemoryBarrier();
			if (__falseValues != null) return __falseValues;
			Interlocked.CompareExchange(ref __falseValues, GetFalseValues(), null);
			if (__falseValues!.Count == 0) throw new InvalidOperationException($"Values are set correctly for {GetType()}.");
			return __falseValues;
		}
	}

	/// <inheritdoc />
	[NotNull]
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is TSource source)
		{
			if (TrueValues.Contains(source)) return TrueValue;
			if (FalseValues.Contains(source)) return FalseValue;
		}
		throw new FormatException($"Value {value} was not recognized as a valid {typeof(TSource)}.");
	}

	/// <inheritdoc />
	[NotNull]
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is TTarget v)
		{
			EqualityComparer<TTarget> comparer = EqualityComparer<TTarget>.Default;
			if (comparer.Equals(v, TrueValue)) return TrueSourceValue;
			if (comparer.Equals(v, FalseValue)) return FalseSourceValue;
		}

		throw new FormatException($"Value {value} was not recognized as a valid {typeof(TTarget)}.");
	}

	[NotNull]
	protected abstract IReadOnlySet<TSource> GetTrueValues();

	[NotNull]
	protected abstract IReadOnlySet<TSource> GetFalseValues();
}