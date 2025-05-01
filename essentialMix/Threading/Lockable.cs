using JetBrains.Annotations;

namespace essentialMix.Threading;

public class Lockable<T>(T value)
	where T : class
{
	[NotNull]
	private readonly object _syncRoot = new object();

	private T _value = value;

	/// <inheritdoc />
	public Lockable() 
		: this(null)
	{
	}

	public T Value
	{
		get => _value;
		set
		{
			lock(_syncRoot)
			{
				_value = value;
			}
		}
	}

	public static implicit operator T([NotNull] Lockable<T> lockable) { return lockable.Value; }
}