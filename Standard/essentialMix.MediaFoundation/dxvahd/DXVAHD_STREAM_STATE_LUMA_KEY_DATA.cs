using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_STREAM_STATE_LUMA_KEY_DATA")]
	public struct DXVAHD_STREAM_STATE_LUMA_KEY_DATA
	{
		[MarshalAs(UnmanagedType.Bool)]
		public bool Enable;

		public float Lower;
		public float Upper;
	}
}