using System;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_GetVideoProcessStreamStatePrivate(IntPtr hVideoProcessor,
		int StreamNumber,
		ref DXVAHD_STREAM_STATE_PRIVATE_DATA pData);
}