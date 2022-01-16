using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Converters;

public abstract class BooleanConverterBase<T> : IValueConverter
{
	private static T __trueValue;
	private static T __falseValue;
	private static volatile IReadOnlySet<T> __trueValues;
	private static volatile IReadOnlySet<T> __falseValues;

	protected BooleanConverterBase()
	{
	}

	[NotNull]
	protected T TrueValue => __trueValue ??= TrueValues.First();

	[NotNull]
	protected T FalseValue => __falseValue ??= FalseValues.First();

	[NotNull]
	protected IReadOnlySet<T> TrueValues
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
	protected IReadOnlySet<T> FalseValues
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
		return value is true
					? TrueValue
					: FalseValue;
	}

	/// <inheritdoc />
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is T v)
		{
			if (TrueValues.Contains(v)) return true;
			if (FalseValues.Contains(v)) return false;
		}

		throw new FormatException($"Value {value} was not recognized as a valid Boolean.");
	}

	[NotNull]
	protected abstract IReadOnlySet<T> GetTrueValues();

	[NotNull]
	protected abstract IReadOnlySet<T> GetFalseValues();
}