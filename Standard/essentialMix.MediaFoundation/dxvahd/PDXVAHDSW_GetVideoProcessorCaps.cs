using System;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_GetVideoProcessorCaps(IntPtr hDevice,
		DXVAHD_CONTENT_DESC pContentDesc,
		DXVAHD_DEVICE_USAGE Usage,
		int Count,
		DXVAHD_VPCAPS[] pCaps);
}