using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("46a30204-a696-4b18-8804-246b8f031bb1")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEngineNeedKeyNotify
	{
		[PreserveSig]
		void NeedKey(
			IntPtr initData,
			int cb
		);
	}
}