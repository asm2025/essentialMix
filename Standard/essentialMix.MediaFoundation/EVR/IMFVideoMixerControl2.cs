using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("8459616D-966E-4930-B658-54FA7E5A16D3")]
	public interface IMFVideoMixerControl2 : IMFVideoMixerControl
	{
		#region IMFVideoMixerControl methods

		[PreserveSig]
		new int SetStreamZOrder([In] int dwStreamID,
			[In] int dwZ);

		[PreserveSig]
		new int GetStreamZOrder([In] int dwStreamID,
			out int pdwZ);

		[PreserveSig]
		new int SetStreamOutputRect([In] int dwStreamID,
			[In] MFVideoNormalizedRect pnrcOutput);

		[PreserveSig]
		new int GetStreamOutputRect([In] int dwStreamID,
			[Out][MarshalAs(UnmanagedType.LPStruct)]
			MFVideoNormalizedRect pnrcOutput);

		#endregion

		[PreserveSig]
		int SetMixingPrefs([In] MFVideoMixPrefs dwMixFlags);

		[PreserveSig]
		int GetMixingPrefs(out MFVideoMixPrefs pdwMixFlags);
	}
}