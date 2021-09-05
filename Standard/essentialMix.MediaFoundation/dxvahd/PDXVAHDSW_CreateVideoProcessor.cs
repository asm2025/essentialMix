using System;
using System.Runtime.InteropServices;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_CreateVideoProcessor(IntPtr hDevice,
		[In][MarshalAs(UnmanagedType.LPStruct)]
		Guid pVPGuid,
		out IntPtr phVideoProcessor);
}