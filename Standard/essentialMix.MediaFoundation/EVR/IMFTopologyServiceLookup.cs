using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("FA993889-4383-415A-A930-DD472A8CF6F7")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFTopologyServiceLookup
	{
		[PreserveSig]
		int LookupService([In] MFServiceLookupType type,
			[In] int dwIndex,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidService,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface)][Out]
			object[] ppvObjects,
			[In][Out] ref int pnObjects);
	}
}