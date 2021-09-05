using System;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_GetVideoProcessorOutputFormats(IntPtr hDevice,
		DXVAHD_CONTENT_DESC pContentDesc,
		DXVAHD_DEVICE_USAGE Usage,
		int Count,
		int[] pFormats // D3DFORMAT
	);
}