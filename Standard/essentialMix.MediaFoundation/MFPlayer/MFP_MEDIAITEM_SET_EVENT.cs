using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFP_MEDIAITEM_SET_EVENT")]
	public class MFP_MEDIAITEM_SET_EVENT : MFP_EVENT_HEADER
	{
		public IMFPMediaItem pMediaItem;
	}
}