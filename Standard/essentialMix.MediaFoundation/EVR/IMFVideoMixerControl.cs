using System.Runtime.InteropServices;
using System.Security;

namespace essentialMix.MediaFoundation.EVR
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("A5C6C53F-C202-4AA5-9695-175BA8C508A5")]
	public interface IMFVideoMixerControl
	{
		[PreserveSig]
		int SetStreamZOrder([In] int dwStreamID,
			[In] int dwZ);

		[PreserveSig]
		int GetStreamZOrder([In] int dwStreamID,
			out int pdwZ);

		[PreserveSig]
		int SetStreamOutputRect([In] int dwStreamID,
			[In] MFVideoNormalizedRect pnrcOutput);

		[PreserveSig]
		int GetStreamOutputRect([In] int dwStreamID,
			[Out][MarshalAs(UnmanagedType.LPStruct)]
			MFVideoNormalizedRect pnrcOutput);
	}
}