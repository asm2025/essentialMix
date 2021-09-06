using System;
using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("3D1FF0EA-679A-4190-8D46-7FA69E8C7E15")]
	public interface IMFDRMNetHelper
	{
		[PreserveSig]
		int ProcessLicenseRequest(
			[In] IntPtr pLicenseRequest,
			[In] int cbLicenseRequest,
			[Out] out IntPtr ppLicenseResponse,
			out int pcbLicenseResponse,
			[MarshalAs(UnmanagedType.BStr)] out string pbstrKID
		);

		[PreserveSig]
		int GetChainedLicenseResponse(
			[Out] out IntPtr ppLicenseResponse,
			out int pcbLicenseResponse
		);
	}
}