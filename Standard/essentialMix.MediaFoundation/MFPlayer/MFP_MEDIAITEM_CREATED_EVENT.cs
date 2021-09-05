using System;
using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFP_MEDIAITEM_CREATED_EVENT")]
	public class MFP_MEDIAITEM_CREATED_EVENT : MFP_EVENT_HEADER
	{
		public IMFPMediaItem pMediaItem;
		public IntPtr dwUserData;
	}
}