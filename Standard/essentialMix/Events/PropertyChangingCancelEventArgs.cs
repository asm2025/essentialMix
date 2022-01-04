namespace essentialMix.Events;

public class PropertyChangingEventArgs : System.ComponentModel.PropertyChangingEventArgs
{
	public PropertyChangingEventArgs(string propertyName)
		: base(propertyName) { }

	public bool Cancel { get; set; }
}

public class PropertyChangingEventArgs<T> : PropertyChangingEventArgs
{
	public PropertyChangingEventArgs(string propertyName, T value, T newValue)
		: base(propertyName)
	{
		Value = value;
		NewValue = newValue;
	}

	public T Value { get; }

	public T NewValue { get; }
}