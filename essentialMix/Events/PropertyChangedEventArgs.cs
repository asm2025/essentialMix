using System.ComponentModel;

namespace essentialMix.Events;

public class PropertyChangedEventArgs<T> : PropertyChangedEventArgs
{
	public PropertyChangedEventArgs(string propertyName, T oldValue, T value)
		: base(propertyName)
	{
		OldValue = oldValue;
		Value = value;
	}

	public T OldValue { get; }

	public T Value { get; }
}