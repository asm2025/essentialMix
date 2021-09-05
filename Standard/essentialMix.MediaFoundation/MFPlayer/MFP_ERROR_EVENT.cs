using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFP_ERROR_EVENT")]
	public class MFP_ERROR_EVENT : MFP_EVENT_HEADER
	{
	}
}