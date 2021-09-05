using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation
{
	[Flags]
	[UnmanagedName("ASF_STATUSFLAGS")]
	public enum ASFStatusFlags
	{
		None = 0,
		Incomplete = 0x1,
		NonfatalError = 0x2
	}
}