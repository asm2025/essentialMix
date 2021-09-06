using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[Flags]
	[UnmanagedName("MFASF_SPLITTERFLAGS")]
	public enum MFASFSplitterFlags
	{
		None = 0,
		Reverse = 0x00000001,
		WMDRM = 0x00000002
	}
}