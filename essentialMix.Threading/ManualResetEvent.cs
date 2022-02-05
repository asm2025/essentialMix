using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace essentialMix.Threading;

[HostProtection(Synchronization=true, ExternalThreading=true)]
[ComVisible(true)]	
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