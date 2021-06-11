using System;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Threading
{
	public class TimedCallback : Disposable
	{
		private readonly Action<TimedCallback> _action;
		private uint _timerId;
		private uint _interval;

		/// <inheritdoc />
		public TimedCallback([NotNull] Action<TimedCallback> action, uint interval)
		{
			if (interval < 1) throw new ArgumentOutOfRangeException(nameof(interval));
			_action = action;
			_interval = interval;
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing) Enabled = false;
			base.Dispose(disposing);
		}

		public bool Enabled
		{
			get => _timerId != 0;
			set
			{
				if (value)
				{
					if (_timerId != 0)
					{
						Win32.timeKillEvent(_timerId);
						Win32.timeEndPeriod(1);
					}

					Win32.timeBeginPeriod(1);
					_timerId = Win32.timeSetEvent(Interval, 0, TimerProc, UIntPtr.Zero, fuEvent.TIME_PERIODIC);
					return;
				}

				if (_timerId == 0) return;
				Win32.timeEndPeriod(1);
				Win32.timeKillEvent(_timerId);
				_timerId = 0;
			}
		}

		public uint Interval
		{
			get => _interval;
			set
			{
				if (value < 1) throw new ArgumentOutOfRangeException(nameof(value));
				_interval = value;
			}
		}

		private void TimerProc(uint uTimerId, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
		{
			if (uTimerId == 0) return;
			_action(this);
		}

		[NotNull]
		public static TimedCallback Create([NotNull] Action<TimedCallback> action, uint interval)
		{
			return new TimedCallback(action, interval)
			{
				Enabled = true
			};
		}
	}
}