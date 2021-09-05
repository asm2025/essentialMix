using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[Flags]
	[UnmanagedName("EVRFilterConfigPrefs")]
	public enum EVRFilterConfigPrefs
	{
		None = 0,
		EnableQoS = 0x1,
		Mask = 0x1
	}
}