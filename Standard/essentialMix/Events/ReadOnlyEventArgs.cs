using System;

namespace essentialMix.Events;

[Serializable]
public class ReadOnlyEventArgs<T> : EventArgs
{
	/// <inheritdoc />
	public ReadOnlyEventArgs()
	{
	}

	/// <inheritdoc />
	public ReadOnlyEventArgs(T value)
	{
		Value = value;
	}

	public virtual T Value { get; }
}