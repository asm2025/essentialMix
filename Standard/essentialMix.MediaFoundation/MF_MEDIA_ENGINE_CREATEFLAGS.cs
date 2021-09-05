using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation
{
	[Flags]
	[UnmanagedName("MF_MEDIA_ENGINE_CREATEFLAGS")]
	public enum MF_MEDIA_ENGINE_CREATEFLAGS
	{
		None = 0,
		AudioOnly = 0x0001,
		WaitForStableState = 0x0002,
		ForceMute = 0x0004,
		RealTimeMode = 0x0008,
		DisableLocalPlugins = 0x0010,
		CreateFlagsMask = 0x001F
	}
}