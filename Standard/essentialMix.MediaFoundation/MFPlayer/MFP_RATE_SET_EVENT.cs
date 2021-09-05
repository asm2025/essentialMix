using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFP_RATE_SET_EVENT")]
	public class MFP_RATE_SET_EVENT : MFP_EVENT_HEADER
	{
		public IMFPMediaItem pMediaItem;
		public float flRate;
	}
}