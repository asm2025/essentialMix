using System;
using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFT_REGISTRATION_INFO")]
	public struct MFT_REGISTRATION_INFO
	{
		public Guid clsid;
		public Guid guidCategory;
		public MFT_EnumFlag uiFlags;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszName;

		public int cInTypes;
		public MFTRegisterTypeInfo[] pInTypes;
		public int cOutTypes;
		public MFTRegisterTypeInfo[] pOutTypes;
	}
}