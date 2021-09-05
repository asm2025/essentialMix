using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_STREAM_STATE_INPUT_COLOR_SPACE_DATA")]
	public struct DXVAHD_STREAM_STATE_INPUT_COLOR_SPACE_DATA
	{
		public int Value;
	}
}