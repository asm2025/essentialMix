using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_CUSTOM_RATE_DATA")]
	public struct DXVAHD_CUSTOM_RATE_DATA
	{
		public DXVAHD_RATIONAL CustomRate;
		public int OutputFrames;

		[MarshalAs(UnmanagedType.Bool)]
		public bool InputInterlaced;

		public int InputFramesOrFields;
	}
}