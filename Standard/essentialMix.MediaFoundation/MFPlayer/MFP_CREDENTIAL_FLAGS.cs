using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[Flags]
	[UnmanagedName("MFP_CREDENTIAL_FLAGS")]
	public enum MFP_CREDENTIAL_FLAGS
	{
		None = 0x00000000,
		Prompt = 0x00000001,
		Save = 0x00000002,
		DoNotCache = 0x00000004,
		ClearText = 0x00000008,
		Proxy = 0x00000010,
		LoggedOnUser = 0x00000020
	}
}