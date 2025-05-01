using JetBrains.Annotations;

namespace essentialMix.Patterns.Threading;

public class TimerState(System.Threading.Timer timer)
{
	public TimerState()
		: this(null)
	{
	}

	public bool Canceled { get; private set; }
	public System.Threading.Timer Timer { get; set; } = timer;

	public void Cancel() { Canceled = true; }
}

public class TimerState<T>(System.Threading.Timer timer, T value) : TimerState(timer)
{
	public TimerState()
		: this(null, default)
	{
	}

	public TimerState(System.Threading.Timer timer)
		: this(timer, default)
	{
	}

	public T Value { get; set; } = value;

	public static implicit operator T([NotNull] TimerState<T> timerState) { return timerState.Value; }
}