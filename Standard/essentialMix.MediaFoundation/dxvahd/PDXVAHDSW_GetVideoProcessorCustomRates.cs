using System;
using System.Runtime.InteropServices;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_GetVideoProcessorCustomRates(IntPtr hDevice,
		[In][MarshalAs(UnmanagedType.LPStruct)]
		Guid pVPGuid,
		int Count,
		DXVAHD_CUSTOM_RATE_DATA[] pRates);
}