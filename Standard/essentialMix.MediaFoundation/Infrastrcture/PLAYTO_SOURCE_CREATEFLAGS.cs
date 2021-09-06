using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[Flags]
	[UnmanagedName("PLAYTO_SOURCE_CREATEFLAGS")]
	public enum PLAYTO_SOURCE_CREATEFLAGS
	{
		None = 0x0,
		Image = 0x1,
		Audio = 0x2,
		Video = 0x4,
		Protected = 0x8,
	}
}