using System.Runtime.InteropServices;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[ComImport]
	[System.Security.SuppressUnmanagedCodeSecurity]
	[Guid("AA9DD80F-C50A-4220-91C1-332287F82A34")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPlayToControlWithCapabilities : IPlayToControl
	{
		#region IPlayToControl methods

		[PreserveSig]
		new int Connect(
			IMFSharingEngineClassFactory pFactory
		);

		[PreserveSig]
		new int Disconnect();

		#endregion

		[PreserveSig]
		int GetCapabilities(
			out PLAYTO_SOURCE_CREATEFLAGS pCapabilities
		);
	}
}