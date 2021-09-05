using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_STREAM_STATE_ASPECT_RATIO_DATA")]
	public struct DXVAHD_STREAM_STATE_ASPECT_RATIO_DATA
	{
		[MarshalAs(UnmanagedType.Bool)]
		public bool Enable;

		public DXVAHD_RATIONAL SourceAspectRatio;
		public DXVAHD_RATIONAL DestinationAspectRatio;
	}
}