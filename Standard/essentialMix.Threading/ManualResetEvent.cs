using System.Threading;

namespace essentialMix.Threading
{
	public sealed class ManualResetEvent : EventWaitHandleBase
	{
		/// <inheritdoc />
		public ManualResetEvent()
			: this(false)
		{
		}

		/// <inheritdoc />
		public ManualResetEvent(bool initialState)
			: base(initialState, EventResetMode.ManualReset)
		{
		}
	}
}