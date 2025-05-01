namespace essentialMix.Events;

public class PropertyChangingEventArgs(string propertyName)
	: System.ComponentModel.PropertyChangingEventArgs(propertyName)
{
	public bool Cancel { get; set; }
}

public class PropertyChangingEventArgs<T>(string propertyName, T value, T newValue)
	: PropertyChangingEventArgs(propertyName)
{
	public T Value { get; } = value;

	public T NewValue { get; } = newValue;
}