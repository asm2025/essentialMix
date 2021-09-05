using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_STREAM_STATE_PRIVATE_IVTC_DATA")]
	public struct DXVAHD_STREAM_STATE_PRIVATE_IVTC_DATA
	{
		[MarshalAs(UnmanagedType.Bool)]
		public bool Enable;

		public int ITelecineFlags;
		public int Frames;
		public int InputField;
	}
}