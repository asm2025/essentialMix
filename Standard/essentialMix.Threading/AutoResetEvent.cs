using System.Threading;

namespace essentialMix.Threading
{
	public sealed class AutoResetEvent : EventWaitHandleBase
	{
		/// <inheritdoc />
		public AutoResetEvent()
			: this(false)
		{
		}

		/// <inheritdoc />
		public AutoResetEvent(bool initialState)
			: base(initialState, EventResetMode.AutoReset)
		{
		}
	}
}
