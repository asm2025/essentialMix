using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace essentialMix.Threading;

[HostProtection(Synchronization=true, ExternalThreading=true)]
[ComVisible(true)]	
public sealed class NativeWaitHandle : WaitHandle
{
	public NativeWaitHandle(IntPtr handle, bool isOwning)
	{
		SafeWaitHandle = new SafeWaitHandle(handle, isOwning);
	}
}