using System;
using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFT_REGISTER_TYPE_INFO")]
	public class MFTRegisterTypeInfo
	{
		public Guid guidMajorType;
		public Guid guidSubtype;
	}
}