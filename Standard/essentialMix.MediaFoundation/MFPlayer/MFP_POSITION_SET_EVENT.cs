using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFP_POSITION_SET_EVENT")]
	public class MFP_POSITION_SET_EVENT : MFP_EVENT_HEADER
	{
		public IMFPMediaItem pMediaItem;
	}
}