using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHDSW_CALLBACKS")]
	public struct DXVAHDSW_CALLBACKS
	{
		public PDXVAHDSW_CreateDevice CreateDevice;
		public PDXVAHDSW_ProposeVideoPrivateFormat ProposeVideoPrivateFormat;
		public PDXVAHDSW_GetVideoProcessorDeviceCaps GetVideoProcessorDeviceCaps;
		public PDXVAHDSW_GetVideoProcessorOutputFormats GetVideoProcessorOutputFormats;
		public PDXVAHDSW_GetVideoProcessorInputFormats GetVideoProcessorInputFormats;
		public PDXVAHDSW_GetVideoProcessorCaps GetVideoProcessorCaps;
		public PDXVAHDSW_GetVideoProcessorCustomRates GetVideoProcessorCustomRates;
		public PDXVAHDSW_GetVideoProcessorFilterRange GetVideoProcessorFilterRange;
		public PDXVAHDSW_DestroyDevice DestroyDevice;
		public PDXVAHDSW_CreateVideoProcessor CreateVideoProcessor;
		public PDXVAHDSW_SetVideoProcessBltState SetVideoProcessBltState;
		public PDXVAHDSW_GetVideoProcessBltStatePrivate GetVideoProcessBltStatePrivate;
		public PDXVAHDSW_SetVideoProcessStreamState SetVideoProcessStreamState;
		public PDXVAHDSW_GetVideoProcessStreamStatePrivate GetVideoProcessStreamStatePrivate;
		public PDXVAHDSW_VideoProcessBltHD VideoProcessBltHD;
		public PDXVAHDSW_DestroyVideoProcessor DestroyVideoProcessor;
	}
}