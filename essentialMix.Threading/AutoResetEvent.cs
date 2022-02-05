using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace essentialMix.Threading;

[HostProtection(Synchronization=true, ExternalThreading=true)]
[ComVisible(true)]	
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