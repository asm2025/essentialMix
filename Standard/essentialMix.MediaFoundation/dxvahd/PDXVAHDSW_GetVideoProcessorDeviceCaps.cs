using System;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_GetVideoProcessorDeviceCaps(IntPtr hDevice,
		DXVAHD_CONTENT_DESC pContentDesc,
		DXVAHD_DEVICE_USAGE Usage,
		out DXVAHD_VPDEVCAPS pCaps);
}