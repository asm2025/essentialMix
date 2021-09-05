using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFP_FRAME_STEP_EVENT")]
	public class MFP_FRAME_STEP_EVENT : MFP_EVENT_HEADER
	{
		public IMFPMediaItem pMediaItem;
	}
}