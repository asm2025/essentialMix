using System.ComponentModel;

namespace essentialMix.Events;

public class PropertyChangedEventArgs<T>(string propertyName, T oldValue, T value)
	: PropertyChangedEventArgs(propertyName)
{
	public T OldValue { get; } = oldValue;

	public T Value { get; } = value;
}