using System;

namespace essentialMix.MediaFoundation.dxvahd
{
	public delegate int PDXVAHDSW_GetVideoProcessorFilterRange(IntPtr hDevice,
		DXVAHD_FILTER Filter,
		out DXVAHD_FILTER_RANGE_DATA pRange);
}