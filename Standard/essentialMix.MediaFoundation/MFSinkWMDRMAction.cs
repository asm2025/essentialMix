using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation
{
	[UnmanagedName("MFSINK_WMDRMACTION")]
	public enum MFSinkWMDRMAction
	{
		Undefined = 0,
		Encode = 1,
		Transcode = 2,
		Transcrypt = 3,
		Last = 3
	}
}