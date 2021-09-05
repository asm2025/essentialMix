using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFP_MF_EVENT")]
	public class MFP_MF_EVENT : MFP_EVENT_HEADER
	{
		public MediaEventType MFEventType;
		public IMFMediaEvent pMFMediaEvent;
		public IMFPMediaItem pMediaItem;
	}
}