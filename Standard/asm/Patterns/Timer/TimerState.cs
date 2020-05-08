using JetBrains.Annotations;

namespace asm.Patterns.Timer
{
	public class TimerState
	{
		public TimerState()
			: this(null)
		{
		}

		public TimerState(System.Threading.Timer timer)
		{
			Timer = timer;
		}

		public bool Canceled { get; private set; }
		public System.Threading.Timer Timer { get; set; }

		public void Cancel() { Canceled = true; }
	}

	public class TimerState<T> : TimerState
	{
		public TimerState()
			: this(null, default)
		{
		}

		public TimerState(System.Threading.Timer timer)
			: this(timer, default)
		{
		}

		public TimerState(System.Threading.Timer timer, T value)
			: base(timer)
		{
			Value = value;
		}

		public T Value { get; set; }

		public static implicit operator T([NotNull] TimerState<T> timerState) { return timerState.Value; }
	}
}