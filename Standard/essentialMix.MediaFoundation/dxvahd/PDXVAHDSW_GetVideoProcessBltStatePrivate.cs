using System;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_GetVideoProcessBltStatePrivate(IntPtr hVideoProcessor,
		ref DXVAHD_BLT_STATE_PRIVATE_DATA pData);
}