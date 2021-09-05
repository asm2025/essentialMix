using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFP_PAUSE_EVENT")]
	public class MFP_PAUSE_EVENT : MFP_EVENT_HEADER
	{
		public IMFPMediaItem pMediaItem;
	}
}