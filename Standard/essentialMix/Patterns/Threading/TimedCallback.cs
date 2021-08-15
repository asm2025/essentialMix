using System;
using System.Threading;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Threading
{
	public class TimedCallback : Disposable
	{
		private readonly Action<TimedCallback> _action;

		// MUST keep a reference to the timer callback function to prevent GC from collecting it.
		private TimerCallback _timerCallback;
		private uint _timerId;
		private int _interval;
		private AutoResetEvent _event;

		/// <inheritdoc />
		public TimedCallback([NotNull] Action<TimedCallback> action, int interval)
		{
			if (interval < 1) throw new ArgumentOutOfRangeException(nameof(interval));
			_action = action;
			_interval = interval;
			_timerCallback = TimerProc;
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Enabled = false;
				ObjectHelper.Dispose(ref _event);
			}

			_timerCallback = null;
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

					if (IsDisposed) return;
					Win32.timeBeginPeriod(1);
					_timerId = Win32.timeSetEvent((uint)Interval, 0, _timerCallback, UIntPtr.Zero, fuEvent.TIME_PERIODIC);
					return;
				}

				if (_timerId == 0) return;
				Win32.timeEndPeriod(1);
				Win32.timeKillEvent(_timerId);
				_timerId = 0;
			}
		}

		public int Interval
		{
			get => _interval;
			set
			{
				if (value < 1) throw new ArgumentOutOfRangeException(nameof(value));
				_interval = value;
			}
		}

		public bool Wait() { return Wait(TimeSpanHelper.INFINITE, false); }
		public bool Wait(int millisecondsTimeout) { return Wait(millisecondsTimeout, false); }
		public bool Wait(int millisecondsTimeout, bool exitContext)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (_event == null) Interlocked.CompareExchange(ref _event, new AutoResetEvent(false), null);
			return _event.WaitOne(millisecondsTimeout, exitContext);
		}

		private void TimerProc(uint uTimerId, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
		{
			if (uTimerId == 0 || IsDisposed) return;
			_action(this);
			_event?.Set();
		}

		[NotNull]
		public static TimedCallback Create([NotNull] Action<TimedCallback> action, int interval, bool enabled = true)
		{
			return new TimedCallback(action, interval)
			{
				Enabled = enabled
			};
		}
	}
}