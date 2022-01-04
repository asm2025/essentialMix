using System;

namespace essentialMix.Events;

[Serializable]
public class EventArgs<T> : ReadOnlyEventArgs<T>
{
	/// <inheritdoc />
	public EventArgs()
	{
	}

	/// <inheritdoc />
	public EventArgs(T value)
		: base(value)
	{
	}

	public new virtual T Value { get; set; }
}