using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[Flags]
	[UnmanagedName("MFP_CREATION_OPTIONS")]
	public enum MFP_CREATION_OPTIONS
	{
		None = 0x00000000,
		FreeThreadedCallback = 0x00000001,
		NoMMCSS = 0x00000002,
		NoRemoteDesktopOptimization = 0x00000004
	}
}