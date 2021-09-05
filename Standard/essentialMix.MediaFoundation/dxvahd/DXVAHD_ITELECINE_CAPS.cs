using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[Flags]
	[UnmanagedName("DXVAHD_ITELECINE_CAPS")]
	public enum DXVAHD_ITELECINE_CAPS
	{
		CAPS_32 = 0x1,
		CAPS_22 = 0x2,
		CAPS_2224 = 0x4,
		CAPS_2332 = 0x8,
		CAPS_32322 = 0x10,
		CAPS_55 = 0x20,
		CAPS_64 = 0x40,
		CAPS_87 = 0x80,
		CAPS_222222222223 = 0x100,
		CAPS_OTHER = unchecked((int)0x80000000)
	}
}