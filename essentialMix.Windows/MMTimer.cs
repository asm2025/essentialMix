using System;
using essentialMix.Patterns.Object;

namespace essentialMix.Windows;

// bug: something wrong with this timer. The second time it's called causes a NullReferenceException
public class MMTimer : Disposable
{
	private uint _id;

	private bool _enabled;

	/// <inheritdoc />
	public MMTimer()
		: this(0)
	{
	}

	/// <inheritdoc />
	public MMTimer(uint interval)
	{
		Interval = interval;
	}

	public event EventHandler Tick;

	public bool Enabled
	{
		get => _enabled;
		set
		{
			if (_enabled == value) return;
			_enabled = value;

			if (_enabled)
				Start(Interval, true);
			else
				Stop();
		}
	}

	public uint Interval { get; set; }

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing) Stop();
		base.Dispose(disposing);
	}

	protected virtual void OnTick(EventArgs e)
	{
		Tick?.Invoke(this, e);
	}

	private void Start(uint ms, bool repeat)
	{
		Stop();
		fuEvent f = fuEvent.TIME_CALLBACK_FUNCTION | (repeat ? fuEvent.TIME_PERIODIC : fuEvent.TIME_ONESHOT);

		lock (this)
		{
			_id = Win32.timeSetEvent(ms, 0, TimerCB, UIntPtr.Zero, f);
		}

		if (_id == 0) throw new Exception("Could not set timer.");
	}

	private void Stop()
	{
		lock (this)
		{
			if (_id != 0)
			{
				Win32.timeKillEvent(_id);
				_id = 0;
			}
		}
	}

	private void TimerCB(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
	{
		//Callback from the MMTimer API that fires the Timer event. Note we are in a different thread here
		OnTick(EventArgs.Empty);
	}
}