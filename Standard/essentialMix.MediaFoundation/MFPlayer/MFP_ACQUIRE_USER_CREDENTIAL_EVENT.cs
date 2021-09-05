using System;
using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFP_ACQUIRE_USER_CREDENTIAL_EVENT")]
	public class MFP_ACQUIRE_USER_CREDENTIAL_EVENT : MFP_EVENT_HEADER
	{
		public IntPtr dwUserData;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fProceedWithAuthentication;
		public int hrAuthenticationStatus;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pwszURL;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pwszSite;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pwszRealm;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pwszPackage;
		public int nRetries;
		public MFP_CREDENTIAL_FLAGS flags;
		public IMFNetCredential pCredential;
	}
}