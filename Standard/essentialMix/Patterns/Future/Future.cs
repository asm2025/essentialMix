using System;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Future;

/// <inheritdoc />
/// <summary>
/// Poor-man's version of a Future. This wraps a result which *will* be
/// available in the future. It's up to the caller/provider to make sure
/// that the value has been specified by the time it's requested.
/// </summary>
public class Future<T> : IFuture<T>
{
	private T _value;

	public Future()
	{
	}

	/// <summary>
	/// Returns a string representation of the value if available, null otherwise
	/// </summary>
	/// <returns>A string representation of the value if available, null otherwise</returns>
	[NotNull]
	public override string ToString()
	{
		return HasValue
					? Convert.ToString(_value)
					: string.Empty;
	}

	/// <inheritdoc />
	/// <summary>
	/// Returns the value of the future, once it has been set
	/// </summary>
	/// <exception cref="T:System.InvalidOperationException">If the value is not yet available</exception>
	public T Value
	{
		get
		{
			if (!HasValue) throw new InvalidOperationException("No value has been set yet");
			return _value;
		}
		set
		{
			if (HasValue) throw new InvalidOperationException("Value has already been set");
			HasValue = true;
			_value = value;
		}
	}

	public bool HasValue { get; private set; }
}